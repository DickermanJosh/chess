using Core;
using Opera;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SquareRenderer : MonoBehaviour
{
    private SpriteRenderer face, marker, selection, hover;
    private Color defaultColor, markerColor;
    private bool lastMove;
    public Square squareData { get; private set; }
    public bool IsLastMove => lastMove;
    public bool IsHighlighted => marker != null && marker.enabled;

    public void Init(Square square, Sprite defaultSquareSprite, Color color, float squareSize)
    {
        squareData = square; defaultColor = color;
        markerColor = square.IsWhite ? OperaTheme.Hex("456851") : OperaTheme.Sage;
        face = GetComponent<SpriteRenderer>(); face.sprite = OperaTheme.Wood; face.color = color; face.sortingOrder = 2;
        transform.localPosition = new Vector3((square.Coord.file - 3.5f) * squareSize, (square.Coord.rank - 3.5f) * squareSize, 0);
        transform.localScale = Vector3.one * squareSize;
        gameObject.name = "Square " + square.Coord;
        gameObject.AddComponent<SquareClickHandler>();
        gameObject.AddComponent<BoxCollider2D>().size = Vector2.one;
        marker = OperaTheme.World(transform, "Legal destination", OperaTheme.Disc, Vector2.zero, Vector2.one * .23f, markerColor, 5);
        selection = OperaTheme.World(transform, "Selected piece", OperaTheme.Outline, Vector2.zero, Vector2.one * .98f, markerColor, 4);
        hover = OperaTheme.World(transform, "Drop preview", OperaTheme.Outline, Vector2.zero, Vector2.one * .90f, OperaTheme.Ink, 6);
        marker.enabled = selection.enabled = hover.enabled = false;
    }
    public void AddHighlight(bool capture = false, bool castle = false)
    {
        marker.sprite = castle ? OperaTheme.Outline : capture ? OperaTheme.Ring : OperaTheme.Disc;
        marker.transform.localScale = Vector3.one * (castle ? .92f : capture ? .91f : .23f);
        marker.color = new Color(markerColor.r, markerColor.g, markerColor.b, capture || castle ? .94f : .82f);
        marker.enabled = true;
    }
    public void SetSelected(bool selected) => selection.enabled = selected;
    public void SetDropTarget(bool target) => hover.enabled = target;
    public void SetLastMove(bool value)
    {
        lastMove = value; face.color = value ? Color.Lerp(defaultColor, OperaTheme.LastMove, .56f) : defaultColor;
    }
    public void RemoveHighlight()
    {
        marker.enabled = selection.enabled = hover.enabled = false;
        // Last move belongs to the position, not the current selection.
    }
}
