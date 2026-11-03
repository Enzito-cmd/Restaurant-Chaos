using System.Collections;
using UnityEngine;

public class UIPanelPop : MonoBehaviour
{
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private bool animateScale = true;
    [SerializeField] private CanvasGroup fade;

    private Vector3 baseScale;
    private bool hasBaseScale;
    private float current;
    private Coroutine routine;

    public bool IsOpen { get; private set; }

    private void OnEnable()
    {
        if (!IsOpen)
        {
            HideInstantly();
        }
    }

    public static float EaseOutBack(float t)
    {
        t -= 1f;
        return 1f + t * t * (4f * t + 3f);
    }

    public void Show()
    {
        if (IsOpen) return;

        CaptureScale();

        IsOpen = true;
        gameObject.SetActive(true);

        if (!isActiveAndEnabled)
        {
            Apply(1f);
            return;
        }

        Play(1f);
    }

    public void Hide()
    {
        if (!IsOpen) return;

        IsOpen = false;

        if (!isActiveAndEnabled)
        {
            HideInstantly();
            return;
        }

        Play(0f);
    }

    public void HideInstantly()
    {
        CaptureScale();

        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        IsOpen = false;
        Apply(0f);
        gameObject.SetActive(false);
    }

    private void CaptureScale()
    {
        if (hasBaseScale) return;

        baseScale = transform.localScale;
        hasBaseScale = true;
    }

    private void Play(float target)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
        }

        routine = StartCoroutine(Animate(current, target));
    }

    private IEnumerator Animate(float from, float to)
    {
        bool bounce = to > from && animateScale;

        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duration)
        {
            float eased = t * t * (3f - 2f * t);

            if (bounce)
            {
                eased = EaseOutBack(t);
            }

            Apply(Mathf.LerpUnclamped(from, to, eased));
            yield return null;
        }

        Apply(to);
        routine = null;

        if (!IsOpen)
        {
            gameObject.SetActive(false);
        }
    }

    private void Apply(float value)
    {
        current = value;

        if (animateScale)
        {
            transform.localScale = baseScale * value;
        }

        if (fade != null)
        {
            fade.alpha = Mathf.Clamp01(value);
        }
    }
}
