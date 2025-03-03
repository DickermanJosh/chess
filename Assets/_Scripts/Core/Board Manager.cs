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
        [SerializeField] private BoardRenderer boardRenderer;

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

            board = new Board(64);
            board.InitEmptyBoard(); // Initilize board in memory
            boardRenderer.RenderBoardSquares(board); // Render the empty board
        }

        private void Start()
        {
            // Create piece renderers to render individual pieces
            // boardRenderer.CreatePieceRenderers(board); 

            board.squares[0].AddPieceToSquare(PieceType.Pawn, PieceColor.White);

            boardRenderer.RenderPieceOnBoard(board.squares[0]);
     
            
        }

        // Read-only access to the board
        public Board GetBoard()
        {
            return board;
        }
    }

}