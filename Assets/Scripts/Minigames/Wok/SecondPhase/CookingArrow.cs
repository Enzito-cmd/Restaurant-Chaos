using UnityEngine;
using System;

public class CookingArrow : MonoBehaviour
{
    private Vector3 targetPosition;
    private Transform wokCenterHitbox;
    private Camera wokCamera;
    private float moveSpeed;

    private float waitTime = 1f;
    private float currentWait = 0f;

    private bool isWaiting = false;
    private bool isResolved = false;

    public Action onHit;
    public Action onMiss;

    public void Initialize(Vector3 target, float speed, Transform wokCenter, Camera camera)
    {
        targetPosition = target;
        moveSpeed = speed;
        wokCenterHitbox = wokCenter;
        wokCamera = camera;
    }

    private Vector3 GetWokCenterOnArrowPlane()
    {
        Vector3 center = wokCenterHitbox.position;

        if (wokCamera == null)
        {
            return center;
        }

        Plane arrowPlane = new Plane(Vector3.up, transform.position);
        Ray centerRay = wokCamera.ScreenPointToRay(wokCamera.WorldToScreenPoint(center));

        if (arrowPlane.Raycast(centerRay, out float enter))
        {
            return centerRay.GetPoint(enter);
        }

        return center;
    }

    private void Update()
    {
        if (isResolved) return;

        Vector3 arrowFlatPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 targetFlatPos = new Vector3(targetPosition.x, 0, targetPosition.z);

        if (!isWaiting)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(arrowFlatPos, targetFlatPos) <= 0.01f)
            {
                isWaiting = true;
            }
        }
        else
        {
            currentWait += Time.deltaTime;

            Vector3 wokOnPlane = GetWokCenterOnArrowPlane();
            Vector3 wokFlatPos = new Vector3(wokOnPlane.x, 0, wokOnPlane.z);

            if (Vector3.Distance(arrowFlatPos, wokFlatPos) < 0.5f)
            {
                isResolved = true;
                onHit?.Invoke();
                Destroy(gameObject);
            }
            else if (currentWait >= waitTime)
            {
                isResolved = true;
                onMiss?.Invoke();
                Destroy(gameObject);
            }
        }
    }
}
