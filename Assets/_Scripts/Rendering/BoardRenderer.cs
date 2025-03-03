using Core;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Render
{
    public class BoardRenderer : MonoBehaviour
    {
        [Header("References")]
        public Sprite defaultSquareSprite;

        [Header("Square Colors")]
        [Tooltip("Color tint for squares designated 'white'.")]
        public Color whiteSquareColor = Color.white;

        [Tooltip("Color tint for squares designated 'black'.")]
        public Color blackSquareColor = Color.black;

        [Header("Piece Sprites")]
        public Sprite whitePawnSprite;
        public Sprite whiteKnightSprite;
        public Sprite whiteBishopSprite;
        public Sprite whiteRookSprite;
        public Sprite whiteQueenSprite;
        public Sprite whiteKingSprite;

        public Sprite blackPawnSprite;
        public Sprite blackKnightSprite;
        public Sprite blackBishopSprite;
        public Sprite blackRookSprite;
        public Sprite blackQueenSprite;
        public Sprite blackKingSprite;

        [Header("Settings")]
        [Tooltip("How large each square is in world units")]
        public float squareSize = 1.0f;

        // List of all the active pieceRenderers in the sceen to prevent doubling up
        // renderers on the same square
        private Dictionary<int, PieceRenderer> pieceRenderers = new();

        /// <summary>
        /// Spawns SquareRenderer objects as children and assigns the correct sprite/color/position.
        /// Also creates PieceRenderer objects to render potential pieces on the squares
        /// </summary>
        public void RenderBoardSquares(Board board)
        {
            if (board.squares is null)
            {
                Debug.LogError("BoardRenderer: The board data is not initialized.");
                return;
            }

            foreach (Square square in board.squares)
            {
                CreateSquareAndPieceRenderer(square);
            }
        }

        /// <summary>
        /// Renders a piece on the board using the square's already created PieceRenderer
        /// Achieves this by swapping out the sprite stored in the PieceRenderer
        /// </summary>
        public void RenderPieceOnBoard(Square square)
        {
            if (square == null)
            {
                Debug.LogError("BoardRenderer: The board data is not initialized.");
                return;
            }
;
            PieceRenderer renderer = GetRendererFromIndex(square.Index);

            renderer.ChangePiece(square.Piece, GetPieceSprite(square.Piece));

        }

        private void CreateSquareAndPieceRenderer(Square square)
        {
            GameObject squareObj = CreateSquareRenderer(square);
            CreatePieceRenderer(square, squareObj);
        }

        private GameObject CreateSquareRenderer(Square square)
        {
            // Create child at runtime, attach a square renderer, assign it a color and initialize it
            GameObject squareObj = new GameObject("Square");
            squareObj.transform.parent = this.transform;

            SquareRenderer renderer = squareObj.AddComponent<SquareRenderer>();
            Color squareColor = square.IsWhite ? whiteSquareColor : blackSquareColor;

            renderer.Init(square, defaultSquareSprite, squareColor, squareSize);

            return squareObj;
        }

        private void CreatePieceRenderer(Square square, GameObject squareObj)
        {
            // Create a child object for the piece
            GameObject pieceObj = new GameObject("PieceObject");
            pieceObj.transform.parent = squareObj.transform;

            PieceRenderer pieceRenderer = pieceObj.AddComponent<PieceRenderer>();

            // Pick the correct sprite for the piece
            Sprite pieceSprite = GetPieceSprite(square.Piece);

            // The position is typically the same as the square's center
            float xPos = square.Coord.file * squareSize - 3.5f;
            float yPos = square.Coord.rank * squareSize - 3.5f;

            pieceRenderer.Init(
                square.Piece,
                pieceSprite,
                new Vector2(xPos, yPos)
            );

            pieceRenderers.Add(square.Index, pieceRenderer);
        }

        private PieceRenderer GetRendererFromIndex(int squareIndex)
        {
            if (pieceRenderers.TryGetValue(squareIndex, out PieceRenderer r))
            {
                return r;
            }

            return null;
        }

        /// <summary>
        /// Return the correct sprite for the given piece.
        /// For brevity, we do a simple switch. 
        /// In practice, you might prefer a Dictionary<(PieceType, PieceColor), Sprite> or something more robust.
        /// </summary>
        private Sprite GetPieceSprite(Piece piece)
        {
            if (piece.GetType() == PieceType.None || piece.GetColor() == PieceColor.None)
            {
                return null;
            }

            if (piece.GetColor() != PieceColor.Black)
            {
                switch (piece.GetType())
                {
                    case PieceType.Pawn: return whitePawnSprite;
                    case PieceType.Knight: return whiteKnightSprite;
                    case PieceType.Bishop: return whiteBishopSprite;
                    case PieceType.Rook: return whiteRookSprite;
                    case PieceType.Queen: return whiteQueenSprite;
                    case PieceType.King: return whiteKingSprite;
                }
            }
            else
            {
                switch (piece.GetType())
                {
                    case PieceType.Pawn: return blackPawnSprite;
                    case PieceType.Knight: return blackKnightSprite;
                    case PieceType.Bishop: return blackBishopSprite;
                    case PieceType.Rook: return blackRookSprite;
                    case PieceType.Queen: return blackQueenSprite;
                    case PieceType.King: return blackKingSprite;
                }
            }

            return null;
        }
    }
}
