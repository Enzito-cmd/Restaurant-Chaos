using System.Collections;
using UnityEngine;

namespace RestaurantChaos.Tutorial
{
    public class KeyPatternDemo : MonoBehaviour
    {
        [SerializeField] private CanvasGroup[] patterns;
        [SerializeField] private float holdTime = 1.2f;
        [SerializeField] private float fadeTime = 0.18f;

        private void OnEnable()
        {
            foreach (CanvasGroup pattern in patterns)
            {
                pattern.alpha = 0f;
            }

            StartCoroutine(Cycle());
        }

        private IEnumerator Cycle()
        {
            int index = 0;

            while (true)
            {
                yield return Fade(patterns[index], 0f, 1f);
                yield return new WaitForSecondsRealtime(holdTime);
                yield return Fade(patterns[index], 1f, 0f);

                index = (index + 1) % patterns.Length;
            }
        }

        private IEnumerator Fade(CanvasGroup target, float from, float to)
        {
            float elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.unscaledDeltaTime;
                target.alpha = Mathf.Lerp(from, to, elapsed / fadeTime);

                yield return null;
            }

            target.alpha = to;
        }
    }
}
