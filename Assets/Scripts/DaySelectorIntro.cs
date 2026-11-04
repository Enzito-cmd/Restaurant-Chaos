using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DaySelectorIntro : MonoBehaviour
{
    [Header("Book")]
    [SerializeField] private Image bookImage;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float framesPerSecond = 24f;

    [Header("Buttons")]
    [SerializeField] private CanvasGroup buttonsGroup;
    [SerializeField] private RectTransform[] popTargets;
    [SerializeField] private float popDuration = 0.35f;

    private IEnumerator Start()
    {
        buttonsGroup.interactable = false;

        foreach (RectTransform target in popTargets)
        {
            target.localScale = Vector3.zero;
        }

        if (frames.Length > 0) bookImage.sprite = frames[0];

        yield return new WaitWhile(() => SceneTransition.IsRunning);

        SoundManager.Instance?.PlaySound(SoundType.BookOpen);

        foreach (Sprite frame in frames)
        {
            bookImage.sprite = frame;

            yield return new WaitForSecondsRealtime(1f / framesPerSecond);
        }

        yield return PopTargets();

        buttonsGroup.interactable = true;
    }

    private IEnumerator PopTargets()
    {
        float elapsed = 0f;

        while (elapsed < popDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float scale = Mathf.SmoothStep(0f, 1f, elapsed / popDuration);

            foreach (RectTransform target in popTargets)
            {
                target.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        foreach (RectTransform target in popTargets)
        {
            target.localScale = Vector3.one;
        }
    }
}
