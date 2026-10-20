using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RestaurantChaos.Tutorial
{
    public class TutorialSignUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text messageText;

        [Header("Animation")]
        [SerializeField] private float fadeDuration = 0.35f;
        [SerializeField] private float scaleDuration = 0.3f;

        [Header("Fit")]
        [SerializeField] private float minFit = 0.4f;

        private const int FitIterations = 10;
        private const int ChainGraceFrames = 3;
        private const int MaxHoles = 8;
        private const float BubbleMargin = 0.03f;
        private const string SpotlightShaderPath = "Shaders/TutorialSpotlight";

        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int DimAlphaId = Shader.PropertyToID("_DimAlpha");
        private static readonly int FadeId = Shader.PropertyToID("_Fade");
        private static readonly int PulseId = Shader.PropertyToID("_Pulse");
        private static readonly int HolesId = Shader.PropertyToID("_Holes");
        private static readonly int HoleCountId = Shader.PropertyToID("_HoleCount");
        private static readonly int RectSizeId = Shader.PropertyToID("_RectSize");

        private Image dimBackground;
        private Transform bubble;
        private RectTransform panelRect;
        private RectTransform bubbleRect;
        private RawImage spotlightImage;
        private Material spotlightMaterial;
        private readonly Vector4[] holeData = new Vector4[MaxHoles];
        private Rect spotlightUnion;
        private bool useSpotlight;
        private float currentDimAlpha;
        private float dimTargetAlpha = 1f;
        private Vector3 bubbleBaseScale = Vector3.one;
        private Vector3 bubbleBaseLocalPosition;
        private Vector3 textBaseScale = Vector3.one;
        private Vector2 textBaseSize;
        private float currentFit = 1f;
        private bool wasMenuPaused;
        private bool isClosing;
        private Coroutine pendingClose;

        public static bool IsAnySignShowing { get; private set; }
        public static bool IsScreenCovered { get; private set; }

        public bool IsShowing { get; private set; }

        private void Awake()
        {
            IsAnySignShowing = false;
            IsScreenCovered = false;

            if (panel != null)
            {
                dimBackground = panel.GetComponent<Image>();
                panelRect = panel.GetComponent<RectTransform>();

                if (panel.transform.childCount > 0)
                {
                    bubble = panel.transform.GetChild(0);
                    bubbleRect = bubble as RectTransform;
                }

                if (dimBackground != null)
                {
                    dimTargetAlpha = dimBackground.color.a;
                }

                if (bubble != null)
                {
                    bubbleBaseScale = bubble.localScale;
                    bubbleBaseLocalPosition = bubble.localPosition;
                }

                if (messageText != null)
                {
                    textBaseScale = messageText.rectTransform.localScale;
                    textBaseSize = messageText.rectTransform.sizeDelta;
                }

                CreateSpotlight();

                panel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            IsAnySignShowing = false;
            IsScreenCovered = false;
            WorldTime.SetTutorialFrozen(false);

            if (spotlightMaterial != null)
            {
                Destroy(spotlightMaterial);
            }
        }

        private void Update()
        {
            if (!IsShowing || !useSpotlight || spotlightMaterial == null)
            {
                return;
            }

            float pulse = 0.65f + 0.35f * Mathf.Sin(Time.unscaledTime * 4f);

            spotlightMaterial.SetFloat(PulseId, pulse);
        }

        private void CreateSpotlight()
        {
            Shader shader = Resources.Load<Shader>(SpotlightShaderPath);

            if (shader == null)
            {
                Debug.LogWarning("TutorialSignUI: spotlight shader not found, highlights will be skipped.");
                return;
            }

            GameObject spotlightObject = new GameObject("Spotlight", typeof(RectTransform), typeof(RawImage));
            spotlightObject.layer = panel.layer;
            spotlightObject.transform.SetParent(panel.transform, false);
            spotlightObject.transform.SetAsFirstSibling();

            RectTransform spotlightRect = (RectTransform)spotlightObject.transform;
            spotlightRect.anchorMin = Vector2.zero;
            spotlightRect.anchorMax = Vector2.one;
            spotlightRect.offsetMin = Vector2.zero;
            spotlightRect.offsetMax = Vector2.zero;

            spotlightMaterial = new Material(shader);

            Color dimColor = Color.black;

            if (dimBackground != null)
            {
                dimColor = dimBackground.color;
            }

            spotlightMaterial.SetColor(ColorId, new Color(dimColor.r, dimColor.g, dimColor.b, 1f));
            spotlightMaterial.SetFloat(DimAlphaId, dimTargetAlpha);
            spotlightMaterial.SetFloat(FadeId, 0f);

            spotlightImage = spotlightObject.GetComponent<RawImage>();
            spotlightImage.raycastTarget = false;
            spotlightImage.material = spotlightMaterial;

            spotlightObject.SetActive(false);
        }

        public IEnumerator ShowPages(string[] pages)
        {
            return ShowPages(pages, null);
        }

        public IEnumerator ShowPages(string[] pages, IList<Rect> highlightScreenRects)
        {
            if (pages == null || pages.Length == 0)
            {
                yield break;
            }

            while (isClosing)
            {
                yield return null;
            }

            if (pendingClose != null)
            {
                StopCoroutine(pendingClose);
                pendingClose = null;
            }

            wasMenuPaused = WorldTime.IsMenuPaused;

            if (!IsShowing)
            {
                yield return OpenPanel(pages[0], highlightScreenRects);
            }
            else
            {
                ConfigureSpotlight(highlightScreenRects);
            }

            for (int i = 0; i < pages.Length; i++)
            {
                SetPage(pages[i]);
                PlaceBubble();
                SetBubbleProgress(1f);

                yield return null;

                while (!WasClicked())
                {
                    yield return null;
                }
            }

            pendingClose = StartCoroutine(ClosePanelAfterGrace());
        }

        private IEnumerator OpenPanel(string firstPage, IList<Rect> highlightScreenRects)
        {
            IsShowing = true;
            IsAnySignShowing = true;
            WorldTime.SetTutorialFrozen(true);

            SetDimAlpha(0f);
            SetBubbleProgress(0f);
            panel.SetActive(true);
            ConfigureSpotlight(highlightScreenRects);
            SetPage(firstPage);
            PlaceBubble();
            SetBubbleProgress(0f);

            yield return FadeDim(0f, dimTargetAlpha);

            IsScreenCovered = true;

            yield return GrowBubble();
        }

        private IEnumerator ClosePanelAfterGrace()
        {
            for (int i = 0; i < ChainGraceFrames; i++)
            {
                yield return null;
            }

            isClosing = true;
            pendingClose = null;

            yield return ShrinkBubble();

            IsScreenCovered = false;

            yield return FadeDim(dimTargetAlpha, 0f);

            panel.SetActive(false);
            IsShowing = false;
            IsAnySignShowing = false;
            isClosing = false;
            WorldTime.SetTutorialFrozen(false);
        }

        private IEnumerator FadeDim(float from, float to)
        {
            float duration = Mathf.Max(fadeDuration, 0.01f);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t);

                SetDimAlpha(Mathf.Lerp(from, to, t));

                yield return null;
            }

            SetDimAlpha(to);
        }

        private IEnumerator GrowBubble()
        {
            float duration = Mathf.Max(scaleDuration, 0.01f);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                float remaining = 1f - t;

                SetBubbleProgress(1f - remaining * remaining * remaining);

                yield return null;
            }

            SetBubbleProgress(1f);
        }

        private IEnumerator ShrinkBubble()
        {
            float duration = Mathf.Max(scaleDuration, 0.01f);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(elapsed / duration);

                SetBubbleProgress(1f - t * t * t);

                yield return null;
            }

            SetBubbleProgress(0f);
        }

        private void ConfigureSpotlight(IList<Rect> screenRects)
        {
            useSpotlight = spotlightImage != null && screenRects != null && screenRects.Count > 0;

            if (!useSpotlight)
            {
                if (spotlightImage != null)
                {
                    spotlightImage.gameObject.SetActive(false);
                }

                if (bubble != null)
                {
                    bubble.localPosition = bubbleBaseLocalPosition;
                }

                ApplyDimAlpha();
                return;
            }

            spotlightImage.gameObject.SetActive(true);
            BuildHoles(screenRects);
            ApplyDimAlpha();
        }

        private void BuildHoles(IList<Rect> screenRects)
        {
            RectTransform spotlightRect = spotlightImage.rectTransform;
            Canvas canvas = spotlightImage.canvas;
            Camera canvasCamera = null;

            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                canvasCamera = canvas.worldCamera;
            }

            Rect localRect = spotlightRect.rect;
            Vector2 screenMin = RectTransformUtility.WorldToScreenPoint(canvasCamera, spotlightRect.TransformPoint(localRect.min));
            Vector2 screenMax = RectTransformUtility.WorldToScreenPoint(canvasCamera, spotlightRect.TransformPoint(localRect.max));
            Vector2 pixelSize = new Vector2(Mathf.Max(Mathf.Abs(screenMax.x - screenMin.x), 1f), Mathf.Max(Mathf.Abs(screenMax.y - screenMin.y), 1f));

            int holeCount = Mathf.Min(screenRects.Count, MaxHoles);
            Vector2 unionMin = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 unionMax = new Vector2(float.MinValue, float.MinValue);

            for (int i = 0; i < MaxHoles; i++)
            {
                if (i >= holeCount)
                {
                    holeData[i] = Vector4.zero;
                    continue;
                }

                Rect hole = screenRects[i];
                Vector2 center = new Vector2((hole.center.x - screenMin.x) / pixelSize.x, (hole.center.y - screenMin.y) / pixelSize.y);
                Vector2 half = new Vector2(hole.width * 0.5f / pixelSize.x, hole.height * 0.5f / pixelSize.y);

                holeData[i] = new Vector4(center.x, center.y, half.x, half.y);

                unionMin = Vector2.Min(unionMin, center - half);
                unionMax = Vector2.Max(unionMax, center + half);
            }

            spotlightUnion = Rect.MinMaxRect(unionMin.x, unionMin.y, unionMax.x, unionMax.y);

            spotlightMaterial.SetVectorArray(HolesId, holeData);
            spotlightMaterial.SetFloat(HoleCountId, holeCount);
            spotlightMaterial.SetVector(RectSizeId, new Vector4(pixelSize.x, pixelSize.y, 0f, 0f));
        }

        private void PlaceBubble()
        {
            if (!useSpotlight || bubble == null || bubbleRect == null || panelRect == null)
            {
                return;
            }

            Rect panelBounds = panelRect.rect;

            if (panelBounds.width <= 0f || panelBounds.height <= 0f)
            {
                return;
            }

            float halfWidth = Mathf.Abs(bubbleRect.rect.width * bubbleBaseScale.x * currentFit) * 0.5f / panelBounds.width;
            float halfHeight = Mathf.Abs(bubbleRect.rect.height * bubbleBaseScale.y * currentFit) * 0.5f / panelBounds.height;

            float freeAbove = 1f - spotlightUnion.yMax;
            float freeBelow = spotlightUnion.yMin;
            float y;

            if (freeAbove >= freeBelow)
            {
                y = spotlightUnion.yMax + BubbleMargin + halfHeight;
            }
            else
            {
                y = spotlightUnion.yMin - BubbleMargin - halfHeight;
            }

            y = Mathf.Clamp(y, halfHeight, 1f - halfHeight);

            float x = Mathf.Clamp(spotlightUnion.center.x, halfWidth, 1f - halfWidth);

            bubble.localPosition = new Vector3(panelBounds.xMin + x * panelBounds.width, panelBounds.yMin + y * panelBounds.height, bubble.localPosition.z);
        }

        private void SetDimAlpha(float alpha)
        {
            currentDimAlpha = alpha;

            ApplyDimAlpha();
        }

        private void ApplyDimAlpha()
        {
            float panelAlpha = currentDimAlpha;
            float spotlightFade = 0f;

            if (useSpotlight)
            {
                panelAlpha = 0f;
                spotlightFade = currentDimAlpha / Mathf.Max(dimTargetAlpha, 0.0001f);
            }

            if (dimBackground != null)
            {
                Color color = dimBackground.color;
                color.a = panelAlpha;
                dimBackground.color = color;
            }

            if (spotlightMaterial != null)
            {
                spotlightMaterial.SetFloat(FadeId, spotlightFade);
            }
        }

        private void SetBubbleProgress(float progress)
        {
            if (bubble == null)
            {
                return;
            }

            bubble.localScale = bubbleBaseScale * (currentFit * progress);
        }

        private void SetPage(string text)
        {
            messageText.text = text;

            ApplyFit(ComputeFit(text));
        }

        private void ApplyFit(float fit)
        {
            currentFit = fit;

            RectTransform textRect = messageText.rectTransform;
            textRect.sizeDelta = textBaseSize * fit;
            textRect.localScale = textBaseScale / fit;
        }

        private float ComputeFit(string text)
        {
            float lowest = Mathf.Clamp(minFit, 0.05f, 1f);

            if (TextFits(text, lowest))
            {
                return lowest;
            }

            float low = lowest;
            float high = 1f;

            for (int i = 0; i < FitIterations; i++)
            {
                float middle = (low + high) * 0.5f;

                if (TextFits(text, middle))
                {
                    high = middle;
                }
                else
                {
                    low = middle;
                }
            }

            return high;
        }

        private bool TextFits(string text, float fit)
        {
            float width = textBaseSize.x * fit;
            float height = textBaseSize.y * fit;

            Vector2 preferred = messageText.GetPreferredValues(text, width, height);

            return preferred.y <= height;
        }

        private bool WasClicked()
        {
            bool isMenuPaused = WorldTime.IsMenuPaused;
            bool wasPausedLastCheck = wasMenuPaused;

            wasMenuPaused = isMenuPaused;

            if (isMenuPaused || wasPausedLastCheck)
            {
                return false;
            }

            return Input.GetMouseButtonDown(0);
        }
    }
}
