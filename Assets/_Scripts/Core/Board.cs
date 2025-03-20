using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

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
        /// 
        /// Example FEN: "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        /// What will be used here: "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR"
        /// </summary>
        public List<int> LoadPiecesFromFen(string fen)
        {
            // Just parse up to the first space (piece placement only).
            string[] parts = fen.Split(' ');
            string piecePlacement = parts[0];

            return FENUtils.ParsePiecePlacementSegment(piecePlacement, this);
        }

        /// <summary>
        /// Places the from's piece on the to square and removes the piece from the to square
        /// </summary>
        public readonly void ApplyMove(Square From, Square To, string EnPassantSquare)
        {
            Square _to = null;
            Square _from = null;

            foreach(var s in squares)
            {
                if (s.Equals(To)) { _to = s; }
                else if (s.Equals(From)) { _from = s; }

                if (_to != null && _from != null) { break; }
            }

            UpdatePieceOnSquare(_to, _from.Piece);
            RemovePieceFromSquare(_from); 
        }

        public readonly Square GetSquareFromIndex(int index)
        {
            foreach (var sq in squares)
            {
                if (sq.Index == index) { return sq; }
            }
            
            Debug.Log($"[Board] GetSquareFromIndex(int index) could not resolve index [{index}] into square");
            return null;
        }

        /// <summary>
        /// returns the square in the board from notation. e.g. 'e2' 
        /// </summary>
        /// <returns></returns>
        public Square GetSquareFromNotation(string squareNotation)
        {
            // if (squareNotation.Length != 2) { return null; }

            char fileChar = squareNotation[0];
            char rankChar = squareNotation[1];
                                          
            // int file = Coord.GetFileAsInt(fileAsChar);
            int file = fileChar - 'a';
            int rank = rankChar - '1';

            if (file < 0 || file > 7 || rank < 0 || rank > 7)
                return null; // out of standard chess bounds

            Coord coord = new Coord(file, rank);
            int index = coord.GetIndex();

            return GetSquareFromIndex(index);
        }

        public readonly Square FindKing(PieceColor kingColor)
        {
            foreach (var square in squares)
            {
                Piece p = square.Piece;
                if (p.GetType() == PieceType.King && p.GetColor() == kingColor)
                {
                    return square;
                }
            }

            return null;
        }

        public readonly Board Clone()
        {
            Board copy = this;
            return copy;
        }

        /// <summary>
        /// Places a piece on the given square in the board
        /// </summary>
        private readonly void UpdatePieceOnSquare(Square sq, Piece p)
        {
            if (sq == null) { return; }

            foreach(var square in squares)
            {
                if (square.Equals(sq))
                {
                    square.Piece = p;
                    return;
                }
            }

            Debug.Log($"[Board] Could not find square {sq} in the board");
        }

        public readonly void RemovePieceFromSquare(Square sq)
        {
            if (sq == null) { return; }
            if (sq.Piece.GetType() == PieceType.None) { return; }

            foreach(var square in squares)
            {
                if (square.Equals(sq))
                {
                    square.Piece = new Piece();
                    return;
                }
            }

            Debug.Log($"[Board] Could not find square {sq} in the board");
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
