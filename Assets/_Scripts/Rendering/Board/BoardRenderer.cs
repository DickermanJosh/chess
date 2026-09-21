using Core;
using Opera;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Render
{
    public class BoardRenderer : MonoBehaviour
    {
        public Sprite defaultSquareSprite;
        public Color whiteSquareColor = Color.white, blackSquareColor = Color.black;
        public Sprite whitePawnSprite, whiteKnightSprite, whiteBishopSprite, whiteRookSprite, whiteQueenSprite, whiteKingSprite;
        public Sprite blackPawnSprite, blackKnightSprite, blackBishopSprite, blackRookSprite, blackQueenSprite, blackKingSprite;
        public float squareSize = 1;
        private readonly Dictionary<int, PieceRenderer> pieces = new();
        private readonly List<TextMeshPro> coordinates = new();
        public static BoardRenderer Instance { get; private set; }
        private void Awake() { if (Instance != null && Instance != this) Destroy(Instance); Instance = this; }

        public void RenderBoardSquares(Board board)
        {
            if (board.squares == null) return;
            BoardInputManager.Instance?.UnselectSquare();
            foreach (Transform child in transform) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            pieces.Clear(); coordinates.Clear(); transform.rotation = Quaternion.identity;
            if (Camera.main != null) Camera.main.backgroundColor = OperaTheme.Background;
            // Replace the persistent space backdrop if this scene came through Init.
            foreach (var background in FindObjectsByType<SpaceBackgroundRenderer>(FindObjectsSortMode.None))
                background.gameObject.SetActive(false);
            Frame();
            foreach (Square square in board.squares)
            {
                var squareObject = new GameObject("Square " + square.Coord);
                squareObject.transform.SetParent(transform, false);
                var renderer = squareObject.AddComponent<SquareRenderer>();
                renderer.Init(square, defaultSquareSprite, square.IsWhite ? OperaTheme.LightSquare : OperaTheme.DarkSquare, squareSize);
                square.Renderer = renderer;
                var pieceObject = new GameObject("Piece");
                pieceObject.transform.SetParent(squareObject.transform, false);
                var piece = pieceObject.AddComponent<PieceRenderer>();
                piece.Init(square.Piece, GetPieceSprite(square.Piece), Vector2.zero); pieces.Add(square.Index, piece);
            }
            for (int i = 0; i < 8; i++)
            {
                float position = (i - 3.5f) * squareSize;
                Coordinate(((char)('a' + i)).ToString(), new Vector2(position, -4.23f * squareSize));
                Coordinate((i + 1).ToString(), new Vector2(-4.23f * squareSize, position));
            }
        }
        private void Frame()
        {
            OperaTheme.World(transform, "Linen table", OperaTheme.Grain, Vector2.zero, Vector2.one * 100, OperaTheme.Background, -20);
            OperaTheme.World(transform, "Board shadow", OperaTheme.Pixel, new Vector2(.025f, -.09f), Vector2.one * (9.04f * squareSize), new Color(.06f, .04f, .025f, .45f), -8);
            OperaTheme.World(transform, "Walnut frame", OperaTheme.Wood, Vector2.zero, Vector2.one * (8.94f * squareSize), OperaTheme.Hex("513E2E"), -7);
            OperaTheme.World(transform, "Brass outer inlay", OperaTheme.Pixel, Vector2.zero, Vector2.one * (8.83f * squareSize), OperaTheme.Brass * new Color(.65f, .65f, .65f, 1), -6);
            OperaTheme.World(transform, "Frame face", OperaTheme.Wood, Vector2.zero, Vector2.one * (8.80f * squareSize), OperaTheme.Hex("4C392B"), -5);
            OperaTheme.World(transform, "Brass inner inlay", OperaTheme.Pixel, Vector2.zero, Vector2.one * (8.07f * squareSize), OperaTheme.Brass, -4);
        }
        private void Coordinate(string text, Vector2 position)
        {
            var label = new GameObject("Coordinate " + text, typeof(TextMeshPro)).GetComponent<TextMeshPro>();
            label.transform.SetParent(transform, false); label.transform.localPosition = position;
            label.rectTransform.sizeDelta = new Vector2(.4f, .4f) * squareSize;
            label.text = text; label.font = OperaTheme.Serif; label.fontSize = 2.6f * squareSize;
            label.color = OperaTheme.Ink; label.alignment = TextAlignmentOptions.Center;
            label.GetComponent<MeshRenderer>().sortingOrder = 3; coordinates.Add(label);
        }
        public void RenderChangedSquares(List<int> changedSquares, Board board)
        {
            BoardInputManager.Instance?.UnselectSquare();
            foreach (int index in changedSquares) RenderPieceOnBoard(board.squares[index]);
        }
        public void RenderPieceOnBoard(Square square)
        {
            if (square != null && pieces.TryGetValue(square.Index, out var renderer))
                renderer.ChangePiece(square.Piece, GetPieceSprite(square.Piece));
        }
        public PieceRenderer GetRendererFromIndex(int index) => pieces.TryGetValue(index, out var renderer) ? renderer : null;
        public void FlipPerspective(Board board)
        {
            transform.rotation = Quaternion.Euler(0, 0, 180);
            foreach (var piece in pieces.Values) piece.Flip180();
            // The board rotates, but coordinates stay on the visible bottom and
            // left edges. Their labels follow the files/ranks and remain upright.
            for (int i = 0; i < 8; i++)
            {
                float position = (i - 3.5f) * squareSize;
                coordinates[i * 2].transform.localPosition = new Vector2(position, 4.23f * squareSize);
                coordinates[i * 2 + 1].transform.localPosition = new Vector2(4.23f * squareSize, position);
            }
            foreach (var label in coordinates) label.transform.localRotation = Quaternion.Euler(0, 0, 180);
        }
        public Sprite GetPieceSprite(Piece piece)
        {
            bool white = piece.GetColor() == PieceColor.White;
            return piece.GetType() switch {
                PieceType.Pawn => white ? whitePawnSprite : blackPawnSprite,
                PieceType.Knight => white ? whiteKnightSprite : blackKnightSprite,
                PieceType.Bishop => white ? whiteBishopSprite : blackBishopSprite,
                PieceType.Rook => white ? whiteRookSprite : blackRookSprite,
                PieceType.Queen => white ? whiteQueenSprite : blackQueenSprite,
                PieceType.King => white ? whiteKingSprite : blackKingSprite, _ => null
            };
        }
        private void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
