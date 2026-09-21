using Core;
using Opera;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PieceRenderer : MonoBehaviour
{
    private SpriteRenderer face, shadow;
    private const float RestScale = .78f;
    public void Init(Piece piece, Sprite sprite, Vector2 position)
    {
        face = GetComponent<SpriteRenderer>(); face.sortingOrder = 10;
        transform.localPosition = Vector3.zero; transform.localScale = Vector3.one * RestScale;
        shadow = OperaTheme.World(transform, "Piece shadow", sprite, new Vector2(.025f, -.04f), Vector2.one, new Color(.08f, .055f, .025f, .25f), 9);
        ChangePiece(piece, sprite);
    }
    public void ChangePiece(Piece piece, Sprite sprite)
    {
        face.sprite = sprite; shadow.sprite = sprite;
        face.color = piece.GetColor() == PieceColor.White ? OperaTheme.Hex("F2E7D5") : OperaTheme.Hex("E5DAC6");
        gameObject.name = "Piece " + piece;
    }
    public void SetLifted(bool lifted)
    {
        if (!lifted) transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one * (lifted ? RestScale * 1.12f : RestScale);
        face.sortingOrder = lifted ? 40 : 10; shadow.sortingOrder = lifted ? 39 : 9;
        shadow.transform.localPosition = lifted ? new Vector3(.08f, -.11f, 0) : new Vector3(.025f, -.04f, 0);
        shadow.color = new Color(.08f, .055f, .025f, lifted ? .34f : .25f);
    }
    public void ChangeSpriteSize(float percent) => transform.localScale = Vector3.one * percent;
    public void Flip180() => transform.localRotation = Quaternion.Euler(0, 0, 180);
}
