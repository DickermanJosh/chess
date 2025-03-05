using Render;
using UnityEngine;
using UnityEngine.InputSystem;
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

            board = new Board(64);
            board.Init();
        }

        private void Start()
        {
            board.LoadFEN(FENUtils.StartFen);
        }

        // Read-only access to the board
        public Board GetBoard()
        {
            return board;
        }
    }

}