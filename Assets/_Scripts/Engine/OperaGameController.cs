using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Opera;
using Render;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-200)]
public class OperaGameController : MonoBehaviour
{
    public static OperaGameController Instance { get; private set; }
    public bool Ready { get; private set; }
    public bool Thinking { get; private set; }
    public string Status { get; private set; } = "Starting Opera…";
    public bool CanHumanMove => Ready && !Thinking && pendingPromotion == null && GameManager.Instance.IsMyTurn();
    public GameState State => GameManager.Instance.GameState;

    private UciEngineClient engine;
    private CancellationTokenSource session;
    private int thinkMilliseconds = 1000;
    private Move pendingPromotion;
    private TextMeshProUGUI statusText;
    private TextMeshProUGUI lastMoveText;
    private TextMeshProUGUI thinkingTimeText;
    private GameObject promotionPanel;

    private void Awake()
    {
        Instance = this;
        if (GameManager.Instance == null) new GameObject("Game Manager").AddComponent<GameManager>();
        GameManager.Instance.StartEngineGame(PieceColor.White);
    }

    private async void Start()
    {
        BuildControls();
        if (Camera.main != null)
        {
            Camera.main.rect = new Rect(0, 0, 0.72f, 1);
            Camera.main.orthographicSize = 4.7f;
        }
        // Allow the board's Start to finish before accepting engine or human moves.
        await Task.Yield();
        if (this != null) await StartEngineAsync();
    }

    public async Task NewGameAsync(PieceColor color)
    {
        StopSession();
        BoardInputManager.Instance?.UnselectSquare();
        GameManager.Instance.StartEngineGame(color);
        BoardRenderer.Instance.RenderBoardSquares(State.Board);
        if (color == PieceColor.Black) BoardRenderer.Instance.FlipPerspective(State.Board);
        pendingPromotion = null;
        promotionPanel?.SetActive(false);
        if (lastMoveText != null) lastMoveText.text = "Click a piece to see its legal moves.";
        await StartEngineAsync();
    }

    private async Task StartEngineAsync()
    {
        Ready = false;
        SetStatus("Starting Opera…");
        session = new CancellationTokenSource();
        var cancellation = session.Token;
        var client = new UciEngineClient();
        engine = client;
        try
        {
            await client.StartAsync(OperaEngineLocator.FindExecutable(), cancellation);
            if (cancellation.IsCancellationRequested || engine != client) return;
            Ready = true;
            RefreshStatus();
            await MoveForEngineAsync();
        }
        catch (OperationCanceledException) { }
        catch (Exception error)
        {
            if (!cancellation.IsCancellationRequested && engine == client) ReportError(error);
        }
    }

    public bool TryHumanMove(Move move)
    {
        if (!CanHumanMove || move.From.Piece.GetColor() != GameManager.Instance.MyColor ||
            !LegalMovesHandler.IsMoveLegal(State, move.To, move.From)) return false;
        if (move.From.Piece.GetType() == PieceType.Pawn && (move.To.Coord.rank == 0 || move.To.Coord.rank == 7))
        {
            pendingPromotion = move;
            promotionPanel.SetActive(true);
            SetStatus("Choose a promotion piece.");
            return true;
        }
        return ApplyHumanMove(move);
    }

    private bool ApplyHumanMove(Move move)
    {
        if (!State.TryApplyMove(move)) return false;
        RenderMove(move);
        _ = MoveForEngineAsync();
        return true;
    }

    private void Promote(PieceType type)
    {
        if (pendingPromotion == null) return;
        Move move = pendingPromotion;
        pendingPromotion = null;
        promotionPanel.SetActive(false);
        move.Promotion = type;
        ApplyHumanMove(move);
    }

