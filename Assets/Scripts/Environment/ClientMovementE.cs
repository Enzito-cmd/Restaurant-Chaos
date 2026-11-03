using UnityEngine;
using System;

public class ClientMovementE : MonoBehaviour
{
    private Transform target;
    private Action onArrived;

    [SerializeField] private float speed = 2f;

    private bool isGrounded;

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void SetTarget(Transform newTarget, Action callback)
    {
        target = newTarget;
        onArrived = callback;
    }

    private void Update()
    {
        if (target == null)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        Vector3 direction =
            (target.position - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            transform.forward = direction;
        }

        if (Vector3.Distance(
            transform.position,
            target.position
        ) < 0.2f)
        {
            target = null;

            onArrived?.Invoke();
            onArrived = null;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            Debug.Log("TOCANDO SUELO");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
            Debug.Log("NO TOCA SUELO");
        }
    }
}