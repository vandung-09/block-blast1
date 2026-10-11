using UnityEngine;
using UnityEngine.InputSystem;

// Xử lý kéo thả khối từ khay lên bàn (chuột + cảm ứng qua Input System).
public class DragController : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] BoardManager board;
    [SerializeField] PieceSpawner spawner;
    [SerializeField] float pickRadius = 170f; // bán kính bấm trúng slot
    [SerializeField] float dragLift = 170f;   // khối nổi lên trên ngón tay để không bị che

    Piece drag;
    Vector2Int cell;
    bool valid;

    void Update()
    {
        if (!game.InputEnabled)
        {
            if (drag != null) CancelDrag();
            return;
        }

        var ptr = Pointer.current;
        if (ptr == null) return;
        Vector3 world = PointerWorld(ptr.position.ReadValue());

        if (drag == null)
        {
            if (ptr.press.wasPressedThisFrame) TryPick(world);
        }
        else if (ptr.press.isPressed) MoveDrag(world);
        else Drop();
    }

    Vector3 PointerWorld(Vector2 screen)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(spawner.Layer, screen, null, out var w);
        return w;
    }

    void TryPick(Vector3 world)
    {
        Vector2 p = spawner.Layer.InverseTransformPoint(world);
        for (int i = 0; i < spawner.SlotCount; i++)
        {
            var piece = spawner.GetPiece(i);
            if (piece == null) continue;
            Vector2 s = spawner.Layer.InverseTransformPoint(spawner.SlotWorldPos(i));
            if (Vector2.Distance(p, s) > pickRadius) continue;

            drag = piece;
            drag.transform.localScale = Vector3.one;
            drag.transform.SetAsLastSibling();
            AudioManager.Instance.PlayClick();
            MoveDrag(world);
            return;
        }
    }

    void MoveDrag(Vector3 world)
    {
        Vector3 center = world + spawner.Layer.TransformVector(new Vector3(0, dragLift, 0));
        drag.transform.position = center;
        cell = board.SnapCell(drag, center);
        valid = board.CanPlace(drag, cell.x, cell.y);
        if (valid) board.ShowGhost(drag, cell.x, cell.y);
        else board.HideGhost();
    }

    void Drop()
    {
        board.HideGhost();
        var p = drag;
        drag = null;
        if (valid) game.PlacePiece(p, cell.x, cell.y);
        else p.ReturnTo(spawner.SlotWorldPos(p.Slot), spawner.TrayScale);
    }

    void CancelDrag()
    {
        board.HideGhost();
        drag.ReturnTo(spawner.SlotWorldPos(drag.Slot), spawner.TrayScale);
        drag = null;
    }
}
