using UnityEngine;

[DefaultExecutionOrder(1000)]
public class Billboard : MonoBehaviour
{
    private const float MaxScaleRatio = 1.01f;
    private const float AlwaysVisibleDepth = 2f;
    private const float NearClipMargin = 0.5f;

    private Camera targetCamera;
    private Transform anchor;
    private Vector3 anchorLocalPosition;
    private Vector3 baseLocalPosition;
    private Vector3 baseLocalScale;
    private bool isDetached;
    private bool isInitialized;
    private bool alwaysVisible;

    public static void Attach(GameObject target)
    {
        Attach(target, false);
    }

    public static void AttachAlwaysVisible(GameObject target)
    {
        Attach(target, true);
    }

    private static void Attach(GameObject target, bool visibleOverScenery)
    {
        if (target == null)
        {
            return;
        }

        Billboard existing = target.GetComponent<Billboard>();

        if (existing != null)
        {
            if (visibleOverScenery)
            {
                existing.alwaysVisible = true;
            }

            return;
        }

        Billboard billboard = target.AddComponent<Billboard>();
        billboard.alwaysVisible = visibleOverScenery;
    }

    private void Initialize()
    {
        isInitialized = true;

        if (transform.parent != null && HasDeformedParent())
        {
            DetachFromParent();
        }

        baseLocalPosition = transform.localPosition;
        baseLocalScale = transform.localScale;
    }

    private void LateUpdate()
    {
        if (!isInitialized)
        {
            Initialize();
        }

        if (isDetached && anchor == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 truePosition = GetTruePosition();

        if (isDetached)
        {
            transform.position = truePosition;
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            return;
        }

        transform.rotation = targetCamera.transform.rotation;

        if (alwaysVisible)
        {
            ApplyAlwaysVisible(truePosition);
        }
    }

    private Vector3 GetTruePosition()
    {
        if (isDetached)
        {
            return anchor.TransformPoint(anchorLocalPosition);
        }

        if (!alwaysVisible)
        {
            return transform.position;
        }

        if (transform.parent == null)
        {
            return baseLocalPosition;
        }

        return transform.parent.TransformPoint(baseLocalPosition);
    }

    private void ApplyAlwaysVisible(Vector3 truePosition)
    {
        Transform cameraTransform = targetCamera.transform;
        Vector3 toTarget = truePosition - cameraTransform.position;
        float depth = Vector3.Dot(toTarget, cameraTransform.forward);
        float wantedDepth = Mathf.Max(AlwaysVisibleDepth, targetCamera.nearClipPlane + NearClipMargin);

        if (depth <= wantedDepth)
        {
            transform.position = truePosition;
            transform.localScale = baseLocalScale;
            return;
        }

        if (targetCamera.orthographic)
        {
            transform.position = truePosition - cameraTransform.forward * (depth - wantedDepth);
            transform.localScale = baseLocalScale;
            return;
        }

        float ratio = wantedDepth / depth;

        transform.position = cameraTransform.position + toTarget * ratio;
        transform.localScale = baseLocalScale * ratio;
    }

    private bool HasDeformedParent()
    {
        Vector3 parentScale = transform.parent.lossyScale;

        float smallest = Mathf.Min(Mathf.Abs(parentScale.x), Mathf.Abs(parentScale.y), Mathf.Abs(parentScale.z));
        float largest = Mathf.Max(Mathf.Abs(parentScale.x), Mathf.Abs(parentScale.y), Mathf.Abs(parentScale.z));

        if (smallest <= 0f)
        {
            return true;
        }

        return largest / smallest > MaxScaleRatio;
    }

    private void DetachFromParent()
    {
        anchor = transform.parent;
        anchorLocalPosition = anchor.InverseTransformPoint(transform.position);

        Vector3 worldScale = transform.lossyScale;

        transform.SetParent(null, true);
        transform.localScale = worldScale;

        isDetached = true;
    }
}
