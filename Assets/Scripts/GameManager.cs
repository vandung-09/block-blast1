using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Điều phối luồng game trong scene Game: bắt đầu, đặt khối, tính điểm, hết nước đi.
public class GameManager : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] BoardManager board;
    [SerializeField] PieceSpawner spawner;

    [Header("UI")]
    [SerializeField] ScoreUI scoreUI;
    [SerializeField] ComboEffect comboEffect;
    [SerializeField] SettingsPopup settingsPopup;
    [SerializeField] ResultPopup resultPopup;

    [SerializeField] string homeSceneName = "Home";

    int score, best, bestAtStart, streak;
    bool playing;

    AudioManager Audio => AudioManager.Instance;
    public bool InputEnabled => playing && !settingsPopup.IsOpen && !resultPopup.IsOpen;

    void Start()
    {
        best = PlayerPrefs.GetInt("best", 0);
        StartGame();
    }

    // ----- gọi từ các nút trong Inspector -----
    public void GoHome() => SceneManager.LoadScene(homeSceneName);

    public void StartGame()
    {
        StopAllCoroutines();
        settingsPopup.Close();
        resultPopup.Hide();

        board.ResetBoard();
        spawner.Clear();
        score = streak = 0;
        bestAtStart = best;
        scoreUI.ResetScore(best);
        playing = true;
        spawner.SpawnSet();
    }

    // ----- gọi từ DragController -----
    public void PlacePiece(Piece piece, int col, int row)
    {
        int placed = board.Place(piece, col, row);
        spawner.Release(piece);
        Destroy(piece.gameObject);
        Audio.Play(Sfx.Place);
        score += placed;

        int lines = board.ClearFullLines();
        if (lines > 0)
        {
            streak++;
            score += lines * lines * 10 + (streak - 1) * 10;
            Audio.Play(lines > 1 || streak > 1 ? Sfx.Combo : Sfx.Clear);
            comboEffect.Show(lines + streak - 1);
        }
        else streak = 0;

        scoreUI.SetScore(score);
        if (score > best)
        {
            best = score;
            scoreUI.SetBest(best);
            PlayerPrefs.SetInt("best", best);
        }

        if (spawner.AllUsed()) spawner.SpawnSet();
        if (!spawner.AnyFits()) StartCoroutine(GameOver());
    }

    IEnumerator GameOver()
    {
        playing = false;
        bool isNewBest = score > bestAtStart;
        yield return new WaitForSeconds(0.8f);
        Audio.Play(isNewBest ? Sfx.NewBest : Sfx.GameOver);
        scoreUI.ShowFinalScore();
        resultPopup.Show(score, best, isNewBest);
    }
}
