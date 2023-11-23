using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FENHandler : MonoBehaviour
{
    private static readonly Dictionary<char, string> PieceTypeFromSymbol = new Dictionary<char, string>()
    {
        ['k'] = "King", ['p'] = "Pawn", ['n'] = "Knight", ['b'] = "Bishop", ['r'] = "Rook", ['q'] = "Queen"
    };

    private static string _fen = "";
    // public const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public const string StartFen = "rnbqkbnr/8/8/8/8/8/8/RNBQKBNR w KQkq - 0 1";

    // Reads the Board[] in BoardManager and outputs the current FEN string based on the state of the board
    public static string GetCurrentFenPos()
    {
        _fen = "";
        var rank = 7;
        var file = 0;
        var emptySquareCount = 0;

        for (var i = 0; i < 64; i++)
        {
            var currSq = BoardManager.Board[BoardManager.SquarePosInArrayFromVectorPos(new Vector2(file, rank))];
            if (currSq.isOccupied)
            {
                var pieceType = currSq.pieceOnSquare;
                var isWhite = currSq.isPieceWhite;

                if (emptySquareCount > 0)
                {
                    if (isWhite)
                        _fen += emptySquareCount + "" + char.ToUpper(GetDictSymbolFromPieceType(pieceType));
                    else
                        _fen += emptySquareCount + "" + GetDictSymbolFromPieceType(pieceType);
                    emptySquareCount = 0;
                }
                else
                {
                    if (isWhite)
                        _fen += "" + char.ToUpper(GetDictSymbolFromPieceType(pieceType));
                    else
                        _fen += "" + GetDictSymbolFromPieceType(pieceType);
                }

                if (file != 7)
                    file++;
                else
                {
                    rank--;
                    file = 0;
                    _fen += "/";
                }
            }
            else
            {
                if (file != 7)
                {
                    file++;
                    emptySquareCount++;
                }
                else
                {
                    emptySquareCount++;
                    rank--;
                    file = 0;
                    _fen += emptySquareCount + "/";
                    emptySquareCount = 0;
                }
            }
        }

        // Remove the last / at the end of the string (This is range index, similar to a substring of (0, len-1))
        _fen = _fen[..^1];

        //TODO: Add side to move, Castling rights, half move clock, full move clock
        if (MoveTracker.IsWhiteToMove)
            _fen += " w";
        else
            _fen += " b";
        return _fen;
    }

    public static void SetBoardFromFen(string fenString)
    {
        BoardManager.ClearPiecesFromBoard();

        // FEN notation starts at A8 and works its way through to B8 -> C8 ... H8 -> A7 ...
        var rank = 7;
        var file = 0;

        var
            sections = fenString.Split(' '); //TODO: Read the other lines when implementing rules and moves / move order
        foreach (var token in sections[0])
        {
            // Notifier to drop rank and reset file pos is '/'
            if (token == '/')
            {
                rank--;
                file = 0;
            }
            // Numerical value is used to move up in the file without adding any pieces to squares in between
            else if (char.IsDigit(token))
            {
                file += (int)char.GetNumericValue(token);
            }

            // Deciphering the desired piece types from the chars
            else
            {
                var pieceColor = char.IsUpper(token);
                var pieceType = PieceTypeFromSymbol[char.ToLower(token)];
                BoardManager.Board[BoardManager.SquarePosInArrayFromVectorPos(new Vector2(file, rank))]
                    .AddPieceToSquare(pieceType, pieceColor);
                file++;
            }
        }
    }

    private static char GetDictSymbolFromPieceType(string pieceType)
    {
        var symbol = PieceTypeFromSymbol.FirstOrDefault(x => x.Value == pieceType).Key;
        return symbol;
    }
}