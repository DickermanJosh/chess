using Core;
using System;
using System.Collections.Generic;
using UnityEngine;
public static class FENUtils
{
    public const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public const string EmptyFen = "8/8/8/8/8/8/8/8 w KQkq - 0 1";

    static Dictionary<char, PieceType> pieceTypeFromSymbol = new Dictionary<char, PieceType>()
    {
        ['k'] = PieceType.King,
        ['p'] = PieceType.Pawn,
        ['n'] = PieceType.Knight,
        ['b'] = PieceType.Bishop,
        ['r'] = PieceType.Rook,
        ['q'] = PieceType.Queen
    };

    public static string EnPassantSquare = "-";

    public static PieceType CharToPieceType(char pieceType)
    {
        return pieceTypeFromSymbol[pieceType];
    }

    #region Applying FEN to GameStates
    /// <summary>
    /// Meant to be called by clients when receiving a new FEN from the server.
    /// Will update the game state to match piece placement, castling rights, en passant square, move order
    /// </summary>
    public static List<int> ParseFenString(GameState gameState, string fen)
    {
        string[] segments = fen.Split(' ');
        string piecePlacement = segments[0];
        string colorToMove = segments[1];
        string castlingRights = segments[2];
        string enPassantSquare = segments[3];
        string fullMoveClock = segments[4];
        string halfMoveClock = segments[5];

        List<int> changedSquares = ParsePiecePlacementSegment(piecePlacement, gameState.Board);
        ParseColorToMoveSegment(gameState, colorToMove);
        ParseCastlingRightsSegment(gameState, castlingRights);
        ParseEnPassantSquareSegment(gameState, enPassantSquare);
        ParseFullClockSegment(gameState, fullMoveClock);
        ParseHalfClockSegment(gameState, halfMoveClock);

        return changedSquares;
    }


    public static List<int> ParsePiecePlacementSegment(string piecePlacement, Board board)
    {
        // FEN has 8 segments (split by '/') from top rank (7) down to bottom rank (0)
        string[] ranks = piecePlacement.Split('/');

        List<int> changedSquares = new List<int>();

        // "rankFenIndex" = 0 means the top row in the FEN, which is "rank = 7" in 0-based
        for (int rankFenIndex = 0; rankFenIndex < 8; rankFenIndex++)
        {
            int rank = 7 - rankFenIndex; // map top rank (fen) => rank=7, next =>6,...0

            string rankString = ranks[rankFenIndex];
            int file = 0;

            foreach (char c in rankString)
            {
                if (char.IsDigit(c))
                {
                    // Handle squares that need to be cleared if leftover pieces remain
                    // This can happen if the FEN says e.g. '4' in a rank, meaning 4 empty squares.
                    int emptyCount = c - '0';
                    for (int skip = 0; skip < emptyCount; skip++)
                    {
                        int index = rank * 8 + file;
                        Square sq = board.squares[index];
                        if (sq.Piece.GetType() != PieceType.None || sq.Piece.GetColor() != PieceColor.None)
                        {
                            // It's not empty, so clear it
                            sq.RemovePieceFromSquare();
                            board.squares[index] = sq;
                            changedSquares.Add(index);
                        }
                        file++;
                    }
                }
                else
                {
                    // It's a piece symbol. Determine color & type
                    bool isUpper = char.IsUpper(c);
                    PieceColor color = isUpper ? PieceColor.White : PieceColor.Black;

                    // Convert the letter (lowercased or uppercased) to a piece type
                    char pieceChar = char.ToLower(c);
                    PieceType type = FENUtils.CharToPieceType(pieceChar);

                    int index = rank * 8 + file;
                    Square sq = board.squares[index];

                    // Compare with what's currently there
                    PieceType oldType = sq.Piece.GetType();
                    PieceColor oldColor = sq.Piece.GetColor();

                    if (oldType != type || oldColor != color)
                    {
                        // Something changed. Update the square's piece
                        sq.AddPieceToSquare(type, color);
                        board.squares[index] = sq;
                        changedSquares.Add(index);
                    }

                    file++;
                }
            }
        }

        return changedSquares;
    }

    public static void ParseColorToMoveSegment(GameState gameState, string fenSegment)
    {
        if (fenSegment.Equals("w"))
        {
            gameState.ColorToMove = PieceColor.White;
            return;
        }
        else if (fenSegment.Equals("b"))
        {
            gameState.ColorToMove = PieceColor.Black;
            return;
        }

        Debug.Log($"[FENUtils] Unable to parse color to move segment. Received: {fenSegment}");
    }

