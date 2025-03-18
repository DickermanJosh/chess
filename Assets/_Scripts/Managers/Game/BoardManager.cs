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
        }

    }
}