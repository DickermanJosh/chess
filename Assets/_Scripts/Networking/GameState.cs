using Core;

public class GameState
{
    public Board Board { get; set; }
    public PieceColor ColorToMove { get; set; }
    public bool IsGameOver { get; set; }
    public int HalfMoves { get; set; }
    public int FullMoves { get; set; }
    public bool WhiteKingSideCastle { get; set; }
    public bool WhiteQueenSideCastle { get; set; }
    public bool BlackKingSideCastle { get; set; }
    public bool BlackQueenSideCastle { get; set; }
    public string EnPassantSquare { get; set; }
    public string CurrentFen { get; set; }
    public MoveTracker MoveTracker { get; private set; }

    public GameState()
    {
        Board = new Board(64);
        Board.Init();

        Board.LoadPiecesFromFen(FENUtils.StartFen);
        CurrentFen = FENUtils.StartFen;

        ColorToMove = PieceColor.White;

        IsGameOver = false;

        HalfMoves = 1;
        FullMoves = 0;

        WhiteKingSideCastle = true;
        WhiteQueenSideCastle = true;
        BlackKingSideCastle = true;
        BlackQueenSideCastle = true;

        EnPassantSquare = "-";

        MoveTracker = new MoveTracker();
    }

    public void UpdateMoveOrder()
    {
        HalfMoves++;

        if (HalfMoves % 2 == 0)
        {
            FullMoves++;
        }

        if (ColorToMove == PieceColor.White)
        {
            ColorToMove = PieceColor.Black;
        }
        else
        {
            ColorToMove = PieceColor.White;
        }
    }
}
