using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Opera
{
    /// <summary>Shared materials and typography for the chess app.</summary>
    public static class OperaTheme
    {
        public static readonly Color Ink = Hex("E8DCC5"), Muted = Hex("B3A38D");
        public static readonly Color Background = Hex("201C18"), Surface = Hex("302923");
        public static readonly Color Recess = Hex("241F1B"), ButtonFace = Hex("493D30");
        public static readonly Color Brass = Hex("B39A65"), Sage = Hex("92AD99");
        public static readonly Color LastMove = Hex("CB994E"), LightSquare = Hex("CDBB98");
        public static readonly Color DarkSquare = Hex("75604A");
        private static Sprite pixel, disc, ring, outline, grain, wood;
        private static TMP_FontAsset serif;

        public static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out var c); return c; }
        public static TMP_FontAsset Serif
        {
            get
            {
                if (serif != null) return serif;
                var font = Resources.Load<Font>("Opera/EBGaramond");
                if (font == null) return TMP_Settings.defaultFontAsset;
                serif = TMP_FontAsset.CreateFontAsset(font);
                serif.name = "Opera Garamond";
                return serif;
            }
        }
        public static Sprite Pixel => pixel != null ? pixel : pixel = Make("Solid", 4, (x, y) => Color.white);
        public static Sprite Disc => disc != null ? disc : disc = Make("Soft disc", 96, (x, y) =>
        {
            float radius = new Vector2(x - .5f, y - .5f).magnitude;
            return new Color(1, 1, 1, 1 - Edge(.46f, .5f, radius));
        });
        public static Sprite Ring => ring != null ? ring : ring = Make("Capture ring", 128, (x, y) =>
        {
            float radius = new Vector2(x - .5f, y - .5f).magnitude;
            float alpha = Edge(.39f, .415f, radius) * (1 - Edge(.46f, .485f, radius));
            return new Color(1, 1, 1, alpha);
        });
        public static Sprite Outline => outline != null ? outline : outline = Make("Inlaid outline", 128, (x, y) =>
        {
            float edge = Mathf.Min(x, y, 1 - x, 1 - y);
            return new Color(1, 1, 1, Edge(.02f, .035f, edge) * (1 - Edge(.055f, .07f, edge)));
        });
        public static Sprite Grain => grain != null ? grain : grain = Material("Linen", false);
        public static Sprite Wood => wood != null ? wood : wood = Material("Walnut", true);
        private static float Edge(float from, float to, float value) => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(from, to, value));
        private static Sprite Material(string name, bool timber)
        {
            var random = new System.Random(timber ? 481 : 1729);
            return Make(name, 256, (x, y) =>
            {
                float noise = (float)random.NextDouble();
                float wave = timber ? Mathf.Sin(x * 65 + Mathf.Sin(y * 11) * 2 + Mathf.Sin(y * 29) * .5f) : Mathf.Sin(x * 800) * Mathf.Sin(y * 800);
                float value = .94f + wave * (timber ? .018f : .01f) + noise * .025f;
                return new Color(value, value, value);
            });
        }
        private static Sprite Make(string name, int size, System.Func<float, float, Color> color)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = name, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Repeat };
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++) pixels[y * size + x] = color((x + .5f) / size, (y + .5f) / size);
            texture.SetPixels(pixels); texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size);
        }
        public static Canvas Canvas(Transform parent, string name, int order = 20)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            obj.transform.SetParent(parent, false);
            var canvas = obj.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = order;
            var scaler = obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 800); scaler.matchWidthOrHeight = .5f;
            return canvas;
        }
        public static RectTransform Box(Transform parent, string name, Color color, bool textured = false, bool blocks = false)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image)); obj.transform.SetParent(parent, false);
            var img = obj.GetComponent<Image>(); img.sprite = textured ? Grain : Pixel; img.color = color; img.raycastTarget = blocks;
            return obj.GetComponent<RectTransform>();
        }
        public static void Stretch(RectTransform rect, float inset = 0)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.one * inset; rect.offsetMax = -Vector2.one * inset;
        }
        public static void Place(RectTransform rect, float y, float height, float inset = 40)
        {
            rect.anchorMin = new Vector2(0, 1); rect.anchorMax = Vector2.one; rect.pivot = new Vector2(.5f, 1);
            rect.anchoredPosition = new Vector2(0, -y); rect.sizeDelta = new Vector2(-inset, height);
        }
        public static TextMeshProUGUI Label(Transform parent, string text, float y, float height, float size, float inset = 40, bool heading = false)
        {
            var label = new GameObject(text, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            label.transform.SetParent(parent, false); Place(label.rectTransform, y, height, inset);
            label.text = text; label.fontSize = size; label.color = Ink; label.raycastTarget = false; label.richText = false;
            label.textWrappingMode = TextWrappingModes.Normal; label.overflowMode = TextOverflowModes.Ellipsis;
            if (heading) { label.font = Serif; label.overflowMode = TextOverflowModes.Overflow; }
            return label;
        }
        public static Button Button(Transform parent, string text, UnityAction action, bool primary = false)
        {
            var rect = Box(parent, text, primary ? Hex("655239") : ButtonFace, true, true);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = rect.GetComponent<Image>();
            StyleButton(button);
            button.onClick.AddListener(() => OperaAudio.Play(OperaSound.Button));
            if (action != null) button.onClick.AddListener(action);
            var label = Label(rect, text, 0, 30, 14, 16);
            label.alignment = TextAlignmentOptions.Center; Stretch(label.rectTransform);
            label.margin = new Vector4(10, 0, 10, 0);
            return button;
        }
        public static void StyleButton(Button button)
        {
            var colors = button.colors; colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.22f, 1.18f, 1.10f); colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color(.74f, .72f, .66f); colors.disabledColor = new Color(.53f, .51f, .48f);
            colors.fadeDuration = .12f; button.colors = colors;
            var edge = button.gameObject.AddComponent<UnityEngine.UI.Outline>(); edge.effectColor = new Color(Brass.r, Brass.g, Brass.b, .22f);
            edge.effectDistance = new Vector2(1, -1); edge.useGraphicAlpha = false;
        }
        public static RectTransform Avatar(Transform parent, bool engine, float size = 48)
        {
            var rect = Box(parent, engine ? "Opera avatar" : "Player avatar", engine ? Brass : Sage);
            rect.sizeDelta = Vector2.one * size; rect.GetComponent<Image>().sprite = Disc;
            var inset = Box(rect, "Medallion", Surface); Stretch(inset, 2); inset.GetComponent<Image>().sprite = Disc;
            if (engine)
            {
                var monogram = Label(rect, "O", 0, size, size * .78f, 0, true);
                Stretch(monogram.rectTransform); monogram.alignment = TextAlignmentOptions.Center; monogram.color = Brass;
            }
            else
            {
                var head = Box(rect, "Portrait", Ink); head.GetComponent<Image>().sprite = Disc;
                head.anchorMin = new Vector2(.34f, .49f); head.anchorMax = new Vector2(.66f, .81f); head.offsetMin = head.offsetMax = Vector2.zero;
                var shoulders = Box(rect, "Shoulders", Ink); shoulders.GetComponent<Image>().sprite = Disc;
                shoulders.anchorMin = new Vector2(.23f, .20f); shoulders.anchorMax = new Vector2(.77f, .49f); shoulders.offsetMin = shoulders.offsetMax = Vector2.zero;
            }
            return rect;
        }
        public static SpriteRenderer World(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size, Color color, int order)
        {
            var obj = new GameObject(name, typeof(SpriteRenderer)); obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position; obj.transform.localScale = new Vector3(size.x, size.y, 1);
            var renderer = obj.GetComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.color = color; renderer.sortingOrder = order;
            return renderer;
        }
    }
}
