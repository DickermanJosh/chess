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
        HighlightColor = Color.Lerp(DefaultColor, new Color(0.95f, 0.78f, 0.25f), 0.65f);
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = defaultSquareSprite;
        if (spriteRenderer != null) spriteRenderer.color = DefaultColor;

        gameObject.name = $"Square_{squareData.Coord.ToString()}-({squareData.Coord.ToVector2().x},{squareData.Coord.ToVector2().y})-{squareData.Index}";

        // Add a square click handler upon creation to make the square selectable by the player
        gameObject.AddComponent<SquareClickHandler>();

        // Position this square in the world
        float xPos = squareData.Coord.file * squareSize;
        float yPos = squareData.Coord.rank * squareSize;

        transform.position = new Vector3(xPos - 3.5f, yPos - 3.5f, 0);

        spriteRenderer.sortingOrder = 2;

        BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
        collider.size = spriteRenderer.bounds.size;

    }

    public void AddHighlight()
    {
        if (spriteRenderer != null) spriteRenderer.color = HighlightColor;
    }

    public void RemoveHighlight()
    {
        if (spriteRenderer != null) spriteRenderer.color = DefaultColor;
    }
}
