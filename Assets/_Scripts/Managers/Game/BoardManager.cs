using Render;
using Core;
using UnityEngine;
using UnityEngine.EventSystems;
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

        }

        private void Start()
        {
            if (Camera.main != null && Camera.main.GetComponent<Physics2DRaycaster>() == null)
                Camera.main.gameObject.AddComponent<Physics2DRaycaster>();
            BoardRenderer.Instance.RenderBoardSquares(GameManager.Instance.GameState.Board);
            if (GameManager.Instance.MyColor == PieceColor.Black)
            {
                BoardRenderer.Instance.FlipPerspective(GameManager.Instance.GameState.Board);
            } 
        }

    }
}
