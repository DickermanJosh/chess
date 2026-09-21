using Core;
using TMPro;
using UnityEngine;

namespace Opera
{
    public sealed class OperaPlayerCard
    {
        public RectTransform Rect { get; }
        private readonly TextMeshProUGUI name, detail, turn;
        private readonly RectTransform indicator;
        private readonly bool engine;
        public OperaPlayerCard(Transform parent, bool engine)
        {
            this.engine = engine;
            Rect = OperaTheme.Box(parent, engine ? "Opponent" : "You", OperaTheme.Surface, true);
            Rect.pivot = new Vector2(.5f, .5f);
            var avatar = OperaTheme.Avatar(Rect, engine);
            avatar.anchorMin = avatar.anchorMax = new Vector2(0, .5f); avatar.anchoredPosition = new Vector2(28, 0);
            name = OperaTheme.Label(Rect, engine ? "Opera" : "You", 6, 29, 23, 0, true);
            name.enableAutoSizing = true; name.fontSizeMin = 16; name.fontSizeMax = 23; name.textWrappingMode = TextWrappingModes.NoWrap; name.overflowMode = TextOverflowModes.Ellipsis;
            name.rectTransform.offsetMin = new Vector2(65, name.rectTransform.offsetMin.y);
            name.rectTransform.offsetMax = new Vector2(-110, name.rectTransform.offsetMax.y);
            detail = OperaTheme.Label(Rect, "", 36, 19, 11, 0); detail.color = OperaTheme.Muted;
            detail.rectTransform.offsetMin = new Vector2(66, detail.rectTransform.offsetMin.y);
            detail.rectTransform.offsetMax = new Vector2(-100, detail.rectTransform.offsetMax.y);
            turn = OperaTheme.Label(Rect, "", 21, 22, 12, 0); turn.alignment = TextAlignmentOptions.MidlineRight;
            turn.rectTransform.offsetMin = new Vector2(230, turn.rectTransform.offsetMin.y);
            turn.rectTransform.offsetMax = new Vector2(-24, turn.rectTransform.offsetMax.y); turn.color = OperaTheme.Sage;
            indicator = OperaTheme.Box(Rect, "Turn indicator", OperaTheme.Sage);
            indicator.anchorMin = new Vector2(0, 0); indicator.anchorMax = new Vector2(0, 1);
            indicator.offsetMin = Vector2.zero; indicator.offsetMax = new Vector2(2, 0);
        }
        public void Update(PieceColor human, PieceColor toMove, bool thinking, bool reviewing, bool over)
        {
            PieceColor color = engine ? (human == PieceColor.White ? PieceColor.Black : PieceColor.White) : human;
            if (!engine) name.text = string.IsNullOrWhiteSpace(PlayerIdentity.PlayerName) ? "You" : PlayerIdentity.PlayerName;
            detail.text = color + (engine ? "  /  Morphy style" : "");
            bool active = !over && color == toMove;
            indicator.gameObject.SetActive(active);
            turn.text = !active ? "" : reviewing ? "To move" : engine && thinking ? "Thinking..." : engine ? "To move" : "Your turn";
        }
    }
}
