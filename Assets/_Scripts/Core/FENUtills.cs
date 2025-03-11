using Core;
using System.Collections.Generic;

public static class FENUtils
{
    public const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public const string EmptyFen = "8/8/8/8/8/8/8/8 w KQkq - 0 1";

    static Dictionary<char, PieceType> pieceTypeFromSymbol = new Dictionary<char, PieceType>()
    {
        ['k'] = PieceType.King,
        ['p'] = PieceType.Pawn,
        ['n'] = PieceType.Knight,
        ['b'] = PieceType.Bishop,
        ['r'] = PieceType.Rook,
        ['q'] = PieceType.Queen
    };

    public static string EnPassantSquare = "-";

    public static PieceType CharToPieceType(char pieceType)
    {
        return pieceTypeFromSymbol[pieceType];
    }

    public static List<int> ParsePiecePlacementSegment(string piecePlacement, Board board)
    {
        // FEN has 8 segments (split by '/') from top rank (7) down to bottom rank (0)
        string[] ranks = piecePlacement.Split('/');

        List<int> changedSquares = new List<int>();

        // "rankFenIndex" = 0 means the top row in the FEN, which is "rank = 7" in 0-based
        for (int rankFenIndex = 0; rankFenIndex < 8; rankFenIndex++)
        {
            int rank = 7 - rankFenIndex; // map top rank (fen) => rank=7, next =>6,...0

            string rankString = ranks[rankFenIndex];
            int file = 0;

            foreach (char c in rankString)
            {
                if (char.IsDigit(c))
                {
                    // Handle squares that need to be cleared if leftover pieces remain
                    // This can happen if the FEN says e.g. '4' in a rank, meaning 4 empty squares.
                    int emptyCount = c - '0';
                    for (int skip = 0; skip < emptyCount; skip++)
                    {
                        int index = rank * 8 + file;
                        Square sq = board.squares[index];
                        if (sq.Piece.GetType() != PieceType.None || sq.Piece.GetColor() != PieceColor.None)
                        {
                            // It's not empty, so clear it
                            sq.RemovePieceFromSquare();
                            board.squares[index] = sq;
                            changedSquares.Add(index);
                        }
                        file++;
                    }
                }
                else
                {
                    // It's a piece symbol. Determine color & type
                    bool isUpper = char.IsUpper(c);
                    PieceColor color = isUpper ? PieceColor.White : PieceColor.Black;

                    // Convert the letter (lowercased or uppercased) to a piece type
                    char pieceChar = char.ToLower(c);
                    PieceType type = FENUtils.CharToPieceType(pieceChar);

                    int index = rank * 8 + file;
                    Square sq = board.squares[index];

                    // Compare with what's currently there
                    PieceType oldType = sq.Piece.GetType();
                    PieceColor oldColor = sq.Piece.GetColor();

                    if (oldType != type || oldColor != color)
                    {
                        // Something changed. Update the square's piece
                        sq.AddPieceToSquare(type, color);
                        board.squares[index] = sq;
                        changedSquares.Add(index);
                    }

                    file++;
                }
            }
        }

        return changedSquares;
    }
}
