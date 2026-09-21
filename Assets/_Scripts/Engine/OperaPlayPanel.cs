using System;
using System.Linq;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Opera
{
    /// <summary>Desktop play and review controls, built without changing the online-game scene.</summary>
    public sealed class OperaPlayPanel
    {
        private readonly OperaGameController game;
        private readonly CanvasScaler scaler;
        private readonly RectTransform historyContent;
        private readonly ScrollRect historyScroll;
        private readonly TextMeshProUGUI status, location, notice, eval, candidates, analysisCaption, time, toggleLabel;
        private readonly Button first, back, next, live, resume;
        private readonly RectTransform whiteMeter;
        private readonly GameObject analysisBody, promotion;
        private int lastWidth, lastHeight;
        private static readonly Color Ink = new Color(0.92f, 0.94f, 0.90f);
        private static readonly Color Muted = new Color(0.63f, 0.73f, 0.69f);
        private static readonly Color Surface = new Color(0.065f, 0.08f, 0.10f);
        private static readonly Color ButtonColor = new Color(0.18f, 0.26f, 0.23f);
        public OperaPlayPanel(Transform parent, OperaGameController game)
        {
            this.game = game;
            var root = new GameObject("Opera Controls", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(parent, false);
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            root.GetComponent<Canvas>().sortingOrder = 20;
            scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            var panel = Box(root.transform, "Play and review", Surface);
            panel.anchorMin = new Vector2(0.64f, 0); panel.anchorMax = Vector2.one; panel.offsetMin = panel.offsetMax = Vector2.zero;
            Label(panel, "OPERA", 18, 36, 28);
            status = Label(panel, "Starting Opera…", 88, 49, 18);
            location = Label(panel, "Live position", 143, 23, 13); location.color = Muted;
            var viewport = Box(panel, "Move list", new Color(0.045f, 0.057f, 0.071f)); Place(viewport, 171, 168);
            viewport.gameObject.AddComponent<RectMask2D>();
            historyScroll = viewport.gameObject.AddComponent<ScrollRect>();
            historyScroll.horizontal = false; historyScroll.movementType = ScrollRect.MovementType.Clamped;
            historyScroll.scrollSensitivity = 26; historyScroll.viewport = viewport;
            historyContent = new GameObject("All moves", typeof(RectTransform)).GetComponent<RectTransform>();
            historyContent.SetParent(viewport, false); historyContent.anchorMin = new Vector2(0, 1); historyContent.anchorMax = Vector2.one;
            historyContent.pivot = new Vector2(0.5f, 1); historyContent.anchoredPosition = Vector2.zero;
            historyScroll.content = historyContent;
            var track = Box(viewport, "History scrollbar", new Color(0.10f, 0.13f, 0.14f));
            track.anchorMin = new Vector2(1, 0); track.anchorMax = Vector2.one;
            track.offsetMin = new Vector2(-7, 2); track.offsetMax = new Vector2(-1, -2);
            var thumb = Box(track, "Scroll handle", Muted);
            thumb.anchorMin = Vector2.zero; thumb.anchorMax = Vector2.one; thumb.offsetMin = thumb.offsetMax = Vector2.zero;
            var scrollbar = track.gameObject.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop; scrollbar.handleRect = thumb;
            scrollbar.targetGraphic = thumb.GetComponent<Image>();
            historyScroll.verticalScrollbar = scrollbar;
            historyScroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            var nav = Row(panel, 345, 31);
            first = Button(nav, "Start", () => game.ViewPly(0)); Cell(first, 0, 4);
            back = Button(nav, "< Back", () => game.ViewPly(game.ViewedPly - 1)); Cell(back, 1, 4);
            next = Button(nav, "Next >", () => game.ViewPly(game.ViewedPly + 1)); Cell(next, 2, 4);
            live = Button(nav, "Live", () => game.ViewPly(game.Record.Plies.Count)); Cell(live, 3, 4);
            resume = Button(panel, "Resume from here · saves the original game", async () => await game.ResumeHereAsync()); Place(resume.GetComponent<RectTransform>(), 383, 30);
            resume.GetComponentInChildren<TextMeshProUGUI>().fontSize = 13;
            var exports = Row(panel, 420, 32);
            Cell(Button(exports, "Export game · PGN", game.ExportGame), 0, 2);
            Cell(Button(exports, "Show exports", game.ShowExports), 1, 2);
            notice = Label(panel, "Arrow keys review moves. Export a PGN to share this game.", 460, 37, 12); notice.color = Muted;
            var toggle = Button(panel, "Analysis: shown · click to hide", game.ToggleAnalysis); Place(toggle.GetComponent<RectTransform>(), 504, 29);
            toggleLabel = toggle.GetComponentInChildren<TextMeshProUGUI>(); toggleLabel.fontSize = 14;
            analysisBody = new GameObject("Evaluation and candidate lines", typeof(RectTransform));
            analysisBody.transform.SetParent(panel, false);
            var analysisRect = analysisBody.GetComponent<RectTransform>(); Place(analysisRect, 539, 147);
            var meter = Box(analysisRect, "Black evaluation share", new Color(0.14f, 0.16f, 0.18f));
            Place(meter, 0, 10, 0);
            whiteMeter = Box(meter, "White evaluation share", Ink);
            whiteMeter.anchorMin = Vector2.zero; whiteMeter.anchorMax = new Vector2(0.5f, 1); whiteMeter.offsetMin = whiteMeter.offsetMax = Vector2.zero;
            eval = Label(analysisRect, "Evaluating…", 15, 22, 18, 0);
            candidates = Label(analysisRect, "", 40, 75, 14, 0);
            candidates.textWrappingMode = TextWrappingModes.NoWrap; candidates.overflowMode = TextOverflowModes.Ellipsis;
            analysisCaption = Label(analysisRect, "Opera's estimate · + White / − Black", 119, 21, 11, 0); analysisCaption.color = Muted;
            var newGame = Row(panel, 693, 30);
            Cell(Button(newGame, "New · White", async () => await game.NewGameAsync(PieceColor.White)), 0, 2);
            Cell(Button(newGame, "New · Black", async () => await game.NewGameAsync(PieceColor.Black)), 1, 2);
            time = Label(panel, "Think: 1s", 734, 26, 13);
            time.rectTransform.anchorMax = new Vector2(0.30f, 1); time.rectTransform.sizeDelta = new Vector2(-24, 26);
            var durations = Row(panel, 730, 28); durations.anchorMin = new Vector2(0.3f, 1); durations.sizeDelta = new Vector2(-36, 28);
            int[] values = { 250, 1000, 3000 };
            for (int i = 0; i < values.Length; ++i)
            {
                int value = values[i]; Cell(Button(durations, (value / 1000f).ToString("0.##") + "s", () => game.SetThinkingTime(value)), i, 3);
            }
            var bottom = Row(panel, 766, 26);
            Cell(Button(bottom, "Resign", game.Resign), 0, 2); Cell(Button(bottom, "Main menu", game.ReturnToMenu), 1, 2);
            promotion = new GameObject("Promotion", typeof(RectTransform), typeof(Image)); promotion.transform.SetParent(root.transform, false);
            var popup = promotion.GetComponent<RectTransform>(); popup.anchorMin = popup.anchorMax = new Vector2(0.32f, 0.5f); popup.sizeDelta = new Vector2(520, 150);
            promotion.GetComponent<Image>().color = Surface;
            Label(popup, "Promote your pawn", 20, 35, 25);
            var choices = Row(popup, 85, 45);
            PieceType[] pieces = { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight };
            for (int i = 0; i < pieces.Length; ++i) { PieceType piece = pieces[i]; Cell(Button(choices, piece.ToString(), () => game.Promote(piece)), i, 4); }
            promotion.SetActive(false); Resize();
        }
        public void Resize()
        {
            if (Screen.width == lastWidth && Screen.height == lastHeight) return;
            lastWidth = Screen.width; lastHeight = Screen.height;
            scaler.scaleFactor = Mathf.Min(Screen.width / 1280f, Screen.height / 800f);
            if (Camera.main != null)
            {
                Camera.main.rect = new Rect(0, 0, 0.64f, 1);
                Camera.main.orthographicSize = Mathf.Max(4.65f, 4.65f / Camera.main.aspect);
            }
        }
        public void Status(string text) => status.text = text;
        public void Notice(string text) => notice.text = text;
        public void ThinkingTime(int ms) => time.text = "Think: " + (ms / 1000f).ToString("0.##") + "s";
        public void ShowPromotion(bool value) => promotion.SetActive(value);
        public void History(OperaGameRecord record, int selected)
        {
            foreach (Transform child in historyContent) { child.gameObject.SetActive(false); UnityEngine.Object.Destroy(child.gameObject); }
            int rows = Mathf.Max(1, (record.Plies.Count + 1) / 2);
            historyContent.sizeDelta = new Vector2(0, rows * 28 + 8);
            if (record.Plies.Count == 0) Label(historyContent, "Your full game will appear here.", 12, 30, 14, 20).color = Muted;
            for (int i = 0; i < record.Plies.Count; ++i)
            {
                int ply = i + 1;
                if (i % 2 == 0)
                {
                    var number = Label(historyContent, (i / 2 + 1).ToString() + ".", i / 2 * 28 + 6, 26, 14, 0);
                    number.rectTransform.anchorMin = new Vector2(0.03f, 1); number.rectTransform.anchorMax = new Vector2(0.12f, 1); number.color = Muted;
                }
                var button = Button(historyContent, record.Plies[i].San, () => game.ViewPly(ply));
                var rect = button.GetComponent<RectTransform>(); Place(rect, i / 2 * 28 + 4, 26, 0);
                rect.anchorMin = new Vector2(i % 2 == 0 ? 0.14f : 0.56f, 1); rect.anchorMax = new Vector2(i % 2 == 0 ? 0.54f : 0.96f, 1);
                button.GetComponent<Image>().color = selected == ply ? new Color(0.31f, 0.43f, 0.32f) : new Color(0.08f, 0.105f, 0.12f);
                button.GetComponentInChildren<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
            }
            bool reviewing = selected != record.Plies.Count;
            location.text = reviewing ? "REVIEW · ply " + selected + " of " + record.Plies.Count : "LIVE · " + record.Plies.Count + " plies";
            first.interactable = back.interactable = selected > 0;
            next.interactable = live.interactable = reviewing;
            resume.interactable = reviewing;
            // Keep the selected move visible while stepping through a long game.
            Canvas.ForceUpdateCanvases();
            float max = Mathf.Max(0, historyContent.rect.height - historyScroll.viewport.rect.height);
            float desired = Mathf.Clamp((Mathf.Max(0, selected - 1) / 2) * 28 - 56, 0, max);
            historyContent.anchoredPosition = new Vector2(0, reviewing ? desired : max);
        }
        public void AnalysisVisible(bool visible)
        {
            analysisBody.SetActive(visible); toggleLabel.text = visible ? "Analysis: shown · click to hide" : "Analysis: hidden · click to show";
        }
        public void AnalysisPending()
        {
            eval.text = "Evaluating this position…"; candidates.text = "";
            whiteMeter.anchorMax = new Vector2(0.5f, 1); analysisCaption.text = "Scores favour White when positive. Candidates will appear below.";
        }
        public void AnalysisUnavailable(string error)
        {
            eval.text = "Analysis unavailable"; candidates.text = "Hide and show analysis to retry."; analysisCaption.text = error;
        }
        public void Analysis(UciSearchInfo[] lines, GameState position, string caption, UciSearchInfo primary = null)
        {
            primary = primary ?? lines.FirstOrDefault();
            if (primary == null) return;
            bool white = position.ColorToMove == PieceColor.White;
            eval.text = primary.WhiteLabel(white) + "   ·   depth " + primary.Depth;
            whiteMeter.anchorMax = new Vector2((float)(0.5 + 0.5 * Math.Tanh(primary.WhiteScore(white) / 4)), 1);
            candidates.text = string.Join("\n", lines.OrderByDescending(line => line.WhiteScore(white) * (white ? 1 : -1)).Take(3).Select((line, i) =>
                (i + 1) + ".  " + line.WhiteLabel(white) + "  d" + line.Depth + "   " + ChessNotation.Variation(position, line.Pv)));
            analysisCaption.text = caption;
        }
        public void TerminalAnalysis(GameState position)
        {
            bool mate = position.Result == GameStateUtils.GameResult.Checkmate;
            eval.text = mate ? (position.ColorToMove == PieceColor.White ? "Black wins · checkmate" : "White wins · checkmate") : "0.00 · Draw";
            candidates.text = "No further moves."; analysisCaption.text = "Final position";
            whiteMeter.anchorMax = new Vector2(mate ? (position.ColorToMove == PieceColor.White ? 0 : 1) : 0.5f, 1);
        }
        private static RectTransform Box(Transform parent, string name, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image)); obj.transform.SetParent(parent, false); obj.GetComponent<Image>().color = color;
            return obj.GetComponent<RectTransform>();
        }
        private static RectTransform Row(Transform parent, float y, float height)
        {
            var rect = new GameObject("Row", typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false); Place(rect, y, height); return rect;
        }
        private static void Place(RectTransform rect, float y, float height, float inset = 40)
        {
            rect.anchorMin = new Vector2(0, 1); rect.anchorMax = Vector2.one; rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = new Vector2(0, -y); rect.sizeDelta = new Vector2(-inset, height);
        }
        private static void Cell(Button button, int i, int count)
        {
            var rect = button.GetComponent<RectTransform>(); rect.anchorMin = new Vector2((float)i / count, 0); rect.anchorMax = new Vector2((float)(i + 1) / count, 1);
            rect.offsetMin = new Vector2(3, 0); rect.offsetMax = new Vector2(-3, 0);
        }
        private static TextMeshProUGUI Label(Transform parent, string text, float y, float height, float size, float inset = 40)
        {
            var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            label.transform.SetParent(parent, false); Place(label.rectTransform, y, height, inset);
            label.text = text; label.fontSize = size; label.color = Ink; label.raycastTarget = false; label.richText = false;
            label.textWrappingMode = TextWrappingModes.Normal; label.overflowMode = TextOverflowModes.Ellipsis; return label;
        }
        private static Button Button(Transform parent, string text, UnityAction action)
        {
            var rect = Box(parent, text, ButtonColor); var button = rect.gameObject.AddComponent<Button>(); button.onClick.AddListener(action);
            var colors = button.colors; colors.highlightedColor = new Color(1.18f, 1.18f, 1.18f); colors.disabledColor = new Color(0.5f, 0.5f, 0.5f); button.colors = colors;
            var label = Label(rect, text, 0, 30, 14); label.alignment = TextAlignmentOptions.Center;
            label.rectTransform.anchorMin = Vector2.zero; label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(8, 0); label.rectTransform.offsetMax = new Vector2(-8, 0);
            return button;
        }
    }
}
