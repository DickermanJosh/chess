namespace Core
{
    public struct Piece
    {
        PieceType type;
        PieceColor color;

        // Default constructor logic is as follows:
        //public Piece()
        //{
        //    this.type = PieceType.None;
        //    this.color = PieceColor.None;
        //}

        public Piece(PieceType type, PieceColor color)
        {
            this.type = type;
            this.color = color;
        }

        public void SetType(PieceType type)
        {
            this.type = type;
        }

        public void SetColor(PieceColor color)
        {
            this.color = color;
        }

        new public readonly PieceType GetType()
        {
            return type;
        }

        public readonly PieceColor GetColor()
        {
            return color;
        }

        public void SetTypeAndColor(PieceType type, PieceColor color)
        {
            this.type = type;
            this.color = color;
        }

        public void SetToNone()
        {
            this.type = PieceType.None;
            this.color = PieceColor.None;
        }

        override public readonly string ToString()
        {
            return $"{color}{type}";
        }
    }
    
    /*
     *  ENUM definitions to define piece types and colors
     */
    public enum PieceType
    {
         None,
         King,
         Queen,
         Rook,
         Bishop,
         Knight,
         Pawn
    }

    public enum PieceColor
    {
        None,
        White,
        Black
    }
}