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
        private readonly OperaPlayerCard opponentCard, humanCard;
        private readonly TextMeshProUGUI soundLabel;
        private readonly Button[] thinkingButtons = new Button[3];
        private static readonly Color Ink = OperaTheme.Ink;
        private static readonly Color Muted = OperaTheme.Muted;
        private static readonly Color Surface = OperaTheme.Surface;
        private static readonly Color ButtonColor = OperaTheme.ButtonFace;
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
            var title = Label(panel, "Chess", 13, 49, 39); title.font = OperaTheme.Serif; title.overflowMode = TextOverflowModes.Overflow;
            var rule = Box(panel, "Brass divider", new Color(.70f, .60f, .40f, .30f)); Place(rule, 80, 1);
            var sound = Button(panel, OperaAudio.Muted ? "Sound off" : "Sound on", () => {
                OperaAudio.Muted = !OperaAudio.Muted; soundLabel.text = OperaAudio.Muted ? "Sound off" : "Sound on";
            });
            var soundRect = sound.GetComponent<RectTransform>(); soundRect.anchorMin = soundRect.anchorMax = Vector2.one;
            soundRect.sizeDelta = new Vector2(91, 29); soundRect.anchoredPosition = new Vector2(-67, -34);
            soundLabel = sound.GetComponentInChildren<TextMeshProUGUI>(); soundLabel.fontSize = 12;
            opponentCard = new OperaPlayerCard(root.transform, true);
            humanCard = new OperaPlayerCard(root.transform, false);
            status = Label(panel, "Starting Opera…", 88, 49, 18);
            location = Label(panel, "Live position", 143, 23, 13); location.color = Muted;
            var viewport = Box(panel, "Move list", OperaTheme.Recess); Place(viewport, 171, 168);
            viewport.gameObject.AddComponent<RectMask2D>();
            historyScroll = viewport.gameObject.AddComponent<ScrollRect>();
            historyScroll.horizontal = false; historyScroll.movementType = ScrollRect.MovementType.Clamped;
            historyScroll.scrollSensitivity = 26; historyScroll.viewport = viewport;
            historyContent = new GameObject("All moves", typeof(RectTransform)).GetComponent<RectTransform>();
            historyContent.SetParent(viewport, false); historyContent.anchorMin = new Vector2(0, 1); historyContent.anchorMax = Vector2.one;
            historyContent.pivot = new Vector2(0.5f, 1); historyContent.anchoredPosition = Vector2.zero;
            historyScroll.content = historyContent;
            var track = Box(viewport, "History scrollbar", OperaTheme.Surface);
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
            notice = Label(panel, "Drag or click to move. Select your king, then its rook to castle.", 460, 37, 12); notice.color = Muted;
            var toggle = Button(panel, "Analysis: shown · click to hide", game.ToggleAnalysis); Place(toggle.GetComponent<RectTransform>(), 504, 29);
            toggleLabel = toggle.GetComponentInChildren<TextMeshProUGUI>(); toggleLabel.fontSize = 14;
            analysisBody = new GameObject("Evaluation and candidate lines", typeof(RectTransform));
            analysisBody.transform.SetParent(panel, false);
            var analysisRect = analysisBody.GetComponent<RectTransform>(); Place(analysisRect, 539, 147);
            var meter = Box(analysisRect, "Black evaluation share", OperaTheme.Recess);
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
                int value = values[i]; thinkingButtons[i] = Button(durations, (value / 1000f).ToString("0.##") + "s", () => game.SetThinkingTime(value)); Cell(thinkingButtons[i], i, 3);
            }
            var bottom = Row(panel, 766, 26);
            Cell(Button(bottom, "Resign", game.Resign), 0, 2); Cell(Button(bottom, "Main menu", game.ReturnToMenu), 1, 2);
            promotion = new GameObject("Promotion", typeof(RectTransform), typeof(Image)); promotion.transform.SetParent(root.transform, false);
            OperaTheme.Stretch(promotion.GetComponent<RectTransform>()); var popup = Box(promotion.transform, "Choose promotion", Surface); popup.anchorMin = popup.anchorMax = new Vector2(.32f, .5f); popup.sizeDelta = new Vector2(520, 170);
            promotion.GetComponent<Image>().color = new Color(.06f, .04f, .025f, .72f);
            Label(popup, "Promote your pawn", 20, 35, 25);
            var choices = Row(popup, 85, 45);
            PieceType[] pieces = { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight };
            for (int i = 0; i < pieces.Length; ++i) { PieceType piece = pieces[i]; Cell(Button(choices, piece.ToString(), () => game.Promote(piece)), i, 4); }
            promotion.SetActive(false); Resize(); ThinkingTime(1000);
        }
        public void Resize()
        {
            if (Screen.width == lastWidth && Screen.height == lastHeight) return;
            lastWidth = Screen.width; lastHeight = Screen.height;
            scaler.scaleFactor = Mathf.Min(Screen.width / 1280f, Screen.height / 800f);
            if (Camera.main != null)
            {
                Camera.main.rect = new Rect(0, 0, 0.64f, 1);
                Camera.main.orthographicSize = Mathf.Max(5.8f, 4.85f / Camera.main.aspect);
                float cardWidth = 8.94f * Screen.height / (2 * Camera.main.orthographicSize * scaler.scaleFactor);
                opponentCard.Rect.anchorMin = opponentCard.Rect.anchorMax = new Vector2(.32f, 1);
                humanCard.Rect.anchorMin = humanCard.Rect.anchorMax = new Vector2(.32f, 0);
                opponentCard.Rect.anchoredPosition = new Vector2(0, -46);
                humanCard.Rect.anchoredPosition = new Vector2(0, 46);
                opponentCard.Rect.sizeDelta = humanCard.Rect.sizeDelta = new Vector2(cardWidth, 60);
            }
        }
        public void Status(string text) => status.text = text;
        public void Notice(string text) => notice.text = text;
        public void ThinkingTime(int ms)
        {
            time.text = "Think: " + (ms / 1000f).ToString("0.##") + "s";
            int[] values = { 250, 1000, 3000 };
            for (int i = 0; i < values.Length; i++)
                thinkingButtons[i].GetComponent<Image>().color = values[i] == ms ? OperaTheme.Hex("6A5940") : ButtonColor;
        }
        public void Players(GameState shown)
        {
            opponentCard.Update(game.Record.HumanColor, shown.ColorToMove, game.Thinking, game.Reviewing, shown.IsGameOver);
            humanCard.Update(game.Record.HumanColor, shown.ColorToMove, game.Thinking, game.Reviewing, shown.IsGameOver);
        }
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
                button.GetComponent<Image>().color = selected == ply ? OperaTheme.Hex("625339") : OperaTheme.Recess;
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
        private static RectTransform Box(Transform parent, string name, Color color) =>
            OperaTheme.Box(parent, name, color, true, true);
        private static RectTransform Row(Transform parent, float y, float height)
        {
            var rect = new GameObject("Row", typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); Place(rect, y, height); return rect;
        }
        private static void Place(RectTransform rect, float y, float height, float inset = 40) => OperaTheme.Place(rect, y, height, inset);
        private static void Cell(Button button, int i, int count)
        {
            var rect = button.GetComponent<RectTransform>(); rect.anchorMin = new Vector2((float)i / count, 0); rect.anchorMax = new Vector2((float)(i + 1) / count, 1);
            rect.offsetMin = new Vector2(3, 0); rect.offsetMax = new Vector2(-3, 0);
        }
        private static TextMeshProUGUI Label(Transform parent, string text, float y, float height, float size, float inset = 40) =>
            OperaTheme.Label(parent, text, y, height, size, inset);
        private static Button Button(Transform parent, string text, UnityAction action) => OperaTheme.Button(parent, text, action);
    }
}