    private static void ParseCastlingRightsSegment(GameState gameState, string fenSegment)
    {
        if (fenSegment.Contains('K'))
            gameState.WhiteKingSideCastle = true;
        else
            gameState.WhiteKingSideCastle = false;

        if (fenSegment.Contains('Q'))
            gameState.WhiteQueenSideCastle = true;
        else
            gameState.WhiteQueenSideCastle = false;

        if (fenSegment.Contains('k'))
            gameState.BlackKingSideCastle = true;
        else
            gameState.BlackKingSideCastle = false;

        if (fenSegment.Contains('q'))
            gameState.BlackQueenSideCastle = true;
        else
            gameState.BlackQueenSideCastle = false;
    }

    private static void ParseEnPassantSquareSegment(GameState gameState, string fenSegment)
    {
        gameState.EnPassantSquare = fenSegment;
    }
    private static void ParseHalfClockSegment(GameState gameState, string fenSegment)
    {
        Int32.TryParse(fenSegment, out int x);
        gameState.HalfMoves = x;
    }
    private static void ParseFullClockSegment(GameState gameState, string fenSegment)
    {
        Int32.TryParse(fenSegment, out int x);
        gameState.FullMoves = x;
    }

    #endregion

    #region Generating FEN from GameStates

    public static string GenerateFen(GameState gameState)
    {
        Board board = gameState.Board;

        // 1) Piece Placement (top rank = 7 down to rank = 0)
        string piecePlacement = BuildPiecePlacement(board);

        // 2) Active color
        char activeColorChar = (gameState.ColorToMove == PieceColor.White) ? 'w' : 'b';

        // 3) Castling rights
        string castlingString = BuildCastlingString(gameState);
        // If empty => '-'
        if (string.IsNullOrEmpty(castlingString))
            castlingString = "-";

        // 4) En passant target
        string enPassant = gameState.EnPassantSquare;
        if (string.IsNullOrEmpty(enPassant) || enPassant.Equals("-"))
            enPassant = "-";

        // 5) Halfmove clock (aka "move50" in some references)
        int halfMoveClock = gameState.HalfMoves;

        // 6) Fullmove number
        int fullMoveNumber = gameState.FullMoves;
        // NOTE: Typically in FEN, the "fullmove number" starts at 1 and increments after Black's move.
        // Make sure your game logic sets/updates gameState.FullMoves accordingly.

        // Combine everything: <piecePlacement> <activeColor> <castling> <enPassant> <halfmove> <fullmove>
        return $"{piecePlacement} {activeColorChar} {castlingString} {enPassant} {halfMoveClock} {fullMoveNumber}";
    }

    /// <summary>
    /// Builds the piece placement string (the first field of FEN) by scanning
    /// each rank from top (7) to bottom (0), left to right.
    /// </summary>
    private static string BuildPiecePlacement(Board board)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int rank = 7; rank >= 0; rank--)
        {
            int emptyCount = 0;

            for (int file = 0; file < 8; file++)
            {
                int index = rank * 8 + file;
                Square sq = board.squares[index];
                Piece piece = sq.Piece;

                if (piece.GetType() == PieceType.None)
                {
                    // empty
                    emptyCount++;
                }
                else
                {
                    // if we have some empties pending, write them out
                    if (emptyCount > 0)
                    {
                        sb.Append(emptyCount);
                        emptyCount = 0;
                    }

                    // Convert piece type & color to FEN char
                    char fenChar = PieceToFenChar(piece.GetType(), piece.GetColor());
                    sb.Append(fenChar);
                }
            }

            // end of file loop
            // if there are leftover empty squares
            if (emptyCount > 0)
            {
                sb.Append(emptyCount);
            }

            // if not the last rank, append '/'
            if (rank > 0)
            {
                sb.Append('/');
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Builds the castling rights portion (e.g. "KQkq" or "KQ" or "-").
    /// </summary>
    private static string BuildCastlingString(GameState gameState)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        if (gameState.WhiteKingSideCastle) sb.Append('K');
        if (gameState.WhiteQueenSideCastle) sb.Append('Q');
        if (gameState.BlackKingSideCastle) sb.Append('k');
        if (gameState.BlackQueenSideCastle) sb.Append('q');

        return sb.ToString();
    }

    /// <summary>
    /// Converts a piece type and color to the correct FEN character.
    /// White pieces are uppercase, black pieces are lowercase.
    /// </summary>
    private static char PieceToFenChar(PieceType type, PieceColor color)
    {
        // We'll pick an uppercase base char for white, then toLower if black.
        char c = ' ';

        switch (type)
        {
            case PieceType.Pawn: c = 'P'; break;
            case PieceType.Knight: c = 'N'; break;
            case PieceType.Bishop: c = 'B'; break;
            case PieceType.Rook: c = 'R'; break;
            case PieceType.Queen: c = 'Q'; break;
            case PieceType.King: c = 'K'; break;
            case PieceType.None: c = ' '; break; // Should never happen here
        }

        if (color == PieceColor.Black)
            c = char.ToLower(c);

        return c;
    }


    #endregion
}
