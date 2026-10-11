using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Bàn 8x8: lưu dữ liệu ô, đặt khối, xóa hàng/cột, hiện bóng gợi ý.
// Script này gắn trên object Board (pivot ở giữa), mọi vị trí tính theo toạ độ local của Board.
public class BoardManager : MonoBehaviour
{
    [SerializeField] RectTransform cellLayer;
    [SerializeField] Image[] ghostImages;   // size*size ảnh, index = x * size + y
    [SerializeField] Sprite[] blockSprites; // block_1..block_8
    [SerializeField] int size = 8;
    [SerializeField] float cellSize = 118.5f;

    int[,] grid;
    Image[,] cells;

    public float CellSize => cellSize;
    public Sprite BlockSprite(int color) => blockSprites[color - 1];
    public int ColorCount => blockSprites.Length;

    void Awake() => Init();

    void Init()
    {
        if (grid != null) return;
        grid = new int[size, size];
        cells = new Image[size, size];
        HideGhost();
    }

    public void ResetBoard()
    {
        Init();
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                grid[x, y] = 0;
                if (cells[x, y] != null) Destroy(cells[x, y].gameObject);
                cells[x, y] = null;
            }
        HideGhost();
    }

    Vector2 CellPos(int c, int r) =>
        new Vector2(-size * cellSize / 2f + cellSize * (c + 0.5f), size * cellSize / 2f - cellSize * (r + 0.5f));

    // Ô (cột, hàng) mà khối p đang nằm gần nhất khi tâm khối ở vị trí world.
    public Vector2Int SnapCell(Piece p, Vector3 pieceCenterWorld)
    {
        Vector3 l = transform.InverseTransformPoint(pieceCenterWorld);
        float left = l.x - p.Width * cellSize / 2f;
        float top = l.y + p.Height * cellSize / 2f;
        return new Vector2Int(
            Mathf.RoundToInt((left + size * cellSize / 2f) / cellSize),
            Mathf.RoundToInt((size * cellSize / 2f - top) / cellSize));
    }

    public bool CanPlace(Piece p, int col, int row)
    {
        Init();
        if (col < 0 || row < 0 || col + p.Width > size || row + p.Height > size) return false;
        for (int x = 0; x < p.Width; x++)
            for (int y = 0; y < p.Height; y++)
                if (p.Shape[x, y] && grid[col + x, row + y] != 0) return false;
        return true;
    }

    public bool FitsAnywhere(Piece p)
    {
        for (int c = 0; c <= size - p.Width; c++)
            for (int r = 0; r <= size - p.Height; r++)
                if (CanPlace(p, c, r)) return true;
        return false;
    }

    // Đặt khối, trả về số ô đã đặt.
    public int Place(Piece p, int col, int row)
    {
        int placed = 0;
        var sprite = BlockSprite(p.Color);
        for (int x = 0; x < p.Width; x++)
            for (int y = 0; y < p.Height; y++)
            {
                if (!p.Shape[x, y]) continue;
                grid[col + x, row + y] = p.Color;
                cells[col + x, row + y] = NewCell(sprite, CellPos(col + x, row + y));
                placed++;
            }
        return placed;
    }

    Image NewCell(Sprite sprite, Vector2 pos)
    {
        var go = new GameObject("Cell", typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(cellLayer, false);
        rt.sizeDelta = new Vector2(cellSize, cellSize);
        rt.anchoredPosition = pos;
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;
        return img;
    }

    // Xóa mọi hàng/cột đã đầy, trả về số hàng + cột bị xóa.
    public int ClearFullLines()
    {
        var rows = new List<int>();
        var cols = new List<int>();
        for (int i = 0; i < size; i++)
        {
            bool rowFull = true, colFull = true;
            for (int j = 0; j < size; j++)
            {
                if (grid[j, i] == 0) rowFull = false;
                if (grid[i, j] == 0) colFull = false;
            }
            if (rowFull) rows.Add(i);
            if (colFull) cols.Add(i);
        }

        var toClear = new HashSet<Vector2Int>();
        foreach (int r in rows) for (int c = 0; c < size; c++) toClear.Add(new Vector2Int(c, r));
        foreach (int c in cols) for (int r = 0; r < size; r++) toClear.Add(new Vector2Int(c, r));
        foreach (var v in toClear)
        {
            if (cells[v.x, v.y] != null) StartCoroutine(Vanish(cells[v.x, v.y]));
            cells[v.x, v.y] = null;
            grid[v.x, v.y] = 0;
        }
        return rows.Count + cols.Count;
    }

    IEnumerator Vanish(Image img)
    {
        const float time = 0.25f;
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            if (img == null) yield break;
            float k = t / time;
            img.transform.localScale = Vector3.one * Mathf.Lerp(1.1f, 0f, k);
            img.color = new Color(1, 1, 1, 1 - k);
            yield return null;
        }
        if (img != null) Destroy(img.gameObject);
    }

    public void ShowGhost(Piece p, int col, int row)
    {
        HideGhost();
        var sprite = BlockSprite(p.Color);
        for (int x = 0; x < p.Width; x++)
            for (int y = 0; y < p.Height; y++)
                if (p.Shape[x, y])
                {
                    var g = ghostImages[(col + x) * size + (row + y)];
                    g.sprite = sprite;
                    g.gameObject.SetActive(true);
                }
    }

    public void HideGhost()
    {
        foreach (var g in ghostImages) g.gameObject.SetActive(false);
    }
}
