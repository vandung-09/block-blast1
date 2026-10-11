using UnityEngine;
using UnityEngine.UI;

// Popup kết quả khi hết nước đi.
public class ResultPopup : MonoBehaviour
{
    [SerializeField] Text scoreText;
    [SerializeField] Text bestText;
    [SerializeField] GameObject newBestLabel;

    public bool IsOpen => gameObject.activeSelf;

    public void Show(int score, int best, bool isNewBest)
    {
        scoreText.text = score.ToString();
        bestText.text = "BEST " + best;
        newBestLabel.SetActive(isNewBest);
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void Hide() => gameObject.SetActive(false);
}
