using Core;
public class Move
{
    // TODO: Make this whole system more efficient later, potentially using bits to keep track of to-from info and move flags
    // public bool MadeByWhite;

    public Square From;
    public Square To;
    PieceColor Color;

    public Move(Square from, Square to, PieceColor color)
    {
        From = from; 
        To = to;
        Color = color;
    }

    override public string ToString()
    {
        return $"{From.Coord}|{To.Coord}";
    }
}
