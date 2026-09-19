using Core;
public class Move
{
    // TODO: Make this whole system more efficient later, potentially using bits to keep track of to-from info and move flags
    // public bool MadeByWhite;

    public Square From;
    public Square To;
    PieceColor Color;
    public PieceType Promotion { get; set; }

    public Move(Square from, Square to, PieceColor color, PieceType promotion = PieceType.None)
    {
        From = from; 
        To = to;
        Color = color;
        Promotion = promotion;
    }

    public string ToUci()
    {
        string suffix = Promotion switch {
            PieceType.Queen => "q", PieceType.Rook => "r",
            PieceType.Bishop => "b", PieceType.Knight => "n", _ => ""
        };
        return $"{From.Coord}{To.Coord}{suffix}";
    }

    public static Move FromUci(GameState state, string text)
    {
        if (text == null || (text.Length != 4 && text.Length != 5) ||
            text[0] < 'a' || text[0] > 'h' || text[2] < 'a' || text[2] > 'h' ||
            text[1] < '1' || text[1] > '8' || text[3] < '1' || text[3] > '8')
            throw new System.FormatException("Invalid coordinate move: " + text);
        PieceType promotion = PieceType.None;
        if (text.Length == 5)
            promotion = text[4] switch {
                'q' => PieceType.Queen, 'r' => PieceType.Rook, 'b' => PieceType.Bishop,
                'n' => PieceType.Knight, _ => throw new System.FormatException("Invalid promotion: " + text)
            };
        return new Move(state.Board.GetSquareFromNotation(text.Substring(0, 2)),
            state.Board.GetSquareFromNotation(text.Substring(2, 2)), state.ColorToMove, promotion);
    }

    override public string ToString()
    {
        return $"{From.Coord}|{To.Coord}";
    }
}
