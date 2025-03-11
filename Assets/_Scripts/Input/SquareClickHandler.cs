using UnityEngine;
using Core;

[RequireComponent(typeof(SquareRenderer))]
public class SquareClickHandler : MonoBehaviour
{
    private Square squareData;

    private void Start()
    {
        // We can fetch the SquareRenderer and see which Square it references
        var renderer = GetComponent<SquareRenderer>();
        squareData = renderer.squareData;
    }

    private void OnMouseDown()
    {
        // Unity calls this if the user clicks on this object’s collider
        // Pass the event to BoardInputManager
        BoardInputManager.Instance.OnSquareClicked(squareData);
        Debug.Log("Square clicked.");
    }
}
