using Render;
using Core;
using UnityEngine;
using System.Collections.Generic;

namespace Managers
{
    /// <summary>
    /// Responsible for the execution of the clients board logic internally and on screen
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        private static BoardManager _instance;
        public static BoardManager Instance => _instance;

        private void Awake()
        {
            if (_instance is not null)
            {
                Destroy( _instance );
            }

            _instance = this;

            BoardRenderer.Instance.RenderBoardSquares(GameManager.Instance.GameState.Board);
        }

        private void Start()
        {
            if (GameManager.Instance.MyColor == PieceColor.Black)
            {
                BoardRenderer.Instance.FlipPerspective(GameManager.Instance.GameState.Board);
            } 
            // List<int> changedSquares = localBoard.LoadFEN(FENUtils.StartFen);
            // List<int> changedSquares = localBoard.LoadFEN(FENUtils.StartFen);
            // BoardRenderer.Instance.RenderChangedSquares(changedSquares, localBoard);
        }

        /// <summary>
        /// Performs some client side validation then sends the desired move to the server for
        /// verification.
        /// Does not handle server response
        /// </summary>
        /// <param name="fromSquare"></param>
        /// <param name="toSquare"></param>
        public void TryMovePiece(Square fromSquare, Square toSquare)
        {
            // Check to make sure that:
            // 1. The squares selected are not the same square
            // 2. It is the correct players turn
            // 3. The piece being moved is the right color
            if (toSquare == fromSquare ||
                GameManager.Instance.MyColor != GameManager.Instance.GameState.ColorToMove ||
                GameManager.Instance.MyColor != fromSquare.Piece.GetColor())
            {
                BoardInputManager.Instance.UnselectSquare();
                return;
            }

            Debug.Log($"TryMovePiece(): [{fromSquare.Coord}->{toSquare.Coord}]");

            // 1. Perform Local move validation to confirm legal move
            Square[] legalMoves = LegalMovesHandler.FindPseudoLegalMoves(fromSquare);
            bool moveIsLegal = false;
            foreach (Square move in legalMoves)
            {
                if (move == toSquare)
                {
                    moveIsLegal = true;
                    break;
                }
            }

            if (!moveIsLegal)
            {
                BoardInputManager.Instance.UnselectSquare();
                return;
            }

            // 2. Send the move to the server e.g "MOVE|e2|e4"
            ClientMessageHelper.SendMove(fromSquare, toSquare);

            // End move client side to prevent sending multiple messages
            if (GameManager.Instance.MyColor != PieceColor.White)
            {
                GameManager.Instance.GameState.ColorToMove = PieceColor.White;
            }
            else
            {
                GameManager.Instance.GameState.ColorToMove = PieceColor.Black;
            }
            
            // 3. SERVER-SIDE check will happen to confirm validity of move (Playercolor and valid move gen)
            // 4. Server Will either respond with BAD_MOVE or FEN
            // if BAD_MOVE do nothing
            // If FEN Update local gamestate and board renderer to match

            // Update your board data
            //Board board = GameManager.Instance.GameState.Board;
            //toSquare.AddPieceToSquare(fromSquare.Piece.GetType(), fromSquare.Piece.GetColor());
            //fromSquare.RemovePieceFromSquare();

            //// Re-render just those squares
            //List<int> changed = new List<int>() { fromSquare.Index, toSquare.Index };
            //BoardRenderer.Instance.RenderChangedSquares(changed, board);
        }

        public void UpdateLocalBoard(string fen)
        {
            List<int> changedSquares = GameManager.Instance.GameState.Board.LoadFEN(fen);
            BoardRenderer.Instance.RenderChangedSquares(changedSquares, GameManager.Instance.GameState.Board);
        }
    }

}