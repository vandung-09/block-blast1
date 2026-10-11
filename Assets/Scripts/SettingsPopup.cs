using UnityEngine;
using UnityEngine.UI;

// Panel cài đặt trong scene Game: bật/tắt nhạc và âm thanh.
public class SettingsPopup : MonoBehaviour
{
    [SerializeField] Image musicToggle;
    [SerializeField] Image soundToggle;
    [SerializeField] Sprite toggleOn;
    [SerializeField] Sprite toggleOff;

    public bool IsOpen => gameObject.activeSelf;

    public void Open()
    {
        Refresh();
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void Close() => gameObject.SetActive(false);

    public void ToggleMusic()
    {
        AudioManager.Instance.SetMusic(!AudioManager.Instance.MusicOn);
        Refresh();
    }

    public void ToggleSound()
    {
        AudioManager.Instance.SetSound(!AudioManager.Instance.SoundOn);
        Refresh();
    }

    void Refresh()
    {
        musicToggle.sprite = AudioManager.Instance.MusicOn ? toggleOn : toggleOff;
        soundToggle.sprite = AudioManager.Instance.SoundOn ? toggleOn : toggleOff;
    }
}
