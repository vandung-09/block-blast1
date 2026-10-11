using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Dựng 2 scene Home + Game: Block Blast > Build Scenes.
// Chạy lại được nhiều lần (xóa object cũ rồi dựng lại; camera/light giữ nguyên).
public static class BlockBlastSceneBuilder
{
    const string SpriteDir = "Assets/Sprites/";
    const string AudioDir = "Assets/Audio/";
    const string HomePath = "Assets/Scenes/Home.unity";
    const string GamePath = "Assets/Scenes/Game.unity";
    const float Cell = 118.5f;
    const int Size = 8;
    static readonly Vector2 BoardPos = new Vector2(0, 120);
    static readonly Vector2[] SlotPos = { new Vector2(-340, -620), new Vector2(0, -620), new Vector2(340, -620) };
    static readonly string[] RootNames =
    {
        "GameManager", "AudioManager", "PieceSpawner", "DragController", "HomeManager",
        "Canvas", "EventSystem", "BlockBlast"
    };

    static Font font;

    // Dùng cho batchmode: -executeMethod BlockBlastSceneBuilder.BuildBatch
    public static void BuildBatch() => Build();

    [MenuItem("Block Blast/Build Scenes")]
    public static void Build()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        MergeSlicedSprites();

        // SampleScene -> Home, rồi nhân bản thành Game (để có sẵn camera + light 2D).
        if (!System.IO.File.Exists(HomePath) && System.IO.File.Exists("Assets/Scenes/SampleScene.unity"))
            AssetDatabase.MoveAsset("Assets/Scenes/SampleScene.unity", HomePath);
        if (!System.IO.File.Exists(GamePath)) AssetDatabase.CopyAsset(HomePath, GamePath);

        var home = EditorSceneManager.OpenScene(HomePath);
        ClearOld();
        BuildHome();
        EditorSceneManager.SaveScene(home);