    private async Task MoveForEngineAsync()
    {
        if (!Ready || State.IsGameOver || GameManager.Instance.IsMyTurn() || Thinking) return;
        var client = engine;
        var cancellation = session.Token;
        var position = State;
        Thinking = true;
        SetStatus("Opera is thinking…");
        try
        {
            string[] history = position.MoveTracker.moves.Select(move => move.ToUci()).ToArray();
            string response = await client.GetMoveAsync(history, thinkMilliseconds, cancellation);
            if (cancellation.IsCancellationRequested || engine != client || position != State) return;
            if (response == null) throw new InvalidOperationException("Opera returned no move in an unfinished game.");
            Move move = Move.FromUci(position, response);
            if (!position.TryApplyMove(move)) throw new InvalidOperationException("Opera's move disagrees with the board: " + response);
            Thinking = false;
            RenderMove(move);
        }
        catch (OperationCanceledException) { }
        catch (Exception error)
        {
            if (!cancellation.IsCancellationRequested && engine == client) ReportError(error);
        }
    }

    private void RenderMove(Move move)
    {
        foreach (Square square in State.Board.squares) BoardRenderer.Instance.RenderPieceOnBoard(square);
        if (lastMoveText != null) lastMoveText.text = "Last move: " + move.From.Coord + " → " + move.To.Coord +
            (move.Promotion == PieceType.None ? "" : " = " + move.Promotion);
        GameManager.Instance.NotifyStateUpdated();
        RefreshStatus();
    }

    private void RefreshStatus()
    {
        if (State.IsGameOver)
        {
            if (State.Result == GameStateUtils.GameResult.Checkmate)
                SetStatus(State.ColorToMove == GameManager.Instance.MyColor ? "Checkmate. Opera wins." : "Checkmate. You win!");
            else SetStatus("Draw — " + DrawReason(State.Result));
            engine?.Dispose();
            Ready = false;
            return;
        }
        string turn = GameManager.Instance.IsMyTurn() ? "Your turn · " + GameManager.Instance.MyColor : "Opera's turn";
        if (CheckUtils.IsKingInCheck(State.Board, State.ColorToMove)) turn += "\nCheck";
        SetStatus(turn);
    }

    private static string DrawReason(GameStateUtils.GameResult result) => result switch {
        GameStateUtils.GameResult.Stalemate => "stalemate",
        GameStateUtils.GameResult.InsufficientMaterial => "insufficient material",
        GameStateUtils.GameResult.FiftyMoveRule => "fifty-move rule",
        GameStateUtils.GameResult.ThreefoldRepetition => "threefold repetition", _ => "game over"
    };

    public void Resign()
    {
        if (State.IsGameOver) return;
        StopSession();
        State.IsGameOver = true;
        pendingPromotion = null;
        promotionPanel?.SetActive(false);
        SetStatus("You resigned. Opera wins.");
    }

    private void ReportError(Exception error)
    {
        Debug.LogError("[Opera] " + error);
        StopSession();
        SetStatus("Opera could not continue.\nStart a new game to retry.\n" + error.Message);
    }

    private void SetStatus(string value)
    {
        Status = value;
        if (statusText != null) statusText.text = value;
    }

    private void StopSession()
    {
        Ready = false;
        Thinking = false;
        session?.Cancel();
        engine?.Dispose();
        engine = null;
        session?.Dispose();
        session = null;
    }

    private void ReturnToMenu()
    {
        StopSession();
        GameManager.Instance.ResetToDefault();
        SceneManager.LoadScene(SceneLoader.MainMenu);
    }

    private void OnDestroy() { StopSession(); if (Instance == this) Instance = null; }
    private void OnApplicationQuit() => StopSession();

