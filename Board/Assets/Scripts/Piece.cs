using UnityEngine;

public class Piece : MonoBehaviour
{
    public string type;
    public bool isWhitePiece;
    public Vector2 pos;
    private SpriteRenderer _spriteRenderer;

    public Sprite[] pieceSprites = new Sprite[12];

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        SetPieceSprite(type,isWhitePiece);
    }

    private void SetPieceSprite(string pieceType, bool isWhite)
    {
        string spriteName;
        //string spriteName = !isWhite ? "white" + pieceType : "black" + pieceType;
        if (!isWhite)
        {
            spriteName = "Black" + pieceType;
        }
        else
        {
            spriteName = "White" + pieceType;
        }
        
        //Sprite sprite = pieceSprites.GetSprite(spriteName);
        _spriteRenderer.sprite = GetSpriteByName(spriteName);
    }

    private Sprite GetSpriteByName(string spriteName)
    {
        for (int i = 0; i < pieceSprites.Length; i++)
        {
            if (pieceSprites[i].name == spriteName)
            {
                return pieceSprites[i];
            }
        }
        Debug.Log("No sprite found with name: " + spriteName);
        return null;
    }
}