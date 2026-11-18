using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OlhosDeCacador
{
    public sealed class HunterUI
    {
        public int LastActionFrame { get; private set; } = -1;
        public static readonly Color Ink = new Color(.12f, .14f, .28f);
        public static readonly Color Paper = new Color(1f, .94f, .80f);
        public static readonly Color Muted = new Color(.31f, .28f, .34f);
        public static readonly Color Teal = new Color(.025f, .40f, .43f);
        public static readonly Color Plum = new Color(.24f, .16f, .39f);
        readonly Transform canvas;
        readonly Font font;
        readonly Font display;
        readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        RectTransform root;
        public HunterUI(Transform owner)
        {
            var go = new GameObject("Interface", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(owner, false);
            go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1440, 900); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            canvas = go.transform;
            font = Resources.Load<Font>("Hunter/UI/Itim-Regular") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            display = Resources.Load<Font>("Hunter/UI/Boogaloo-Regular") ?? font; Clear();
        }
        public void Clear()
        {
            if (root != null) { root.gameObject.SetActive(false); UnityEngine.Object.Destroy(root.gameObject); }
            root = new GameObject("Tela", typeof(RectTransform)).GetComponent<RectTransform>();
            root.SetParent(canvas, false); root.anchorMin = root.anchorMax = new Vector2(.5f, .5f);
            root.sizeDelta = new Vector2(1440, 900);
        }
        RectTransform Rect(string name, float x, float y, float w, float h, Transform parent = null)
        {
            var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(parent == null ? root : parent, false); rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1); rt.anchoredPosition = new Vector2(x, -y); rt.sizeDelta = new Vector2(w, h); return rt;
        }
        Sprite LoadSprite(string name)
        {
            if (!sprites.TryGetValue(name, out var sprite))
            {
                sprite = Resources.Load<Sprite>("Hunter/UI/" + name);
                sprites[name] = sprite;
            }
            return sprite;
        }
        public Image Art(string name, float x, float y, float w, float h, bool sliced = false)
        {
            var image = Rect(name, x, y, w, h).gameObject.AddComponent<Image>();
            image.sprite = LoadSprite(name); image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
            image.raycastTarget = false; return image;
        }
        public Image Panel(float x, float y, float w, float h, Color color)
        {
            var image = Rect("Painel", x, y, w, h).gameObject.AddComponent<Image>();
            image.color = color; image.raycastTarget = false; return image;
        }
        public void Sheet(float x, float y, float w, float h)
        {
            Panel(x + 5, y + 6, w, h, new Color(.07f, .07f, .15f, .25f));
            Panel(x, y, w, h, Paper);
            Panel(x, y, 6, h, new Color(.90f, .48f, .59f));
        }
        public void Banner(string value, float x, float y, float w, float h, int size = 28)
        {
            Panel(x, y, w, h - 12, Plum);
            Label(value, x + 24, y + 3, w - 48, h - 15, size, Color.white, true, TextAnchor.MiddleCenter);
        }
        public Text Label(string value, float x, float y, float w, float h, int size = 22, Color? color = null, bool bold = false, TextAnchor align = TextAnchor.UpperLeft)
        {
            var text = Rect("Texto", x, y, w, h).gameObject.AddComponent<Text>(); text.font = font;
            text.text = value; text.fontSize = size; text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            text.color = color ?? Ink; text.alignment = align; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
            if (bold && size >= 25) { text.font = display; text.fontStyle = FontStyle.Normal; }
            return text;
        }
        public Button Button(string label, float x, float y, float w, float h, Action action, Color? fill = null, bool enabled = true, int size = 21)
        {
            var rt = Rect(label, x, y, w, h); var image = rt.gameObject.AddComponent<Image>(); image.color = fill ?? Teal;
            var shadow = rt.gameObject.AddComponent<Shadow>(); shadow.effectDistance = new Vector2(3, -4);
            shadow.effectColor = new Color(.10f, .08f, .22f, .30f);
            var b = rt.gameObject.AddComponent<Button>(); b.targetGraphic = image; b.interactable = enabled;
            var colors = b.colors; colors.highlightedColor = new Color(1.16f, 1.16f, 1.16f); colors.pressedColor = new Color(.72f, .72f, .72f);
            colors.selectedColor = colors.highlightedColor; colors.disabledColor = new Color(.65f, .65f, .65f, .8f); b.colors = colors;
            b.onClick.AddListener(() => { LastActionFrame = Time.frameCount; action(); });
            var txt = Rect("Texto", 24, 4, w - 48, h - 8, rt).gameObject.AddComponent<Text>();
            txt.font = display; txt.text = label; txt.fontSize = size; txt.fontStyle = FontStyle.Normal;
            txt.color = Color.white; txt.alignment = TextAnchor.MiddleCenter; txt.raycastTarget = false;
            return b;
        }
        public void Backdrop(string title, string subtitle)
        {
            Clear(); Panel(0, 0, 1440, 900, Plum);
            Panel(22, 22, 1396, 856, new Color(.27f, .53f, .61f));
            Panel(22, 22, 340, 856, new Color(.68f, .32f, .49f));
            Panel(1140, 22, 278, 856, new Color(.88f, .62f, .31f));
            Art("minimap_icon_star_yellow", 50, 40, 48, 48);
            Art("minimap_icon_star_yellow", 1342, 40, 48, 48);
            Banner(title, 310, 25, 820, 100, 42);
            Label(subtitle, 100, 130, 1240, 48, 24, Paper, false, TextAnchor.MiddleCenter);
        }
        public void Modal(string title, string intro)
        {
            Clear(); Panel(0, 0, 1440, 900, new Color(.09f, .08f, .20f, .90f)).raycastTarget = true;
            Sheet(190, 92, 1060, 716);
            Banner(title, 214, 106, 1012, 98, 34);
            Label(intro, 238, 224, 960, 102, 23, Muted);
        }
    }
}
