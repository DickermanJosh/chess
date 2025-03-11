using Core;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SquareRenderer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color DefaultColor;
    private Color HighlightColor;
    public Square squareData { get; private set; }

    /// <summary>
    /// Initialize this square renderer with the underlying Square data,
    /// a single default sprite, and a color used to tint that sprite.
    /// </summary>
    public void Init(Square squareData, Sprite defaultSquareSprite, Color squareColor, float squareSize)
    {
        this.squareData = squareData;
        this.DefaultColor = squareColor;
        if (squareData.IsWhite) 
        {
            HighlightColor = new Color(DefaultColor.r, DefaultColor.g, DefaultColor.b - 50);
        }
        else
        {
            HighlightColor = new Color(DefaultColor.r + 12, DefaultColor.g + 12, DefaultColor.b);
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = defaultSquareSprite;
        spriteRenderer.color = DefaultColor;

        gameObject.name = $"Square_{squareData.Coord.ToString()}-{squareData.Coord.ToVector2().x},{squareData.Coord.ToVector2().y}";

        // Add a square click handler upon creation to make the square selectable by the player
        gameObject.AddComponent<SquareClickHandler>();

        // Position this square in the world
        float xPos = squareData.Coord.file * squareSize;
        float yPos = squareData.Coord.rank * squareSize;

        transform.position = new Vector3(xPos - 3.5f, yPos - 3.5f, 0);

        spriteRenderer.sortingOrder = 0;

        BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
        collider.size = spriteRenderer.bounds.size;

    }

    public void AddHighlight()
    {
        spriteRenderer.color = HighlightColor;
    }

    public void RemoveHighlight()
    {
        spriteRenderer.color = DefaultColor;
    }
}
