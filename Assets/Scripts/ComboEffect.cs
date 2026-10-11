using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Hiện ảnh combo_1..combo_7 giữa bàn khi xóa hàng.
public class ComboEffect : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Sprite[] comboSprites; // combo_1..combo_7
    [SerializeField] float width = 640f;

    Coroutine running;

    void Awake() => image.gameObject.SetActive(false);

    public void Show(int level)
    {
        level = Mathf.Clamp(level, 1, comboSprites.Length);
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Play(comboSprites[level - 1]));
    }

    IEnumerator Play(Sprite sprite)
    {
        image.sprite = sprite;
        image.rectTransform.sizeDelta = new Vector2(width, width * sprite.rect.height / sprite.rect.width);
        image.gameObject.SetActive(true);
        const float total = 1.1f;
        for (float t = 0; t < total; t += Time.deltaTime)
        {
            float scale = t < 0.2f
                ? Mathf.Lerp(0.3f, 1.15f, t / 0.2f)
                : Mathf.Lerp(1.15f, 1f, Mathf.Min(1f, (t - 0.2f) / 0.2f));
            image.transform.localScale = Vector3.one * scale;
            image.rectTransform.anchoredPosition = new Vector2(0, t * 40f);
            image.color = new Color(1, 1, 1, t > 0.8f ? 1f - (t - 0.8f) / 0.3f : 1f);
            yield return null;
        }
        image.gameObject.SetActive(false);
        running = null;
    }
}
