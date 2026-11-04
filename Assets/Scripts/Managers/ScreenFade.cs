using System.Collections;
using UnityEngine;

public class ScreenFade : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private float holdBlack = 0.2f;

    private void Awake()
    {
        if (group == null)
        {
            group = GetComponent<CanvasGroup>();
        }

        group.alpha = 0f;
        group.blocksRaycasts = false;
    }

    public IEnumerator Out()
    {
        yield return Fade(0f, 1f);
        yield return new WaitForSecondsRealtime(holdBlack);
    }

    public IEnumerator In()
    {
        yield return Fade(1f, 0f);
    }

    private IEnumerator Fade(float from, float to)
    {
        group.blocksRaycasts = true;

        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duration)
        {
            group.alpha = Mathf.Lerp(from, to, t);

            yield return null;
        }

        group.alpha = to;
        group.blocksRaycasts = to > 0f;
    }
}
