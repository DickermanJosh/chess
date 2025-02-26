namespace Core
{
    public class Square
    {
        public int Index { get; }
        public Coord Coord { get; }
        public bool IsWhite { get; }
        public Piece Piece { get; set; }

        
        public Square(int index, Coord coord, bool isWhite)
        {
            Index = index;
            Coord = coord;
            IsWhite = isWhite;
            Piece = new Piece(PieceType.Rook, PieceColor.White); // White Rook for piece render testing
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