        var game = EditorSceneManager.OpenScene(GamePath);
        ClearOld();
        BuildGame();
        EditorSceneManager.SaveScene(game);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(HomePath, true),
            new EditorBuildSettingsScene(GamePath, true),
        };
        EditorSceneManager.OpenScene(HomePath);
        Debug.Log("Block Blast: đã dựng xong scene Home và Game.");
    }

    // Vài ảnh (combo_*, icon_sound...) bị cắt thành nhiều sprite con -> chuyển về Single để dùng nguyên ảnh.
    static void MergeSlicedSprites()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { SpriteDir.TrimEnd('/') }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null || importer.spriteImportMode != SpriteImportMode.Multiple) continue;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePivot = new Vector2(0.5f, 0.5f);
            importer.SaveAndReimport();
        }
    }

    static void ClearOld()
    {
        foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
            if (RootNames.Contains(go.name)) Object.DestroyImmediate(go);
    }

    // ================= scene Home =================
    static void BuildHome()
    {
        BuildAudio();
        var canvas = BuildCanvas();
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        Background(canvas, "bg_home.png");
        Img("Crown", canvas, Spr("icon_crown.png"), SizeW("icon_crown.png", 110), new Vector2(-170, -380));
        var best = Txt("BestText", canvas, "0", 80, new Vector2(60, -380), new Vector2(400, 120), TextAnchor.MiddleLeft);
        var play = Btn("PlayButton", canvas, "btn_play.png", 520, new Vector2(0, -150));

        var homeGo = new GameObject("HomeManager");
        var home = homeGo.AddComponent<HomeScreen>();
        Set(home, "bestText", best);
        OnClick(play, home.Play);
    }

    // ================= scene Game =================
    static void BuildGame()
    {
        var audio = BuildAudio();
        var canvas = BuildCanvas();
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        Background(canvas, "bg_game.png");

        // --- Board ---
        var board = Img("Board", canvas, Spr("board.png"), new Vector2(960, 960), BoardPos);
        var boardMgr = board.gameObject.AddComponent<BoardManager>();
        var ghostLayer = Rt("Ghosts", board.rectTransform, Vector2.zero, Vector2.zero);
        var cellLayer = Rt("Cells", board.rectTransform, Vector2.zero, Vector2.zero);
        var ghosts = new List<Object>();
        for (int x = 0; x < Size; x++)
            for (int y = 0; y < Size; y++)
            {
                var g = Img($"Ghost_{x}_{y}", ghostLayer, Spr("block_1.png"), new Vector2(Cell, Cell),
                    new Vector2(-Size * Cell / 2f + Cell * (x + 0.5f), Size * Cell / 2f - Cell * (y + 0.5f)));
                g.color = new Color(1, 1, 1, 0.45f);
                g.gameObject.SetActive(false);
                ghosts.Add(g);
            }
        Set(boardMgr, "cellLayer", cellLayer);
        SetArray(boardMgr, "ghostImages", ghosts);
        SetArray(boardMgr, "blockSprites", Enumerable.Range(1, 8).Select(i => (Object)Spr($"block_{i}.png")).ToList());

        // --- Khay + lớp chứa khối ---
        var pieceLayer = Rt("PieceLayer", canvas, Vector2.zero, Vector2.zero);
        var slotsRoot = Rt("TraySlots", canvas, Vector2.zero, Vector2.zero);
        var slots = new List<Object>();
        for (int i = 0; i < 3; i++) slots.Add(Rt($"Slot{i + 1}", slotsRoot, Vector2.zero, SlotPos[i]));

        // --- Combo ---
        var comboRoot = Rt("Combo", canvas, Vector2.zero, BoardPos);
        var comboImg = Img("ComboImage", comboRoot, null, new Vector2(640, 170), Vector2.zero);
        comboImg.gameObject.SetActive(false);
        var comboFx = comboRoot.gameObject.AddComponent<ComboEffect>();
        Set(comboFx, "image", comboImg);
        SetArray(comboFx, "comboSprites", Enumerable.Range(1, 7).Select(i => (Object)Spr($"combo_{i}.png")).ToList());

        // --- HUD ---
        var hud = Rt("HUD", canvas, Vector2.zero, Vector2.zero);
        var scoreUI = hud.gameObject.AddComponent<ScoreUI>();
        Img("Crown", hud, Spr("icon_crown.png"), SizeW("icon_crown.png", 80), new Vector2(-390, 800));
        var hudBest = Txt("BestText", hud, "0", 56, new Vector2(-250, 800), new Vector2(300, 80), TextAnchor.MiddleLeft);
        var hudScore = Txt("ScoreText", hud, "0", 130, new Vector2(0, 680), new Vector2(800, 160), TextAnchor.MiddleCenter);
        Set(scoreUI, "scoreText", hudScore);
        Set(scoreUI, "bestText", hudBest);
        var settingsBtn = Btn("SettingsButton", hud, "icon_setting.png", 90, new Vector2(440, 800));

        // --- Panel Settings ---
        var settings = Stretch("SettingsPanel", canvas);
        var settingsPopup = settings.gameObject.AddComponent<SettingsPopup>();
        Dimmer(settings);
        var sp = Rt("Panel", settings, new Vector2(760, 1069), Vector2.zero);
        Img("Bg", sp, Spr("popup_setting.png"), new Vector2(760, 1069), Vector2.zero);
        Txt("Title", sp, "SETTINGS", 64, new Vector2(0, 470), new Vector2(600, 100), TextAnchor.MiddleCenter);
        Img("MusicIcon", sp, Spr("icon_music.png"), SizeW("icon_music.png", 60), new Vector2(-250, 230));
        Txt("MusicLabel", sp, "Music", 52, new Vector2(-40, 230), new Vector2(300, 80), TextAnchor.MiddleLeft);
        var musicBtn = Btn("MusicToggle", sp, "toggle_on.png", 170, new Vector2(230, 230));
        Img("SoundIcon", sp, Spr("icon_sound.png"), SizeW("icon_sound.png", 70), new Vector2(-250, 60));
        Txt("SoundLabel", sp, "Sound", 52, new Vector2(-40, 60), new Vector2(300, 80), TextAnchor.MiddleLeft);
        var soundBtn = Btn("SoundToggle", sp, "toggle_on.png", 170, new Vector2(230, 60));
        var continueBtn = Btn("ContinueButton", sp, "btn_continue.png", 440, new Vector2(0, -150));
        var settingsHome = Btn("HomeButton", sp, "btn_home.png", 440, new Vector2(0, -320));
        Set(settingsPopup, "musicToggle", musicBtn.image);
        Set(settingsPopup, "soundToggle", soundBtn.image);
        Set(settingsPopup, "toggleOn", Spr("toggle_on.png"));
        Set(settingsPopup, "toggleOff", Spr("toggle_off.png"));

        // --- Panel Result ---
        var result = Stretch("ResultPanel", canvas);
        var resultPopup = result.gameObject.AddComponent<ResultPopup>();
        Dimmer(result);
        var rp = Rt("Panel", result, new Vector2(800, 716), new Vector2(0, 120));
        Img("Bg", rp, Spr("popup_result.png"), new Vector2(800, 716), Vector2.zero);
        Img("Crown", rp, Spr("icon_crown.png"), SizeW("icon_crown.png", 150), new Vector2(0, 195));
        Txt("ScoreLabel", rp, "SCORE", 44, new Vector2(0, 95), new Vector2(500, 60), TextAnchor.MiddleCenter);
        var resScore = Txt("ScoreText", rp, "0", 110, new Vector2(0, 10), new Vector2(700, 140), TextAnchor.MiddleCenter);
        var resBest = Txt("BestText", rp, "BEST 0", 48, new Vector2(0, -90), new Vector2(600, 70), TextAnchor.MiddleCenter);
        var resNew = Txt("NewBestText", rp, "NEW BEST!", 50, new Vector2(0, -165), new Vector2(600, 70), TextAnchor.MiddleCenter);
        resNew.color = new Color(1f, 0.85f, 0.2f);
        var replayBtn = Btn("ReplayButton", rp, "btn_play.png", 380, new Vector2(-215, -470));
        var resultHome = Btn("HomeButton", rp, "btn_home.png", 380, new Vector2(215, -470));
        Set(resultPopup, "scoreText", resScore);
        Set(resultPopup, "bestText", resBest);
        Set(resultPopup, "newBestLabel", resNew.gameObject);

        // --- Spawner, GameManager, DragController ---
        var spawner = new GameObject("PieceSpawner").AddComponent<PieceSpawner>();
        Set(spawner, "board", boardMgr);
        Set(spawner, "pieceLayer", pieceLayer);
        SetArray(spawner, "slots", slots);

        var gm = new GameObject("GameManager").AddComponent<GameManager>();
        Set(gm, "board", boardMgr);
        Set(gm, "spawner", spawner);
        Set(gm, "scoreUI", scoreUI);
        Set(gm, "comboEffect", comboFx);
        Set(gm, "settingsPopup", settingsPopup);
        Set(gm, "resultPopup", resultPopup);

        var drag = new GameObject("DragController").AddComponent<DragController>();
        Set(drag, "game", gm);
        Set(drag, "board", boardMgr);
        Set(drag, "spawner", spawner);

        // --- Nút bấm ---
        OnClick(settingsBtn, settingsPopup.Open);
        OnClick(musicBtn, settingsPopup.ToggleMusic);
        OnClick(soundBtn, settingsPopup.ToggleSound);
        OnClick(continueBtn, settingsPopup.Close);
        OnClick(settingsHome, gm.GoHome);
        OnClick(replayBtn, gm.StartGame);
        OnClick(resultHome, gm.GoHome);

        // Trạng thái ban đầu: 2 panel ẩn.
        settings.gameObject.SetActive(false);
        result.gameObject.SetActive(false);
    }

    // ================= dùng chung =================
    static AudioManager BuildAudio()
    {
        var audioGo = new GameObject("AudioManager");
        var audio = audioGo.AddComponent<AudioManager>();
        var musicSrc = Child(audioGo, "Music").AddComponent<AudioSource>();
        musicSrc.playOnAwake = false;
        var effectSrc = Child(audioGo, "Effect").AddComponent<AudioSource>();
        effectSrc.playOnAwake = false;
        Set(audio, "musicSource", musicSrc);
        Set(audio, "effectSource", effectSrc);
        Set(audio, "music", Clip("bgm.ogg"));
        SetArray(audio, "sfxClips", new[] { "click", "place", "clear", "combo", "gameover", "newbest" }
            .Select(n => (Object)Clip(n + ".mp3")).ToList());
        return audio;
    }

    static RectTransform BuildCanvas()
    {
        var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        return (RectTransform)canvasGo.transform;
    }

    static void OnClick(Button b, UnityAction action)
    {
        UnityEventTools.AddPersistentListener(b.onClick, action);
    }

    static void Set(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        var p = so.FindProperty(field);
        if (p == null) { Debug.LogError($"Missing field {field} on {target.GetType().Name}"); return; }
        p.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetArray(Object target, string field, IList<Object> values)
    {
        var so = new SerializedObject(target);
        var p = so.FindProperty(field);
        if (p == null) { Debug.LogError($"Missing field {field} on {target.GetType().Name}"); return; }
        p.arraySize = values.Count;
        for (int i = 0; i < values.Count; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static Sprite Spr(string file)
    {
        var s = AssetDatabase.LoadAllAssetsAtPath(SpriteDir + file).OfType<Sprite>().FirstOrDefault();
        if (s == null) Debug.LogError("Missing sprite " + file);
        return s;
    }

    static AudioClip Clip(string file) => AssetDatabase.LoadAssetAtPath<AudioClip>(AudioDir + file);

    static Vector2 SizeW(string file, float w)
    {
        var s = Spr(file);
        return new Vector2(w, w * s.rect.height / s.rect.width);
    }

    static GameObject Child(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    static RectTransform Rt(string name, Transform parent, Vector2 size, Vector2 pos)
    {
        var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        r.SetParent(parent, false);
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f);
        r.sizeDelta = size;
        r.anchoredPosition = pos;
        return r;
    }

    static RectTransform Stretch(string name, Transform parent)
    {
        var r = Rt(name, parent, Vector2.zero, Vector2.zero);
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
        return r;
    }

    static Image Img(string name, Transform parent, Sprite s, Vector2 size, Vector2 pos)
    {
        var i = Rt(name, parent, size, pos).gameObject.AddComponent<Image>();
        i.sprite = s;
        i.raycastTarget = false;
        return i;
    }

    static void Background(RectTransform parent, string file)
    {
        var s = Spr(file);
        var img = Img("Background", parent, s, Vector2.zero, Vector2.zero);
        var fit = img.gameObject.AddComponent<AspectRatioFitter>();
        fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fit.aspectRatio = s.rect.width / s.rect.height;
    }

    static void Dimmer(RectTransform parent)
    {
        var d = Stretch("Dimmer", parent);
        var img = d.gameObject.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.65f); // raycastTarget bật để chặn bấm xuyên popup
    }

    static Text Txt(string name, Transform parent, string text, int size, Vector2 pos, Vector2 box, TextAnchor anchor)
    {
        var t = Rt(name, parent, box, pos).gameObject.AddComponent<Text>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.fontStyle = FontStyle.Bold;
        t.alignment = anchor;
        t.color = Color.white;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        var o = t.gameObject.AddComponent<Outline>();
        o.effectColor = new Color(0.05f, 0.1f, 0.4f, 1f);
        o.effectDistance = new Vector2(3, -3);
        return t;
    }

    static Button Btn(string name, Transform parent, string sprite, float width, Vector2 pos)
    {
        var i = Img(name, parent, Spr(sprite), SizeW(sprite, width), pos);
        i.raycastTarget = true;
        var b = i.gameObject.AddComponent<Button>();
        b.targetGraphic = i;
        b.colors = new ColorBlock
        {
            normalColor = Color.white, highlightedColor = Color.white,
            pressedColor = new Color(0.75f, 0.75f, 0.75f), selectedColor = Color.white,
            disabledColor = Color.gray, colorMultiplier = 1, fadeDuration = 0.05f
        };
        i.gameObject.AddComponent<ButtonSound>();
        return b;
    }
}
