using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace RestaurantChaos.Clients
{
    public class DayTitleUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text titleText;

        [Header("Timing")]
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private float holdDuration = 1.6f;

        [Header("Day Title")]
        [SerializeField] private string dayEnglish = "Day {0}";
        [SerializeField] private string daySpanish = "Día {0}";

        [Header("Tutorial Title")]
        [SerializeField] private string tutorialEnglish = "Tutorial {0}";
        [SerializeField] private string tutorialSpanish = "Tutorial {0}";

        public static bool IsShowing { get; private set; }

        private void Awake()
        {
            IsShowing = false;

            group.alpha = 0f;
            group.blocksRaycasts = false;
        }

        private void OnDestroy()
        {
            IsShowing = false;
        }

        public string GetTitle(bool isTutorial, int number)
        {
            if (isTutorial)
            {
                return string.Format(Localization.Pick(tutorialEnglish, tutorialSpanish), number);
            }

            return string.Format(Localization.Pick(dayEnglish, daySpanish), number);
        }

        public IEnumerator Show(string title, Action onCovered, bool startCovered)
        {
            IsShowing = true;

            return ShowRoutine(title, onCovered, startCovered);
        }

        private IEnumerator ShowRoutine(string title, Action onCovered, bool startCovered)
        {
            titleText.text = title;
            group.blocksRaycasts = true;

            if (startCovered)
            {
                group.alpha = 1f;
            }
            else
            {
                yield return Fade(0f, 1f);
            }

            onCovered?.Invoke();

            yield return new WaitForSecondsRealtime(holdDuration);
            yield return Fade(1f, 0f);

            group.blocksRaycasts = false;
            IsShowing = false;
        }

        private IEnumerator Fade(float from, float to)
        {
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / fadeDuration)
            {
                group.alpha = Mathf.Lerp(from, to, t);

                yield return null;
            }

            group.alpha = to;
        }
    }
}
