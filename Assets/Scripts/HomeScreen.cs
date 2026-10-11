using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Scene Home: hiển thị điểm cao nhất, nút Play chuyển sang scene Game.
public class HomeScreen : MonoBehaviour
{
    [SerializeField] Text bestText;
    [SerializeField] string gameSceneName = "Game";

    void Start() => bestText.text = PlayerPrefs.GetInt("best", 0).ToString();

    public void Play() => SceneManager.LoadScene(gameSceneName);
}
