using UnityEngine;
using System;

public class CookingArrow : MonoBehaviour
{
    private Vector3 targetPosition;
    private Transform wokCenterHitbox;
    private float moveSpeed;

    private float waitTime = 1f;
    private float currentWait = 0f;

    private bool isWaiting = false;
    private bool isResolved = false;

    public Action onHit;
    public Action onMiss;

    public void Initialize(Vector3 target, float speed, Transform wokCenter)
    {
        targetPosition = target;
        moveSpeed = speed;
        wokCenterHitbox = wokCenter;
    }

    private void Update()
    {
        if (isResolved) return;

        Vector3 arrowFlatPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 targetFlatPos = new Vector3(targetPosition.x, 0, targetPosition.z);
        Vector3 wokFlatPos = new Vector3(wokCenterHitbox.position.x, 0, wokCenterHitbox.position.z);

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