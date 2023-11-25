using TMPro;
using UnityEngine;

public class MoveTracker : MonoBehaviour
{
    private static int
        MovesPlayed; //  Full move number: The number of the full moves. It starts at 1 and is incremented after Black's move.

    private static int
        HalfMovesPlayed; // Half move clock: The number of half moves since the last capture or pawn advance, used for the fifty-move rule.

    public static bool IsWhiteToMove;
    private static string lastMove;
    private static string _lastMoveTo;
    private static string _lastMoveFrom;
    public static int FromBoardPos;
    public static int ToBoardPos;
    public TextMeshProUGUI moveDisplay;

    private static MoveTracker _instance;
    public static MoveTracker Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    private void Start()
    { 
        IsWhiteToMove = true; // If WhiteToMove is not defined here the initial text display will be reversed for the first full move
        MovesPlayed = 0;
        DisplayPlayerToMove();
        //LegalMovesHandler.SquaresAttackedByWhite();
        //LegalMovesHandler.SquaresAttackedByBlack();
    }

    public void Move(string to, string from, int boardPosTo, int boardPosFrom)
    {
        IsWhiteToMove = !IsWhiteToMove;
        HalfMovesPlayed++;
        DisplayPlayerToMove();
        //LegalMovesHandler.SquaresAttackedByWhite();
        //LegalMovesHandler.SquaresAttackedByBlack();

        if (IsWhiteToMove)
            MovesPlayed++;

        _lastMoveFrom = from;
        _lastMoveTo = to;
        lastMove = _lastMoveFrom + "-" + _lastMoveTo;

        FromBoardPos = boardPosFrom;
        ToBoardPos = boardPosTo;

        Debug.Log(FromBoardPos + " - " + ToBoardPos);
    }

    private void DisplayPlayerToMove()
    {
        moveDisplay.text = IsWhiteToMove ? "White to Move" : "Black to Move";
    }
}