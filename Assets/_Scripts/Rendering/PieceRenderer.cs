using Core;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PieceRenderer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Piece pieceData;

    /// <summary>
    /// Call this to set up the piece visuals (sprite, position, etc.)
    /// </summary>
    public void Init(Piece pieceData, Sprite pieceSprite, Vector2 position)
    {
        this.pieceData = pieceData;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Assign the chosen sprite
        spriteRenderer.sprite = pieceSprite;

        ChangeSpriteSize(0.1f);

        // Name for clarity in the hierarchy
        gameObject.name = $"Piece_{pieceData.ToString()}";

        transform.position = new Vector3(position.x, position.y, 0);

        // Ensure the piece is drawn above the squares
        spriteRenderer.sortingOrder = 1;
    }
    
    public void ChangePiece(Piece piece, Sprite pieceSprite)
    {
        this.pieceData = piece;

        // Assign the chosen sprite
        spriteRenderer.sprite = pieceSprite;

        // Name for clarity in the hierarchy
        gameObject.name = $"Piece_{pieceData.ToString()}";
    }

    public void ChangeSpriteSize(float percent)
    {
        spriteRenderer.size = new Vector2(percent, percent);
    }
}
