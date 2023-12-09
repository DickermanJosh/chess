using UnityEngine;
using UnityEngine.Serialization;

public class Square : MonoBehaviour
{
    [Header("General Info")]
    public char file;
    public int rank;
    public bool isLightSquare;
    public bool isAttackedByWhite;
    public bool isAttackedByBlack;
    
    [Header("Other Positional Info")]
    public int BoardPosInArray;
    public Vector2 pos;
    public static readonly int[] Offsets = { 8, -8, -1, 1, 9, -9, 7, -7 };
    public int[] numOfSquaresToEdge = {0, 0, 0, 0, 0, 0, 0, 0};
    
    [Header("Piece Info")]
    public bool isOccupied;
    public bool isPieceWhite;
    public string pieceOnSquare;
    public bool canMove;
    public bool isPinned;
    public int pinnedOffset; // pinned offset will point in the direction of the pinned piece in the board array
    public bool isChecked;

    [Header("Unity Specific Setup")]
    public GameObject pieceToAdd;

    private GameObject _pieceObject;
    public Material defaultMaterial;
    public Material highlightMaterial;
    private Renderer _rend;

    public Square(int boardPosInArray)
    {
        this.BoardPosInArray = boardPosInArray;
    }

    private void Awake()
    {
        _rend = GetComponent<Renderer>();
        _rend.material = defaultMaterial;
    }

    public override int GetHashCode()
    {
        return BoardPosInArray.GetHashCode();
    }

    public override bool Equals(object other)
    {
        if (other == null || GetType() != other.GetType())
        {
            return false;
        }

        return BoardPosInArray == ((Square)other).BoardPosInArray;
    }

    public void AddPieceToSquare(string type, bool isWhite, bool isFlipped)
    {
        var position = transform.position;
        _pieceObject = Instantiate(pieceToAdd, new Vector3(position.x,position.y,-0.6f), Quaternion.identity);
        
        var pieceComponent = _pieceObject.GetComponent<Piece>();
         
        if (pieceComponent != null)
        {
            pieceComponent.isWhitePiece = isWhite;
            pieceComponent.type = type;
            pieceComponent.pos = pos;
            pieceComponent.isFlipped = isFlipped;
        }
        else
        {
            Debug.Log("Could not find piece.");
            return;
        }

        _pieceObject.transform.SetParent(transform); // TODO: Fix the parenting issue by creating a manager script to re-parent them all
        
        // Updating square info on the piece that currently occupies it
        isOccupied = true;
        isPieceWhite = pieceComponent.isWhitePiece;
        pieceOnSquare = pieceComponent.type;
    }

    public void RemovePieceFromSquare()
    {
        if(_pieceObject != null)
            Destroy(_pieceObject);
        
        pieceOnSquare = null;
        isPieceWhite = false;
        isOccupied = false;
        
        _rend.material = defaultMaterial;
    }

    private void OnMouseDown()
    {
        // If there is currently no piece selected and the wrong color is picked
        if (!LegalMovesHandler.IsPiecesTurn(isPieceWhite) && BoardManager.SelectedSquare == null)
            return;
        //  Attempting to click an empty square with no piece to move
        if (BoardManager.SelectedSquare == null && !isOccupied)
            return;
        // Attempting to click on a piece while waiting to promote a pawn
        if (PawnPromotionHandler.Instance.BlackPromotionTable.activeSelf ||
            PawnPromotionHandler.Instance.WhitePromotionTable.activeSelf) 
            return;
        BoardManager.SelectSquare(BoardPosInArray);
    }

    public void HighLightSquare(bool doHighLight)
    {
        _rend.material = doHighLight ? highlightMaterial : defaultMaterial;
    }
}
