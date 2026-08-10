using System.Collections;
using UnityEngine;

public class WokVisuals : MonoBehaviour
{
    [Header("Pan References")]
    [SerializeField] private Transform panTransform;
    [SerializeField] private Transform panStartPoint;
    [SerializeField] private Transform panCookingPoint;

    [Header("Animation Settings")]
    [SerializeField] private float moveDuration = 0.5f;

    private void Awake()
    {
        if (panTransform != null && panStartPoint != null)
        {
            panTransform.position = panStartPoint.position;
        }
    }

    public IEnumerator AnimatePanIn()
    {
        if (panTransform == null || panStartPoint == null || panCookingPoint == null) yield break;

        panTransform.position = panStartPoint.position;

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / moveDuration);

            panTransform.position = Vector3.Lerp(panStartPoint.position, panCookingPoint.position, t);

            yield return null;
        }

        panTransform.position = panCookingPoint.position;
    }

    public IEnumerator AnimatePanOut()
    {
        if (panTransform == null || panStartPoint == null || panCookingPoint == null) yield break;

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / moveDuration);

            panTransform.position = Vector3.Lerp(panCookingPoint.position, panStartPoint.position, t);

            yield return null;
        }

        panTransform.position = panStartPoint.position;
    }
}