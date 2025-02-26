using Render;
using UnityEngine;
namespace Core
{
    /*
    *  Responsible for the execution of board logic internally and on screen
    */
    public class BoardManager : MonoBehaviour
    {
        private Board board;

        // Singleton declaration
        private static BoardManager _instance;
        public static BoardManager Instance => _instance;

        private void Awake()
        {
            if (_instance is not null)
            {
                Destroy( _instance );
            }

            _instance = this;

            board = new Board();
            board.InitEmptyBoard(); // Initilize board in memory
            BoardRenderer.Instance.RenderBoardSquares(board); // Render the empty board
        }

        private void Start()
        {
            Debug.Log($"Square 0 Piece: {board.squares[0].Piece.ToString()}");
            board.squares[0].AddPieceToSquare(PieceType.Pawn, PieceColor.White);

            Debug.Log($"Square 0 Piece: {board.squares[0].Piece.ToString()}");
            BoardRenderer.Instance.RenderPiecesOnBoard(board);
        }

        // Read-only access to the board
        public Board GetBoard()
        {
            return board;
        }
    }

}