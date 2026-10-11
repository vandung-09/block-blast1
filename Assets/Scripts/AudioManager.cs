using UnityEngine;

public enum Sfx { Click, Place, Clear, Combo, GameOver, NewBest }

// Nhạc nền + hiệu ứng âm thanh, có bật/tắt và lưu vào PlayerPrefs.
// Có mặt ở cả 2 scene nhưng chỉ giữ 1 bản (singleton), nhạc không bị ngắt khi đổi scene.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource effectSource;
    [SerializeField] AudioClip music;
    [SerializeField] AudioClip[] sfxClips; // thứ tự theo enum Sfx

    public bool MusicOn { get; private set; }
    public bool SoundOn { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        MusicOn = PlayerPrefs.GetInt("music", 1) == 1;
        SoundOn = PlayerPrefs.GetInt("sound", 1) == 1;
        musicSource.clip = music;
        musicSource.loop = true;
        musicSource.mute = !MusicOn;
    }

    void Start()
    {
        if (Instance == this && music != null && !musicSource.isPlaying) musicSource.Play();
    }

    public void Play(Sfx sfx)
    {
        int i = (int)sfx;
        if (SoundOn && i < sfxClips.Length && sfxClips[i] != null) effectSource.PlayOneShot(sfxClips[i]);
    }

    public void PlayClick() => Play(Sfx.Click);

    public void SetMusic(bool on)
    {
        MusicOn = on;
        musicSource.mute = !on;
        PlayerPrefs.SetInt("music", on ? 1 : 0);
    }

    public void SetSound(bool on)
    {
        SoundOn = on;
        PlayerPrefs.SetInt("sound", on ? 1 : 0);
    }
}
