using Core;
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

        [Header("Piece Sprites (Simple Example)")]
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

        // Singleton declaration
        private static BoardRenderer _instance;
        public static BoardRenderer Instance => _instance;

        private void Awake()
        {
            if (_instance is not null)
            {
                Destroy(_instance);
            }

            _instance = this;
        }

        /// <summary>
        /// Spawns SquareRenderer objects as children and assigns the correct sprite/color/position.
        /// </summary>
        public void RenderBoardSquares(Board board)
        {
            if (board is null || board.squares is null)
            {
                Debug.LogError("BoardRenderer: The board data is not initialized.");
                return;
            }

            foreach (Square square in board.squares)
            {
                CreateSquareRenderer(square);
            }
        }

        private void CreateSquareRenderer(Square square)
        {
            // Create child at runtime, attach a square renderer, assign it a color and initialize it
            GameObject squareObj = new GameObject("Square");
            squareObj.transform.parent = this.transform;

            SquareRenderer renderer = squareObj.AddComponent<SquareRenderer>();
            Color squareColor = square.IsWhite ? whiteSquareColor : blackSquareColor;

            renderer.Init(square, defaultSquareSprite, squareColor, squareSize);
        }

        /// <summary>
        /// Spawns PieceRenderer objects for all squares that currently hold a piece.
        /// </summary>
        public void RenderPiecesOnBoard(Board board)
        {
            if (board == null || board.squares == null)
            {
                Debug.LogError("BoardRenderer: The board data is not initialized.");
                return;
            }
            
            foreach (Square square in board.squares)
            {
                CreatePieceRenderer(square);
            }
        }

        private void CreatePieceRenderer(Square square)
        {

            Debug.Log($"square {square.Coord.file} {square.Coord.rank} has Piece: {square.Piece.ToString()}");
            // Create a child object for the piece
            GameObject pieceObj = new GameObject("PieceObject");
            pieceObj.transform.parent = this.transform;

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
