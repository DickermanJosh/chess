using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SquareRenderer))]
public class SquareClickHandler : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private bool dragged;
    private SquareRenderer Square => GetComponent<SquareRenderer>();
    private static Core.Square Target(PointerEventData data)
    {
        // The EventSystem's topmost hit respects menus and promotion overlays.
        // Do not fall through UI by doing a separate physics raycast.
        var hit = data.pointerCurrentRaycast.gameObject;
        return hit == null ? null : hit.GetComponent<SquareRenderer>()?.squareData;
    }
    public void OnPointerClick(PointerEventData data)
    {
        if (data.button != PointerEventData.InputButton.Left || dragged) { dragged = false; return; }
        BoardInputManager.Instance?.OnSquareClicked(Square.squareData);
    }
    public void OnBeginDrag(PointerEventData data)
    {
        dragged = data.button == PointerEventData.InputButton.Left;
        if (dragged && BoardInputManager.Instance?.BeginDrag(Square.squareData, data.pointerId) == true)
            OnDrag(data);
    }
    public void OnDrag(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Left)
            BoardInputManager.Instance?.DragTo(data.position, Target(data), data.pointerId);
    }
    public void OnEndDrag(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Left)
            BoardInputManager.Instance?.EndDrag(Target(data), data.pointerId);
        // Unity suppresses click when a drag ends. Reset here for the next click.
        dragged = false;
    }
}
