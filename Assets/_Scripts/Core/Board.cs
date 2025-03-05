using Render;
using System.Collections.Generic;

namespace Core
{
    /*
     *  Internal representation of the chess board. Does NOT draw to screen
     *  Represents squares & pieces in play
     */
    public struct Board
    {
        public Square[] squares;

        public Board(int size)
        {
            squares = new Square[size];
        }

        public void Init()
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

            BoardRenderer.Instance.RenderBoardSquares(this);
        }

        /// <summary>
        /// Parse the FEN string and update only changed squares.
        /// Board is updated both in memory and on screen.
        /// 
        /// Example FEN: "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        /// </summary>
        public void LoadFEN(string fen)
        {
            // For now, just parse up to the first space (piece placement only).
            string[] parts = fen.Split(' ');
            string piecePlacement = parts[0];

            List<int> changedSquares = FENUtils.ParsePiecePlacementSegment(piecePlacement, this);

            foreach (int sqIndex in changedSquares)
            {
                // Re-render only that square's piece
                BoardRenderer.Instance.RenderPieceOnBoard(squares[sqIndex]);
            }

        }

        public void UpdateDisplay()
        {
            
        }
    }
}
