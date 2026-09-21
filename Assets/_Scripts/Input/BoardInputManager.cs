using Core;
using Opera;
using Render;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoardInputManager : MonoBehaviour
{
    public static BoardInputManager Instance { get; private set; }
    public Square SelectedSquare => selectedSquare;
    public bool IsDragging => draggedPiece != null;
    private Square selectedSquare, hoveredSquare;
    private PieceRenderer draggedPiece;
    private int dragPointerId;

    private bool CanInteract => GameManager.Instance != null && GameManager.Instance.IsMyTurn() &&
        (!GameManager.Instance.IsEngineGame ||
         (OperaGameController.Instance != null && OperaGameController.Instance.CanHumanMove));

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(Instance);
        Instance = this;
    }
    private void Update()
    {
        if ((selectedSquare != null && !CanInteract) || Keyboard.current?.escapeKey.wasPressedThisFrame == true ||
            Mouse.current?.rightButton.wasPressedThisFrame == true) UnselectSquare();
    }
    public void OnSquareClicked(Square square)
    {
        if (square == null || !CanInteract || IsDragging) return;
        if (selectedSquare == square) { UnselectSquare(); return; }
        if (selectedSquare != null && TryMove(square)) return;
        Select(square);
    }
    private void Select(Square square)
    {
        UnselectSquare();
        if (!CanInteract || square == null || square.Piece.GetColor() != GameManager.Instance.MyColor ||
            square.Piece.GetType() == PieceType.None) return;
        selectedSquare = square;
        square.Renderer?.SetSelected(true);
        var state = GameManager.Instance.GameState;
        foreach (var target in LegalMovesHandler.FindLegalMoves(state, square))
        {
            target.Renderer?.AddHighlight(BoardMoveIntent.IsCapture(state, square, target));
            if (square.Piece.GetType() == PieceType.King && Mathf.Abs(square.Coord.file - target.Coord.file) == 2)
            {
                var rook = state.Board.GetSquareFromIndex(square.Coord.rank * 8 + (target.Coord.file > square.Coord.file ? 7 : 0));
                rook.Renderer?.AddHighlight(false, true);
            }
        }
        OperaAudio.Play(OperaSound.Lift);
    }
    private bool TryMove(Square target)
    {
        if (!CanInteract || selectedSquare == null || target == null) return false;
        var state = GameManager.Instance.GameState;
        var destination = BoardMoveIntent.Destination(state, selectedSquare, target);
        if (!LegalMovesHandler.IsMoveLegal(state, destination, selectedSquare)) return false;
        var move = new Move(selectedSquare, destination, GameManager.Instance.MyColor);
        if (GameManager.Instance.GetMyPlayer().OnMove(move) < 0) return false;
        UnselectSquare(); return true;
    }
    public bool BeginDrag(Square square, int pointerId)
    {
        if (!CanInteract || IsDragging || square == null || square.Piece.GetColor() != GameManager.Instance.MyColor ||
            square.Piece.GetType() == PieceType.None) return false;
        if (selectedSquare != square) Select(square);
        else OperaAudio.Play(OperaSound.Lift);
        draggedPiece = BoardRenderer.Instance.GetRendererFromIndex(square.Index);
        if (draggedPiece == null) return false;
        dragPointerId = pointerId; draggedPiece.SetLifted(true); return true;
    }
    public void DragTo(Vector2 screenPosition, Square target, int pointerId)
    {
        if (!IsDragging || pointerId != dragPointerId) return;
        if (!CanInteract) { UnselectSquare(); return; }
        var camera = Camera.main;
        if (camera != null)
        {
            var position = camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -camera.transform.position.z));
            draggedPiece.transform.position = new Vector3(position.x, position.y, 0);
        }
        if (hoveredSquare == target) return;
        if (hoveredSquare?.Renderer != null) hoveredSquare.Renderer.SetDropTarget(false); hoveredSquare = target;
        if (target == null) return;
        var state = GameManager.Instance.GameState;
        var destination = BoardMoveIntent.Destination(state, selectedSquare, target);
        if (LegalMovesHandler.IsMoveLegal(state, destination, selectedSquare)) target.Renderer?.SetDropTarget(true);
    }
    public void EndDrag(Square target, int pointerId)
    {
        if (!IsDragging || pointerId != dragPointerId) return;
        ReturnPiece();
        if (target != null && target != selectedSquare && TryMove(target)) return;
        // Dropping on the origin, outside the board, or over UI preserves the
        // position and selection, so the player can try again without a click.
        OperaAudio.Play(OperaSound.Return);
    }
    private void ReturnPiece()
    {
        if (draggedPiece != null) draggedPiece.SetLifted(false);
        draggedPiece = null;
        if (hoveredSquare?.Renderer != null) hoveredSquare.Renderer.SetDropTarget(false); hoveredSquare = null;
    }
    public void UnselectSquare()
    {
        ReturnPiece();
        if (selectedSquare?.Renderer != null) selectedSquare.Renderer.SetSelected(false); selectedSquare = null;
        if (GameManager.Instance?.GameState == null) return;
        foreach (var square in GameManager.Instance.GameState.Board.squares)
            if (square.Renderer != null) square.Renderer.RemoveHighlight();
    }
    private void OnApplicationFocus(bool focused) { if (!focused) UnselectSquare(); }
    private void OnDisable() => UnselectSquare();
    private void OnDestroy() { if (Instance == this) Instance = null; }
}
