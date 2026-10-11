using UnityEngine;
using UnityEngine.UI;

// Hiển thị điểm hiện tại (chạy số mượt) và điểm cao nhất trong màn chơi.
public class ScoreUI : MonoBehaviour
{
    [SerializeField] Text scoreText;
    [SerializeField] Text bestText;

    int target, shown;

    public void ResetScore(int best)
    {
        target = shown = 0;
        scoreText.text = "0";
        SetBest(best);
    }

    public void SetScore(int score) => target = score;

    public void ShowFinalScore()
    {
        shown = target;
        scoreText.text = target.ToString();
    }

    public void SetBest(int best) => bestText.text = best.ToString();

    void Update()
    {
        if (shown == target) return;
        shown = Mathf.Min(target, shown + Mathf.Max(1, (target - shown) / 8));
        scoreText.text = shown.ToString();
    }
}
