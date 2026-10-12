using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace SquadRush
{
    public sealed class SquadRushHud : MonoBehaviour
    {
        SquadRushGame game;
        RectTransform safeRoot;
        GameObject menu;
        TextMeshProUGUI count, kills, phase, title, description, actionText, toast;
        UnityEngine.UI.Image progress;
        float toastTime;
        int lastCount = -1, lastKills = -1;
        float lastProgress = -1;
        TMP_FontAsset font;
        readonly Color ink = new Color(0.06f, 0.11f, 0.2f);
        readonly Color blue = new Color(0.08f, 0.66f, 0.96f);

        public void Build(SquadRushGame owner)
        {
            game = owner; font = Resources.Load<TMP_FontAsset>("SquadFont");
            var canvasObject = new GameObject("HUD", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            canvasObject.transform.SetParent(transform);
            Canvas canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(540, 960); scaler.matchWidthOrHeight = 1f;
            safeRoot = Panel("SafeArea", canvasObject.transform, Color.clear, new Vector2(0, 0), new Vector2(1, 1));
            ApplySafeArea();

            RectTransform header = Panel("Header", safeRoot, ink, new Vector2(0, 1), new Vector2(1, 1));
            header.pivot = new Vector2(0.5f, 1); header.sizeDelta = new Vector2(0, 112);
            Text("StageLabel", header, "STAGE 01", 17, new Color(0.6f, 0.75f, 0.89f), new Vector2(88, -25), new Vector2(135, 28));
            count = Text("SquadCount", header, "仲間 3", 34, Color.white, new Vector2(114, -67), new Vector2(205, 48));
            TopAnchor(count.rectTransform, 0.23f, -67);
            kills = Text("Kills", header, "撃破 0", 20, Color.white, new Vector2(358, -28), new Vector2(110, 34));
            TopAnchor(kills.rectTransform, 0.68f, -28);
            var pause = Button("Pause", header, "Ⅱ", new Vector2(483, -50), new Vector2(64, 60), new Color(0.18f, 0.27f, 0.4f), game.TogglePause);
            TopAnchor(pause.GetComponent<RectTransform>(), 0.9f, -50);
            RectTransform track = Panel("ProgressTrack", safeRoot, new Color(0.08f, 0.14f, 0.21f), new Vector2(0, 1), new Vector2(1, 1));
            track.pivot = new Vector2(0.5f, 1); track.sizeDelta = new Vector2(-40, 8); track.anchoredPosition = new Vector2(0, -128);
            progress = Panel("ProgressFill", track, blue, Vector2.zero, Vector2.one).GetComponent<UnityEngine.UI.Image>();
            progress.rectTransform.anchorMax = new Vector2(0, 1);
            phase = Text("Phase", safeRoot, "仲間を増やそう", 21, Color.white, new Vector2(270, -160), new Vector2(460, 35));
            TopAnchor(phase.rectTransform, 0.5f, -160);
            toast = Text("Toast", safeRoot, "", 25, Color.white, new Vector2(270, -215), new Vector2(480, 45));
            TopAnchor(toast.rectTransform, 0.5f, -215);
            var footer = Panel("ControlHint", safeRoot, new Color(0.05f, 0.1f, 0.17f, 0.9f), new Vector2(0.05f, 0.025f), new Vector2(0.95f, 0.085f));
            var hint = Text("Hint", footer, "←   指を左右にドラッグ   →", 20, Color.white, Vector2.zero, Vector2.zero);
            Stretch(hint.rectTransform);

            RectTransform scrim = Panel("Menu", safeRoot, new Color(0.03f, 0.07f, 0.13f, 0.67f), Vector2.zero, Vector2.one);
            scrim.GetComponent<UnityEngine.UI.Image>().raycastTarget = true; menu = scrim.gameObject;
            RectTransform card = Panel("Card", scrim, ink, new Vector2(0.08f, 0.27f), new Vector2(0.92f, 0.77f));
            RectTransform accent = Panel("Accent", card, blue, new Vector2(0, 0.97f), Vector2.one);
            title = Text("Title", card, "SQUAD\nRUSH", 53, Color.white, Vector2.zero, Vector2.zero);
            SetAnchors(title.rectTransform, new Vector2(0.05f, 0.53f), new Vector2(0.95f, 0.92f)); title.fontStyle = FontStyles.Bold;
            description = Text("Description", card, "", 21, new Color(0.76f, 0.85f, 0.92f), Vector2.zero, Vector2.zero);
            SetAnchors(description.rectTransform, new Vector2(0.08f, 0.27f), new Vector2(0.92f, 0.52f));
            var action = Button("Play", card, "スタート", Vector2.zero, new Vector2(330, 66), blue, game.MenuAction);
            action.GetComponent<RectTransform>().anchorMin = action.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.15f);
            action.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            actionText = action.GetComponentInChildren<TextMeshProUGUI>();
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.transform.SetParent(transform);
            }
        }

        public void ApplySafeArea()
        {
            if (safeRoot == null || Screen.width <= 0 || Screen.height <= 0) return;
            Rect safe = Screen.safeArea;
            Rect viewport = game.View.pixelRect;
            float xmin = Mathf.Max(safe.xMin, viewport.xMin), xmax = Mathf.Min(safe.xMax, viewport.xMax);
            safe = Rect.MinMaxRect(xmin, safe.yMin, xmax, safe.yMax);
            safeRoot.anchorMin = safe.position / new Vector2(Screen.width, Screen.height);
            safeRoot.anchorMax = (safe.position + safe.size) / new Vector2(Screen.width, Screen.height);
            safeRoot.offsetMin = safeRoot.offsetMax = Vector2.zero;
        }
        public void UpdateStats()
        {
            if (game.SquadCount != lastCount) { lastCount = game.SquadCount; count.text = "仲間 " + lastCount; }
            if (game.Kills != lastKills) { lastKills = game.Kills; kills.text = "撃破 " + lastKills; }
            float fill = Mathf.Clamp01(game.Elapsed / SquadRushGame.StageSeconds);
            if (Mathf.Abs(fill - lastProgress) > 0.005f) { lastProgress = fill; progress.rectTransform.anchorMax = new Vector2(fill, 1); }
            string status = game.Elapsed >= SquadRushGame.StageSeconds ? "ボスを倒せ！" : "仲間を増やそう";
            if (phase.text != status) phase.text = status;
        }
        public void ShowMenu(string heading, string body, string action)
        {
            title.text = heading; title.fontSize = heading.StartsWith("SQUAD") ? 53 : 34;
            description.text = body; actionText.text = action; menu.SetActive(true);
        }
        public void HideMenu() { menu.SetActive(false); }
        public void Flash(string message) { toast.text = message; toast.alpha = 1; toastTime = 1.7f; }
        void Update()
        {
            if (toastTime <= 0) return;
            toastTime -= Time.unscaledDeltaTime;
            toast.alpha = Mathf.Clamp01(toastTime * 2);
            if (toastTime <= 0) toast.text = "";
        }
        RectTransform Panel(string name, Transform parent, Color color, Vector2 min, Vector2 max)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            obj.transform.SetParent(parent, false);
            var image = obj.GetComponent<UnityEngine.UI.Image>(); image.color = color; image.raycastTarget = false;
            var rect = obj.GetComponent<RectTransform>(); SetAnchors(rect, min, max); return rect;
        }
        TextMeshProUGUI Text(string name, Transform parent, string value, int size, Color color, Vector2 position, Vector2 dimensions)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)); obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.anchoredPosition = position; rect.sizeDelta = dimensions;
            var label = obj.GetComponent<TextMeshProUGUI>(); label.font = font; label.text = value;
            label.fontSize = size; label.enableAutoSizing = false; label.color = color; label.raycastTarget = false;
            label.alignment = TextAlignmentOptions.Center; return label;
        }
        GameObject Button(string name, Transform parent, string label, Vector2 position, Vector2 size, Color color, UnityEngine.Events.UnityAction onClick)
        {
            RectTransform rect = Panel(name, parent, color, new Vector2(0, 1), new Vector2(0, 1));
            rect.anchoredPosition = position; rect.sizeDelta = size;
            rect.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
            var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>(); button.onClick.AddListener(onClick);
            button.targetGraphic = rect.GetComponent<UnityEngine.UI.Image>();
            var text = Text("Label", rect, label, 24, Color.white, Vector2.zero, Vector2.zero); Stretch(text.rectTransform);
            return rect.gameObject;
        }
        static void Stretch(RectTransform rect) { SetAnchors(rect, Vector2.zero, Vector2.one); }
        static void TopAnchor(RectTransform rect, float x, float y)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(x, 1); rect.anchoredPosition = new Vector2(0, y);
        }
        static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
