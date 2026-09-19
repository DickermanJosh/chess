using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SquareRenderer))]
public class SquareClickHandler : MonoBehaviour, IPointerClickHandler
{
    // Use the same Input System / EventSystem path as the menus. Graphic UI
    // raycasts take priority, so a promotion or menu click cannot move a piece.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        BoardInputManager.Instance?.OnSquareClicked(GetComponent<SquareRenderer>().squareData);
    }
}
