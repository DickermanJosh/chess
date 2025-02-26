namespace Core
{
    /*
     *  Internal representation of the chess board. Does NOT draw to screen
     *  Represents squares & pieces in play
     */
    public class Board
    {
        public Square[] squares;

        public Board()
        {
            squares = new Square[64];
        }

        public void InitEmptyBoard()
        {
            for (int i = 0; i < 8; i++) // rank
            {
                for (int j = 0; j < 8; j++) // file
                {
                    int index = i * 8 + j;
                    Coord coord = new Coord(j, i);
                    bool isWhite = (i + j) % 2 != 0;
     
                    squares[index] = new Square(index, coord, isWhite);
                }
            }
        }

        public void PopulateBoardFromFEN(string fen)
        {
            return;
        }

        public void ClearBoard()
        {

        }
    }
}
