using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace SquadRush
{
    public sealed partial class SquadRushHud : MonoBehaviour
    {
        SquadRushGame game;
        RectTransform safeRoot;
        RectTransform battleRoot;
        GameObject menu;
        TextMeshProUGUI count, kills, phase, title, description, actionText, toast, stageLabel, weapon;
        UnityEngine.UI.Image progress;
        float toastTime;
        int lastCount = -1, lastKills = -1, lastStage = -1;
        float lastProgress = -1;
        int lastPower = -1;
        float lastFireRate = -1;
        TMP_FontAsset font;
        Sprite rounded;
        Texture2D roundedTexture;
        readonly Color ink = new Color(0.035f, 0.075f, 0.135f, 0.96f);
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
            battleRoot = Panel("BattleHud", safeRoot, Color.clear, Vector2.zero, Vector2.one);

            RectTransform header = Panel("Header", battleRoot, ink, new Vector2(0, 1), new Vector2(1, 1));
            header.pivot = new Vector2(0.5f, 1); header.sizeDelta = new Vector2(0, 96);
            stageLabel = Text("StageLabel", header, "STAGE 01", 17, new Color(0.6f, 0.75f, 0.89f), new Vector2(110, -25), new Vector2(195, 28));
            count = Text("SquadCount", header, "仲間 3", 34, Color.white, new Vector2(114, -67), new Vector2(240, 56));
            TopAnchor(count.rectTransform, 0.23f, -67);
            kills = Text("Kills", header, "撃破 0", 20, Color.white, new Vector2(358, -28), new Vector2(110, 34));
            TopAnchor(kills.rectTransform, 0.68f, -28);
            weapon = Text("WeaponStats", header, "", 16, new Color(1, 0.83f, 0.43f), Vector2.zero, new Vector2(165, 48));
            TopAnchor(weapon.rectTransform, 0.66f, -68);
            var pause = Button("Pause", header, "Ⅱ", new Vector2(483, -50), new Vector2(64, 60), new Color(0.18f, 0.27f, 0.4f), game.TogglePause);
            TopAnchor(pause.GetComponent<RectTransform>(), 0.9f, -50);
            RectTransform track = Panel("ProgressTrack", battleRoot, new Color(0.08f, 0.14f, 0.21f), new Vector2(0, 1), new Vector2(1, 1));
            track.pivot = new Vector2(0.5f, 1); track.sizeDelta = new Vector2(-40, 6); track.anchoredPosition = new Vector2(0, -112);
            progress = Panel("ProgressFill", track, blue, Vector2.zero, Vector2.one).GetComponent<UnityEngine.UI.Image>();
            progress.rectTransform.anchorMax = new Vector2(0, 1);
            var phasePlate = Panel("PhasePlate", battleRoot, new Color(0.04f, 0.09f, 0.15f, 0.72f), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            phasePlate.sizeDelta = new Vector2(220, 34); phasePlate.anchoredPosition = new Vector2(0, -142);
            phase = Text("Phase", battleRoot, "仲間を増やそう", 21, Color.white, new Vector2(270, -160), new Vector2(460, 35));
            TopAnchor(phase.rectTransform, 0.5f, -142);
            toast = Text("Toast", battleRoot, "", 25, Color.white, new Vector2(270, -215), new Vector2(480, 45));
            TopAnchor(toast.rectTransform, 0.5f, -190);
            foreach (var label in new[] { phase, toast })
            {
                var textShadow = label.gameObject.AddComponent<UnityEngine.UI.Shadow>();
                textShadow.effectColor = new Color(0.025f, 0.04f, 0.07f, 0.85f); textShadow.effectDistance = new Vector2(1.5f, -2);
            }
            var footer = Panel("ControlHint", battleRoot, new Color(0.05f, 0.1f, 0.17f, 0.9f), new Vector2(0.05f, 0.025f), new Vector2(0.95f, 0.085f));
            var hint = Text("Hint", footer, "←   指を左右にドラッグ   →", 20, Color.white, Vector2.zero, Vector2.zero);
            Stretch(hint.rectTransform);

            RectTransform scrim = Panel("Menu", safeRoot, new Color(0.03f, 0.07f, 0.13f, 0.67f), Vector2.zero, Vector2.one);
            scrim.GetComponent<UnityEngine.UI.Image>().raycastTarget = true; menu = scrim.gameObject;
            RectTransform card = Panel("Card", scrim, ink, new Vector2(0.08f, 0.27f), new Vector2(0.92f, 0.77f));
            var shadow = card.gameObject.AddComponent<UnityEngine.UI.Shadow>(); shadow.effectColor = new Color(0, 0, 0, 0.35f); shadow.effectDistance = new Vector2(0, -8);
            RectTransform accent = Panel("Accent", card, blue, new Vector2(0, 0.97f), Vector2.one);
            title = Text("Title", card, "SQUAD\nRUSH", 53, Color.white, Vector2.zero, Vector2.zero);
            SetAnchors(title.rectTransform, new Vector2(0.05f, 0.53f), new Vector2(0.95f, 0.92f)); title.fontStyle = FontStyles.Bold;
            description = Text("Description", card, "", 21, new Color(0.76f, 0.85f, 0.92f), Vector2.zero, Vector2.zero);
            SetAnchors(description.rectTransform, new Vector2(0.08f, 0.27f), new Vector2(0.92f, 0.52f));
            var action = Button("Play", card, "スタート", Vector2.zero, new Vector2(330, 66), blue, game.MenuAction);
            action.GetComponent<RectTransform>().anchorMin = action.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.20f);
            action.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            actionText = action.GetComponentInChildren<TextMeshProUGUI>();
            var homeButton = Button("ReturnHome", card, "ホームへ", Vector2.zero, new Vector2(300, 38), new Color(0.13f, 0.21f, 0.31f), game.ShowHome);
            homeButton.GetComponent<RectTransform>().anchorMin = homeButton.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.065f);
            homeButton.GetComponentInChildren<TextMeshProUGUI>().fontSize = 19;
            BuildFrontEnd();
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
            if (lastStage != game.StageNumber) { lastStage = game.StageNumber; stageLabel.text = "STAGE " + game.StageNumber.ToString("D2") + " · " + game.Stage.Name; }
            if (game.SquadCount != lastCount) { lastCount = game.SquadCount; count.text = "仲間 " + lastCount; }
            if (game.Kills != lastKills) { lastKills = game.Kills; kills.text = "撃破 " + lastKills; }
            if (lastPower != game.WeaponDamage || !Mathf.Approximately(lastFireRate, game.FireRateMultiplier))
            {
                lastPower = game.WeaponDamage; lastFireRate = game.FireRateMultiplier;
                weapon.text = "武器 威力 " + game.WeaponDamage + "\n連射 " + game.FireRateMultiplier.ToString("0.0") + "倍";
            }
            float fill = Mathf.Clamp01(game.Elapsed / game.StageDuration);
            if (Mathf.Abs(fill - lastProgress) > 0.005f) { lastProgress = fill; progress.rectTransform.anchorMax = new Vector2(fill, 1); }
            string status = game.Elapsed >= game.StageDuration ? "ボスを倒せ！" : "仲間を増やそう";
            if (phase.text != status) phase.text = status;
        }
        public void ShowMenu(string heading, string body, string action)
        {
            HideFrontEnd();
            title.text = heading.StartsWith("SQUAD") ? "SQUAD\n<color=#53CFFF>RUSH</color>" : heading; title.fontSize = heading.StartsWith("SQUAD") ? 53 : 34;
            description.text = body; actionText.text = action; menu.SetActive(true);
        }
        public void HideMenu() { menu.SetActive(false); toastTime = 0; toast.text = ""; }
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
            if (name == "Card" || name == "Play" || name == "Pause" || name == "ControlHint" || name == "PhasePlate")
            { image.sprite = RoundedSprite(); image.type = UnityEngine.UI.Image.Type.Sliced; }
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
            var colors = button.colors; colors.highlightedColor = new Color(0.90f, 0.96f, 1); colors.pressedColor = new Color(0.63f, 0.77f, 0.90f); colors.fadeDuration = 0.12f; button.colors = colors;
            var text = Text("Label", rect, label, 24, Color.white, Vector2.zero, Vector2.zero); Stretch(text.rectTransform);
            return rect.gameObject;
        }
        Sprite RoundedSprite()
        {
            if (rounded != null) return rounded;
            const int size = 64;
            roundedTexture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "Rounded UI corners", filterMode = FilterMode.Bilinear };
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(Mathf.Abs(x + 0.5f - size / 2f) - 19, 0), dy = Mathf.Max(Mathf.Abs(y + 0.5f - size / 2f) - 19, 0);
                pixels[y * size + x] = new Color(1, 1, 1, Mathf.Clamp01(12.5f - Mathf.Sqrt(dx * dx + dy * dy)));
            }
            roundedTexture.SetPixels(pixels); roundedTexture.Apply(false, true);
            rounded = Sprite.Create(roundedTexture, new Rect(0, 0, size, size), Vector2.one * 0.5f, 100, 0, SpriteMeshType.FullRect, Vector4.one * 14);
            return rounded;
        }
        void OnDestroy()
        {
            if (Application.isPlaying) { if (rounded != null) Destroy(rounded); if (roundedTexture != null) Destroy(roundedTexture); }
            else { if (rounded != null) DestroyImmediate(rounded); if (roundedTexture != null) DestroyImmediate(roundedTexture); }
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
