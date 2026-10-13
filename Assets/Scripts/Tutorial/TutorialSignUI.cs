using System.Collections;
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

        private Image dimBackground;
        private Transform bubble;
        private float dimTargetAlpha = 1f;
        private Vector3 bubbleBaseScale = Vector3.one;
        private Vector3 textBaseScale = Vector3.one;
        private Vector2 textBaseSize;
        private float currentFit = 1f;
        private bool wasMenuPaused;

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

                if (panel.transform.childCount > 0)
                {
                    bubble = panel.transform.GetChild(0);
                }

                if (dimBackground != null)
                {
                    dimTargetAlpha = dimBackground.color.a;
                }

                if (bubble != null)
                {
                    bubbleBaseScale = bubble.localScale;
                }

                if (messageText != null)
                {
                    textBaseScale = messageText.rectTransform.localScale;
                    textBaseSize = messageText.rectTransform.sizeDelta;
                }

                panel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            IsAnySignShowing = false;
            IsScreenCovered = false;
            WorldTime.SetTutorialFrozen(false);
        }

        public IEnumerator ShowPages(string[] pages)
        {
            if (pages == null || pages.Length == 0)
            {
                yield break;
            }

            IsShowing = true;
            IsAnySignShowing = true;
            wasMenuPaused = WorldTime.IsMenuPaused;
            WorldTime.SetTutorialFrozen(true);

            SetDimAlpha(0f);
            SetBubbleProgress(0f);
            panel.SetActive(true);
            SetPage(pages[0]);
            SetBubbleProgress(0f);

            yield return FadeDim(0f, dimTargetAlpha);

            IsScreenCovered = true;

            yield return GrowBubble();

            for (int i = 0; i < pages.Length; i++)
            {
                SetPage(pages[i]);
                SetBubbleProgress(1f);

                yield return null;

                while (!WasClicked())
                {
                    yield return null;
                }
            }

            yield return ShrinkBubble();

            IsScreenCovered = false;

            yield return FadeDim(dimTargetAlpha, 0f);

            panel.SetActive(false);
            IsShowing = false;
            IsAnySignShowing = false;
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

        private void SetDimAlpha(float alpha)
        {
            if (dimBackground == null)
            {
                return;
            }

            Color color = dimBackground.color;
            color.a = alpha;
            dimBackground.color = color;
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
