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
        spriteRenderer.sprite = pieceSprite;

        ChangeSpriteSize(0.75f);

        gameObject.name = $"Piece_{pieceData.ToString()}";
        transform.position = new Vector3(position.x, position.y, 0);
        spriteRenderer.sortingOrder = 1;
    }
    
    public void ChangePiece(Piece piece, Sprite pieceSprite)
    {
        this.pieceData = piece;
        spriteRenderer.sprite = pieceSprite;

        gameObject.name = $"Piece_{pieceData.ToString()}";
    }

    public void ChangeSpriteSize(float percent)
    {
        Vector2 newScale = new Vector2(percent, percent);
        spriteRenderer.transform.localScale = newScale;
    }

    public void Flip180()
    {
        spriteRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
    }
}
