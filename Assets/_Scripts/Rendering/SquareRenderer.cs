using Core;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SquareRenderer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Square squareData;

    /// <summary>
    /// Initialize this square renderer with the underlying Square data,
    /// a single default sprite, and a color used to tint that sprite.
    /// </summary>
    public void Init(Square squareData, Sprite defaultSquareSprite, Color squareColor, float squareSize)
    {
        this.squareData = squareData;

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = defaultSquareSprite;
        spriteRenderer.color = squareColor;

        // Square Object naming for clarity in the hierarchy
        // gameObject.name = $"Square_{squareData.Coord.file}_{squareData.Coord.rank}";
        gameObject.name = $"Square_{squareData.Coord.ToString()}-{squareData.Coord.ToVector2().x},{squareData.Coord.ToVector2().y}";

        // Position this square in the world
        float xPos = squareData.Coord.file * squareSize;
        float yPos = squareData.Coord.rank * squareSize;

        transform.position = new Vector3(xPos - 3.5f, yPos - 3.5f, 0);

        spriteRenderer.sortingOrder = 0;
    }
}
