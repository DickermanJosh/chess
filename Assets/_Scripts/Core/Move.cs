using Core;
public struct Move
{
    // TODO: Make this whole system more efficient later, potentially using bits to keep track of to-from info and move flags
    public bool MadeByWhite;

    public Square From;
    public Square To;

    // public string Notation;

    public Move(bool madeByWhite, Square from, Square to)
    {
        MadeByWhite = madeByWhite;
        From = from; 
        To = to;

    }
}
