namespace Core
{
    public class Square
    {
        public int Index { get; }
        public Coord Coord { get; }
        public int[] AvailableDistancesToEdge { get; }
        public bool IsWhite { get; }
        public Piece Piece;
        public SquareRenderer Renderer { get; set;  }
        public Square(int index, Coord coord, int[] availableDistances, bool isWhite)
        {
            Index = index;
            Coord = coord;
            AvailableDistancesToEdge = availableDistances;
            IsWhite = isWhite;
            Piece = new Piece();
        }

        // Changes the square's already initialized piece object's type and color
        public void AddPieceToSquare(PieceType type, PieceColor color)
        {
            this.Piece.SetTypeAndColor(type, color);
        }

        // Keeps piece object initialized, params just set to (None, None) 
        public void RemovePieceFromSquare()
        {
            this.Piece.SetToNone();
        }
    }
}
