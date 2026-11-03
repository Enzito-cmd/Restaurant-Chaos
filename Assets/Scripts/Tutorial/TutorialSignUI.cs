using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RestaurantChaos.Tutorial
{
    public class SignOptions
    {
        public IList<Rect> highlightRects;
        public Color holeTint = new Color(1f, 1f, 1f, 0f);
        public bool showMouseIcon;
        public GameObject[] showObjects;
        public bool hideBubble;
        public Func<bool> advanceCondition;
        public bool releaseAtOnce;
    }

    public class TutorialSignUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private RectTransform bubble;
        [SerializeField] private TMP_Text messageText;

        [Header("Spotlight")]
        [SerializeField] private RawImage spotlightImage;

        [Header("Mouse Icons")]
        [SerializeField] private CanvasGroup skipHint;
        [SerializeField] private GameObject stepMouseIcon;

        [Header("Animation")]
        [SerializeField] private float fadeDuration = 0.35f;
        [SerializeField] private float scaleDuration = 0.3f;
        [SerializeField] private float focusSwapDuration = 0.22f;

        [Header("Text")]
        [SerializeField] private float minFit = 0.4f;
        [SerializeField] private float charactersPerSecond = 45f;

        private const int FitIterations = 10;
        private const int ChainGraceFrames = 3;
        private const int MaxHoles = 16;
        private const float BubbleMargin = 0.03f;
        private const float HintFadeSpeed = 6f;

        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int DimAlphaId = Shader.PropertyToID("_DimAlpha");
        private static readonly int FadeId = Shader.PropertyToID("_Fade");
        private static readonly int HoleScaleId = Shader.PropertyToID("_HoleScale");
        private static readonly int HoleTintId = Shader.PropertyToID("_HoleTint");
        private static readonly int PulseId = Shader.PropertyToID("_Pulse");
        private static readonly int HolesId = Shader.PropertyToID("_Holes");
        private static readonly int HoleCountId = Shader.PropertyToID("_HoleCount");
        private static readonly int RectSizeId = Shader.PropertyToID("_RectSize");

        private readonly Vector4[] holeData = new Vector4[MaxHoles];
        private readonly List<GameObject> shownObjects = new List<GameObject>();
        private Image dimBackground;
        private RectTransform panelRect;
        private Material spotlightMaterial;
        private Rect spotlightUnion;
        private Vector3 bubbleBaseScale = Vector3.one;
        private Vector3 bubbleBaseLocalPosition;
        private Vector3 textBaseScale = Vector3.one;
        private Vector2 textBaseSize;
        private float dimTargetAlpha = 1f;
        private float currentDimAlpha;
        private float holeSwapScale = 1f;
        private float currentFit = 1f;
        private bool useSpotlight;
        private bool isTyping;
        private bool bubbleHiddenBySwap;
        private bool hideBubble;
        private bool releaseOnClose;
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

            dimBackground = panel.GetComponent<Image>();
            panelRect = panel.GetComponent<RectTransform>();

            dimTargetAlpha = dimBackground.color.a;
            bubbleBaseScale = bubble.localScale;
            bubbleBaseLocalPosition = bubble.localPosition;
            textBaseScale = messageText.rectTransform.localScale;
            textBaseSize = messageText.rectTransform.sizeDelta;

            if (spotlightImage != null)
            {
                Color dimColor = dimBackground.color;

                spotlightMaterial = new Material(spotlightImage.material);
                spotlightMaterial.SetColor(ColorId, new Color(dimColor.r, dimColor.g, dimColor.b, 1f));
                spotlightMaterial.SetFloat(DimAlphaId, dimTargetAlpha);
                spotlightImage.material = spotlightMaterial;
            }

            if (skipHint != null)
            {
                skipHint.alpha = 0f;
            }

            if (stepMouseIcon != null)
            {
                stepMouseIcon.SetActive(false);
            }

            panel.SetActive(false);
        }

        private void OnDestroy()
        {
            IsAnySignShowing = false;
            IsScreenCovered = false;
            WorldTime.SetTutorialFrozen(false);

            Destroy(spotlightMaterial);
        }

        private void Update()
        {
            if (!IsShowing)
            {
                return;
            }

            if (useSpotlight)
            {
                spotlightMaterial.SetFloat(PulseId, 0.65f + 0.35f * Mathf.Sin(Time.unscaledTime * 4f));
            }

            if (skipHint != null)
            {
                float hintTarget = 0f;

                if ((isTyping || hideBubble) && !isClosing)
                {
                    hintTarget = 1f;
                }

                skipHint.alpha = Mathf.MoveTowards(skipHint.alpha, hintTarget, Time.unscaledDeltaTime * HintFadeSpeed);
            }
        }

        public IEnumerator ShowPages(string[] pages)
        {
            return ShowPages(pages, null);
        }

        public IEnumerator ShowPages(string[] pages, SignOptions options)
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
            releaseOnClose = false;
            hideBubble = options != null && options.hideBubble;

            if (!IsShowing)
            {
                yield return OpenPanel(pages[0], options);
            }
            else
            {
                IsAnySignShowing = true;
                IsScreenCovered = true;
                WorldTime.SetTutorialFrozen(true);

                yield return SwapSpotlight(options);
            }

            foreach (string page in pages)
            {
                yield return TypePage(page, options);
            }

            releaseOnClose = options != null && options.releaseAtOnce;
            pendingClose = StartCoroutine(ClosePanelAfterGrace());
        }

        private IEnumerator OpenPanel(string firstPage, SignOptions options)
        {
            IsShowing = true;
            IsAnySignShowing = true;
            WorldTime.SetTutorialFrozen(true);

            holeSwapScale = 1f;
            bubbleHiddenBySwap = false;

            SetDimAlpha(0f);
            SetBubbleProgress(0f);
            panel.SetActive(true);
            ShowObjects(options);
            ConfigureSpotlight(options);
            SetPage(firstPage);
            PlaceBubble();

            yield return FadeDim(0f, dimTargetAlpha);

            IsScreenCovered = true;

            SoundManager.Instance?.PlaySound(SoundType.TutorialPop);

            yield return GrowBubble();
        }

        private IEnumerator TypePage(string page, SignOptions options)
        {
            SetPage(page);
            PlaceBubble();

            if (stepMouseIcon != null)
            {
                stepMouseIcon.SetActive(options != null && options.showMouseIcon);
            }

            ShowObjects(options);

            if (bubbleHiddenBySwap)
            {
                SetBubbleProgress(0f);

                yield return GrowBubble();

                bubbleHiddenBySwap = false;
            }

            SetBubbleProgress(1f);

            messageText.ForceMeshUpdate();

            int total = messageText.textInfo.characterCount;
            float shown = 0f;
            Func<bool> advanceCondition = null;

            if (options != null)
            {
                advanceCondition = options.advanceCondition;
            }

            messageText.maxVisibleCharacters = 0;
            isTyping = total > 0;

            yield return null;

            while (true)
            {
                bool wasTyping = shown < total;

                if (wasTyping)
                {
                    int previouslyVisible = messageText.maxVisibleCharacters;

                    shown += Mathf.Max(charactersPerSecond, 1f) * Time.unscaledDeltaTime;
                    messageText.maxVisibleCharacters = Mathf.Min(Mathf.FloorToInt(shown), total);

                    if (messageText.maxVisibleCharacters > previouslyVisible)
                    {
                        SoundManager.Instance?.PlaySound(SoundType.TutorialType);
                    }
                    isTyping = shown < total;
                }

                if (WasClicked())
                {
                    bool shouldAdvance = !wasTyping;

                    if (advanceCondition != null)
                    {
                        shouldAdvance = advanceCondition();
                    }

                    if (shouldAdvance)
                    {
                        break;
                    }

                    shown = total;
                    messageText.maxVisibleCharacters = total;
                    isTyping = false;
                }

                yield return null;
            }

            isTyping = false;
        }

        private IEnumerator ClosePanelAfterGrace()
        {
            if (releaseOnClose)
            {
                ReleaseWorldControl();
            }

            for (int i = 0; i < ChainGraceFrames; i++)
            {
                yield return null;
            }

            isClosing = true;
            pendingClose = null;
            isTyping = false;

            if (stepMouseIcon != null)
            {
                stepMouseIcon.SetActive(false);
            }

            yield return ShrinkBubble();

            SetShownObjects(null);

            IsScreenCovered = false;

            yield return FadeDim(dimTargetAlpha, 0f);

            panel.SetActive(false);
            IsShowing = false;
            isClosing = false;

            ReleaseWorldControl();
        }

        private void ShowObjects(SignOptions options)
        {
            GameObject[] objects = null;

            if (options != null)
            {
                objects = options.showObjects;
            }

            SetShownObjects(objects);
        }

        private void SetShownObjects(GameObject[] objects)
        {
            for (int i = shownObjects.Count - 1; i >= 0; i--)
            {
                if (objects == null || Array.IndexOf(objects, shownObjects[i]) < 0)
                {
                    SetObjectShown(shownObjects[i], false);
                    shownObjects.RemoveAt(i);
                }
            }

            if (objects == null)
            {
                return;
            }

            foreach (GameObject target in objects)
            {
                if (!shownObjects.Contains(target))
                {
                    SetObjectShown(target, true);
                    shownObjects.Add(target);
                }
            }
        }

        private static void SetObjectShown(GameObject target, bool shown)
        {
            UIPanelPop pop = target.GetComponent<UIPanelPop>();

            if (pop == null)
            {
                target.SetActive(shown);
            }
            else if (shown)
            {
                pop.Show();
            }
            else
            {
                pop.Hide();
            }
        }

        private void ReleaseWorldControl()
        {
            IsScreenCovered = false;
            IsAnySignShowing = false;
            WorldTime.SetTutorialFrozen(false);
        }

        private IEnumerator SwapSpotlight(SignOptions options)
        {
            bool wantsSpotlight = spotlightImage != null && options != null && options.highlightRects != null && options.highlightRects.Count > 0;

            if (!useSpotlight && !wantsSpotlight)
            {
                ConfigureSpotlight(options);
                yield break;
            }

            isTyping = false;

            if (useSpotlight)
            {
                yield return AnimateFocus(1f, 0f);
            }
            else
            {
                yield return AnimateBubble(1f, 0f);
            }

            bubbleHiddenBySwap = true;
            holeSwapScale = 0f;

            ConfigureSpotlight(options);

            if (useSpotlight)
            {
                yield return AnimateFocus(0f, 1f);
            }
            else
            {
                holeSwapScale = 1f;
                ApplyDimAlpha();
            }
        }

        private IEnumerator AnimateFocus(float from, float to)
        {
            yield return Animate(focusSwapDuration, from, to, value =>
            {
                holeSwapScale = value;
                ApplyDimAlpha();

                if (to < from)
                {
                    SetBubbleProgress(value);
                }
            });
        }

        private IEnumerator AnimateBubble(float from, float to)
        {
            yield return Animate(focusSwapDuration, from, to, SetBubbleProgress);
        }

        private IEnumerator FadeDim(float from, float to)
        {
            yield return Animate(fadeDuration, from, to, SetDimAlpha);
        }

        private IEnumerator GrowBubble()
        {
            yield return Animate(scaleDuration, 0f, 1f, SetBubbleProgress);
        }

        private IEnumerator ShrinkBubble()
        {
            yield return Animate(scaleDuration, 1f, 0f, SetBubbleProgress);
        }

        private IEnumerator Animate(float duration, float from, float to, Action<float> apply)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(elapsed / Mathf.Max(duration, 0.01f));

                apply(Mathf.Lerp(from, to, t * t * (3f - 2f * t)));

                yield return null;
            }

            apply(to);
        }

        private void ConfigureSpotlight(SignOptions options)
        {
            IList<Rect> screenRects = null;

            if (options != null)
            {
                screenRects = options.highlightRects;
            }

            useSpotlight = spotlightImage != null && screenRects != null && screenRects.Count > 0;

            if (spotlightImage != null)
            {
                spotlightImage.gameObject.SetActive(useSpotlight);
            }

            if (useSpotlight)
            {
                spotlightMaterial.SetColor(HoleTintId, options.holeTint);
                BuildHoles(screenRects);
            }
            else
            {
                bubble.localPosition = bubbleBaseLocalPosition;
            }

            ApplyDimAlpha();
        }

        private void BuildHoles(IList<Rect> screenRects)
        {
            RectTransform spotlightRect = spotlightImage.rectTransform;
            Camera canvasCamera = spotlightImage.canvas.rootCanvas.worldCamera;
            Vector2 screenMin = RectTransformUtility.WorldToScreenPoint(canvasCamera, spotlightRect.TransformPoint(spotlightRect.rect.min));
            Vector2 screenMax = RectTransformUtility.WorldToScreenPoint(canvasCamera, spotlightRect.TransformPoint(spotlightRect.rect.max));
            Vector2 pixelSize = new Vector2(Mathf.Max(Mathf.Abs(screenMax.x - screenMin.x), 1f), Mathf.Max(Mathf.Abs(screenMax.y - screenMin.y), 1f));

            int holeCount = Mathf.Min(screenRects.Count, MaxHoles);
            Vector2 unionMin = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 unionMax = new Vector2(float.MinValue, float.MinValue);

            for (int i = 0; i < holeCount; i++)
            {
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
            Rect bounds = panelRect.rect;

            if (!useSpotlight || bounds.width <= 0f || bounds.height <= 0f)
            {
                return;
            }

            float halfWidth = Mathf.Abs(bubble.rect.width * bubbleBaseScale.x * currentFit) * 0.5f / bounds.width;
            float halfHeight = Mathf.Abs(bubble.rect.height * bubbleBaseScale.y * currentFit) * 0.5f / bounds.height;
            float y = spotlightUnion.yMin - BubbleMargin - halfHeight;

            if (1f - spotlightUnion.yMax >= spotlightUnion.yMin)
            {
                y = spotlightUnion.yMax + BubbleMargin + halfHeight;
            }

            float x = Mathf.Clamp(spotlightUnion.center.x, halfWidth, 1f - halfWidth);

            y = Mathf.Clamp(y, halfHeight, 1f - halfHeight);
            bubble.localPosition = new Vector3(bounds.xMin + x * bounds.width, bounds.yMin + y * bounds.height, bubble.localPosition.z);
        }

        private void SetDimAlpha(float alpha)
        {
            currentDimAlpha = alpha;

            ApplyDimAlpha();
        }

        private void ApplyDimAlpha()
        {
            float spotlightFade = currentDimAlpha / Mathf.Max(dimTargetAlpha, 0.0001f);
            Color color = dimBackground.color;

            color.a = currentDimAlpha;

            if (useSpotlight)
            {
                color.a = 0f;
                spotlightMaterial.SetFloat(FadeId, spotlightFade);
                spotlightMaterial.SetFloat(HoleScaleId, spotlightFade * holeSwapScale);
            }

            dimBackground.color = color;
        }

        private void SetBubbleProgress(float progress)
        {
            float visibility = 1f;

            if (hideBubble)
            {
                visibility = 0f;
            }

            bubble.localScale = bubbleBaseScale * (currentFit * progress * visibility);
        }

        private void SetPage(string text)
        {
            messageText.text = text;
            messageText.maxVisibleCharacters = 0;

            ApplyFit(ComputeFit(text));
        }

        private void ApplyFit(float fit)
        {
            currentFit = fit;

            messageText.rectTransform.sizeDelta = textBaseSize * fit;
            messageText.rectTransform.localScale = textBaseScale / fit;
        }

        private float ComputeFit(string text)
        {
            float low = Mathf.Clamp(minFit, 0.05f, 1f);

            if (TextFits(text, low))
            {
                return low;
            }

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
            Vector2 size = textBaseSize * fit;

            return messageText.GetPreferredValues(text, size.x, size.y).y <= size.y;
        }

        private bool WasClicked()
        {
            bool isMenuPaused = WorldTime.IsMenuPaused;
            bool wasPausedLastCheck = wasMenuPaused;

            wasMenuPaused = isMenuPaused;

            return !isMenuPaused && !wasPausedLastCheck && Input.GetMouseButtonDown(0);
        }
    }
}
