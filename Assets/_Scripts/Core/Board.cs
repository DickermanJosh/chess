using Render;
using System;
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
        public static readonly int[] Offsets = { 8, -8, -1, 1, 9, -9, 7, -7 };

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
                    int[] distances = PrecomputeAvailableDistances(j, i);
                    bool isWhite = (i + j) % 2 != 0;
                    squares[index] = new Square(index, coord, distances, isWhite);
                }
            }
        }

        /// <summary>
        /// Parse the FEN string and update only changed squares.
        /// Board is updated both in memory and on screen.
        /// 
        /// Example FEN: "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        /// </summary>
        public List<int> LoadFEN(string fen)
        {
            // For now, just parse up to the first space (piece placement only).
            string[] parts = fen.Split(' ');
            string piecePlacement = parts[0];

            return FENUtils.ParsePiecePlacementSegment(piecePlacement, this);
        }

        public Square GetSquareFromIndex(int index)
        {
            foreach (Square square in squares)
            {
                if (square.Index == index) return square;
            }

            return null;
        }

        private static int[] PrecomputeAvailableDistances(int file, int rank)
        {
            var numUp = 7 - rank;
            var numRight = 7 - file;

            int[] numSquaresToEdge =
            {
            numUp,                      // Up
            rank,                       // Down
            file,                       // Left
            numRight,                   // Right
            Math.Min(numUp, numRight),  // Up-Right
            Math.Min(rank, file),       // Down-Left
            Math.Min(numUp, file),      // Up-Left
            Math.Min(rank, numRight)    // Down-Right
        };
            return numSquaresToEdge;
        }
    }
}
