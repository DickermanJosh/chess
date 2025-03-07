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
            BoardRenderer.Instance.RenderBoardSquares(board);
        }

        private void Start()
        {
            List<int> changedSquares = board.LoadFEN(FENUtils.StartFen);
            BoardRenderer.Instance.RenderChangedSquares(changedSquares, board);
        }

        // Read-only access to the board
        public Board GetBoard()
        {
            return board;
        }
    }

}