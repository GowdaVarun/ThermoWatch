using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace DataCenterGuard
{
    /// <summary>
    /// Adds a small "LABELS: ON/OFF" toggle button at the top of the screen
    /// (plus the L key as a shortcut) that shows or hides every billboard
    /// label in the scene. Builds its own canvas so the scene builder only
    /// has to drop this component on an empty GameObject.
    /// </summary>
    [ExecuteAlways]
    public class LabelToggleUI : MonoBehaviour
    {
        private const string OnText = "LABELS: ON";
        private const string OffText = "LABELS: OFF";
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        private static readonly Color ButtonColor = new Color(0.13f, 0.14f, 0.16f, 0.88f);
        private static readonly Color ButtonTextOn = new Color(0.95f, 0.96f, 0.97f);
        private static readonly Color ButtonTextOff = new Color(0.55f, 0.57f, 0.60f);

        private Canvas canvas;
        private Text buttonText;
        private Image buttonImage;
        private bool labelsVisible = true;

        private void Awake()
        {
            EnsureEventSystem();
            EnsureCanvas();
            ApplyVisibility();
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.lKey.wasPressedThisFrame)
                ToggleLabels();
#endif
        }

        /// <summary>
        /// Toggles every billboard label on or off. Also wired to the button.
        /// </summary>
        public void ToggleLabels()
        {
            labelsVisible = !labelsVisible;
            ApplyVisibility();
        }

        private void ApplyVisibility()
        {
            Billboard[] labels = FindObjectsByType<Billboard>(FindObjectsInactive.Include);
            foreach (Billboard label in labels)
            {
                if (label.gameObject.activeSelf != labelsVisible)
                    label.gameObject.SetActive(labelsVisible);
            }

            if (buttonText != null)
            {
                buttonText.text = labelsVisible ? OnText : OffText;
                buttonText.color = labelsVisible ? ButtonTextOn : ButtonTextOff;
            }

            if (buttonImage != null)
            {
                buttonImage.color = labelsVisible ? ButtonColor : new Color(0.45f, 0.47f, 0.50f, 0.75f);
            }
        }

        // -------------------------------------------------------------
        // UI construction
        // -------------------------------------------------------------

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
                return;

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.transform.SetParent(transform, false);
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        }

        private void EnsureCanvas()
        {
            canvas = GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                buttonText = canvas.GetComponentInChildren<Text>(true);
                return;
            }

            Camera renderCamera = Camera.main;

            GameObject canvasObject = new GameObject("Label Canvas");
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = renderCamera;
            canvas.planeDistance = 10f;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            CreateToggleButton(canvasObject.transform);
        }

        private void CreateToggleButton(Transform canvasTransform)
        {
            GameObject buttonObject = new GameObject("Labels Toggle");
            buttonObject.transform.SetParent(canvasTransform, false);

            RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 1f);
            buttonRect.anchorMax = new Vector2(0.5f, 1f);
            buttonRect.pivot = new Vector2(0.5f, 1f);
            buttonRect.anchoredPosition = new Vector2(0f, -16f);
            buttonRect.sizeDelta = new Vector2(190f, 40f);

            buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = ButtonColor;

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(ToggleLabels);

            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(buttonObject.transform, false);

            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            buttonText = textObject.AddComponent<Text>();
            buttonText.font = GetUiFont();
            buttonText.fontSize = 24;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.text = OnText;
            buttonText.color = ButtonTextOn;
            buttonText.raycastTarget = false;
        }

        private static Font GetUiFont()
        {
            Font font = null;
            try
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch
            {
                // Fall through to an OS font below.
            }

            if (font == null)
                font = Font.CreateDynamicFontFromOSFont(
                    new[] { "DejaVu Sans", "Liberation Sans", "Arial" }, 24);

            return font;
        }
    }
}
