using System;
using System.Collections;
using UnityEngine;

public static class MinigameMotion
{
    public static IEnumerator Slide(Transform target, Vector3 to, float duration)
    {
        Vector3 from = target.position;

        for (float t = 0f; t < 1f; t += Time.deltaTime / duration)
        {
            target.position = Vector3.Lerp(from, to, t);

            yield return null;
        }

        target.position = to;
    }

    public static IEnumerator Move(Transform target, Vector3 to, float duration)
    {
        Vector3 from = target.position;

        for (float t = 0f; t < 1f; t += Time.deltaTime / duration)
        {
            target.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));

            yield return null;
        }

        target.position = to;
    }

    public static IEnumerator Arc(Transform target, Func<Vector3> destination, float duration, float height)
    {
        Vector3 from = target.position;

        for (float t = 0f; t < 1f; t += Time.deltaTime / duration)
        {
            target.position = Vector3.Lerp(from, destination(), t) + Vector3.up * (height * 4f * t * (1f - t));

            yield return null;
        }

        target.position = destination();
    }
}
