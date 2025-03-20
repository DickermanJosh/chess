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

    /// <summary>
    /// Validates if the given move is legal in the current gamestate.
    /// Updates the gamestates info by checking if the move being made is special in any way.
    /// (En passant, castling, etc).
    /// Then, the move is applied to the board
    /// </summary>
    public void ValidateUpdateAndAppleMove(Move move)
    {
        Square from = move.From;
        Square to = move.To;
        string validEnPassant = EnPassantSquare;

        if (!LegalMovesHandler.IsMoveLegal(this, to, from))
        {
            ServerMessageHelper.Log($"{move} deemed illegal by server.");
            return;
        }

        // Check for any special moves and 
        PawnMoveUtils.CheckIfMoveAllowsEnPassant(this, move);
        PawnMoveUtils.CheckIfMoveWasEnPassant(this, move, validEnPassant);
        KingMoveUtils.CheckIfMoveWasCastle(this, move);

        // make the move in the match's GameState
        Board.ApplyMove(from, to, validEnPassant);
        MoveTracker.AddMove(move);

        UpdateMoveOrder();

        string newFen = FENUtils.GenerateFen(this);
        CurrentFen = newFen;
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