    private void BuildControls()
    {
        var root = new GameObject("Opera Controls", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.transform.SetParent(transform, false);
        root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        root.GetComponent<Canvas>().sortingOrder = 20;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 800);
        scaler.matchWidthOrHeight = 1;
        var panel = new GameObject("Game Controls", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(root.transform, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.72f, 0); rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.065f, 0.08f, 0.10f);
        Label(panel.transform, "Opera", 32, 54, 36);
        Label(panel.transform, "PLAY THE CURRENT ENGINE", 90, 30, 14).color = new Color(0.65f, 0.75f, 0.63f);
        statusText = Label(panel.transform, Status, 155, 125, 23);
        lastMoveText = Label(panel.transform, "Click a piece to see its legal moves.", 292, 65, 16);
        Button(panel.transform, "New game · White", 385, async () => await NewGameAsync(PieceColor.White));
        Button(panel.transform, "New game · Black", 441, async () => await NewGameAsync(PieceColor.Black));
        thinkingTimeText = Label(panel.transform, "Thinking time: 1 second", 515, 30, 16);
        var times = Row(panel.transform, 552, 40);
        int[] durations = { 250, 1000, 3000 };
        for (int i = 0; i < durations.Length; i++)
        {
            int duration = durations[i];
            var button = Button(times, (duration / 1000f).ToString("0.##") + "s", 0, () => {
                thinkMilliseconds = duration;
                thinkingTimeText.text = "Thinking time: " + (duration / 1000f).ToString("0.##") + " seconds";
            });
            Cell(button.GetComponent<RectTransform>(), i, 3);
        }
        Button(panel.transform, "Resign", 638, Resign);
        Button(panel.transform, "Main menu", 704, ReturnToMenu);

        promotionPanel = new GameObject("Promotion", typeof(RectTransform), typeof(Image));
        promotionPanel.transform.SetParent(root.transform, false);
        var popup = promotionPanel.GetComponent<RectTransform>();
        popup.anchorMin = popup.anchorMax = new Vector2(0.36f, 0.5f);
        popup.sizeDelta = new Vector2(560, 170);
        promotionPanel.GetComponent<Image>().color = new Color(0.065f, 0.08f, 0.10f, 0.98f);
        Label(popup, "Promote your pawn", 22, 40, 26);
        var choices = Row(popup, 90, 50);
        PieceType[] pieces = { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight };
        for (int i = 0; i < pieces.Length; i++)
        {
            PieceType piece = pieces[i];
            var button = Button(choices, piece.ToString(), 0, () => Promote(piece));
            Cell(button.GetComponent<RectTransform>(), i, 4);
        }
        promotionPanel.SetActive(false);
    }

    private static RectTransform Row(Transform parent, float y, float height)
    {
        var rect = new GameObject("Row", typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false); Place(rect, y, height); return rect;
    }
    private static void Place(RectTransform rect, float y, float height)
    {
        rect.anchorMin = new Vector2(0, 1); rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 1); rect.anchoredPosition = new Vector2(0, -y);
        rect.sizeDelta = new Vector2(-48, height);
    }
    private static void Cell(RectTransform rect, int i, int count)
    {
        rect.anchorMin = new Vector2((float)i / count, 0); rect.anchorMax = new Vector2((float)(i + 1) / count, 1);
        rect.offsetMin = new Vector2(3, 0); rect.offsetMax = new Vector2(-3, 0);
    }
    private static TextMeshProUGUI Label(Transform parent, string text, float y, float height, float size)
    {
        var label = new GameObject(text, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        label.transform.SetParent(parent, false); Place(label.rectTransform, y, height);
        label.text = text; label.fontSize = size; label.color = new Color(0.92f, 0.94f, 0.9f);
        label.raycastTarget = false; label.textWrappingMode = TextWrappingModes.Normal;
        return label;
    }
    private static Button Button(Transform parent, string text, float y, UnityAction action)
    {
        var root = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button));
        root.transform.SetParent(parent, false); Place(root.GetComponent<RectTransform>(), y, 46);
        root.GetComponent<Image>().color = new Color(0.22f, 0.31f, 0.23f);
        var button = root.GetComponent<Button>(); button.onClick.AddListener(action);
        var label = Label(root.transform, text, 0, 46, 18);
        label.alignment = TextAlignmentOptions.Center;
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = new Vector2(6, 0);
        label.rectTransform.offsetMax = new Vector2(-6, 0);
        return button;
    }
}
