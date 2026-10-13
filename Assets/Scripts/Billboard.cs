using UnityEngine;

[DefaultExecutionOrder(1000)]
public class Billboard : MonoBehaviour
{
    private const float MaxScaleRatio = 1.01f;

    private Camera targetCamera;
    private Transform anchor;
    private Vector3 anchorLocalPosition;
    private bool isDetached;

    public static void Attach(GameObject target)
    {
        if (target == null) return;
        if (target.GetComponent<Billboard>() != null) return;

        target.AddComponent<Billboard>();
    }

    private void Start()
    {
        if (transform.parent == null) return;
        if (!HasDeformedParent()) return;

        DetachFromParent();
    }

    private void LateUpdate()
    {
        if (isDetached)
        {
            if (anchor == null)
            {
                Destroy(gameObject);
                return;
            }

            transform.position = anchor.TransformPoint(anchorLocalPosition);
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null) return;

        transform.rotation = targetCamera.transform.rotation;
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
