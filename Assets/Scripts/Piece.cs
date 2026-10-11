using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Một khối nằm ở khay (3 khối mỗi lượt). Tự dựng các ô con từ chuỗi shape.
public class Piece : MonoBehaviour
{
    public bool[,] Shape { get; private set; } // [x, y]
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Color { get; private set; }     // 1..8
    public int Slot { get; private set; }

    public void Init(string shape, int color, int slot, Sprite sprite, float cellSize)
    {
        var rows = shape.Split('/');
        Height = rows.Length;
        Width = rows[0].Length;
        Color = color;
        Slot = slot;
        Shape = new bool[Width, Height];

        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                if (rows[y][x] != 'X') continue;
                Shape[x, y] = true;
                var go = new GameObject("Cell", typeof(RectTransform), typeof(Image));
                var rt = (RectTransform)go.transform;
                rt.SetParent(transform, false);
                rt.sizeDelta = new Vector2(cellSize, cellSize);
                rt.anchoredPosition = new Vector2((x - (Width - 1) / 2f) * cellSize, -(y - (Height - 1) / 2f) * cellSize);
                var img = go.GetComponent<Image>();
                img.sprite = sprite;
                img.raycastTarget = false;
            }
    }

    public void PopIn(float targetScale)
    {
        StartCoroutine(Scale(0f, targetScale, 0.2f));
    }

    public void ReturnTo(Vector3 worldPos, float targetScale)
    {
        StartCoroutine(Return(worldPos, targetScale));
    }

    IEnumerator Scale(float from, float to, float time)
    {
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            transform.localScale = Vector3.one * Mathf.Lerp(from, to, t / time);
            yield return null;
        }
        transform.localScale = Vector3.one * to;
    }

    IEnumerator Return(Vector3 worldPos, float targetScale)
    {
        Vector3 from = transform.position;
        float fromScale = transform.localScale.x;
        const float time = 0.15f;
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            float k = t / time;
            transform.position = Vector3.Lerp(from, worldPos, k);
            transform.localScale = Vector3.one * Mathf.Lerp(fromScale, targetScale, k);
            yield return null;
        }
        transform.position = worldPos;
        transform.localScale = Vector3.one * targetScale;
    }
}
