using UnityEngine;

// Sinh 3 khối mỗi lượt vào 3 slot ở khay.
public class PieceSpawner : MonoBehaviour
{
    [SerializeField] BoardManager board;
    [SerializeField] RectTransform pieceLayer; // nơi chứa các khối (vẽ phía trên bàn)
    [SerializeField] RectTransform[] slots;    // 3 vị trí khay
    [SerializeField] float trayScale = 0.5f;

    readonly Piece[] pieces = new Piece[3];

    public RectTransform Layer => pieceLayer;
    public float TrayScale => trayScale;
    public Piece GetPiece(int i) => pieces[i];
    public int SlotCount => slots.Length;
    public Vector3 SlotWorldPos(int i) => slots[i].position;

    public void Clear()
    {
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] != null) Destroy(pieces[i].gameObject);
            pieces[i] = null;
        }
    }

    public void Release(Piece p) => pieces[p.Slot] = null;

    public bool AllUsed()
    {
        foreach (var p in pieces) if (p != null) return false;
        return true;
    }

    // Còn ít nhất 1 khối đặt được trên bàn không?
    public bool AnyFits()
    {
        foreach (var p in pieces) if (p != null && board.FitsAnywhere(p)) return true;
        return false;
    }

    public void SpawnSet()
    {
        // Thử nhiều lần để tránh bộ khối không đặt được ngay từ đầu.
        for (int attempt = 0; attempt < 30; attempt++)
        {
            Clear();
            bool anyFits = false;
            for (int i = 0; i < pieces.Length; i++)
            {
                pieces[i] = CreatePiece(i);
                if (board.FitsAnywhere(pieces[i])) anyFits = true;
            }
            if (anyFits) break;
        }
        foreach (var p in pieces) p.PopIn(trayScale);
    }

    Piece CreatePiece(int slot)
    {
        var go = new GameObject("Piece", typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(pieceLayer, false);
        rt.position = slots[slot].position;
        rt.localScale = Vector3.one * trayScale;

        int color = Random.Range(1, board.ColorCount + 1);
        var piece = go.AddComponent<Piece>();
        piece.Init(ShapeLibrary.Random(), color, slot, board.BlockSprite(color), board.CellSize);
        return piece;
    }
}
