using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 12f;

    private CharacterController characterController;
    private Vector3 moveDirection;

    private bool canMove = true;

    private Transform mainCameraTransform;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    public void SetMovement(bool state)
    {
        canMove = state;
    }

    private void Update()
    {
        if (!canMove) return;

        HandleInput();
        HandleMovement();
        HandleRotation();
    }

    private void HandleInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        if (mainCameraTransform != null)
        {
            Vector3 camForward = mainCameraTransform.forward;
            Vector3 camRight = mainCameraTransform.right;

            camForward.y = 0f;
            camRight.y = 0f;

            camForward.Normalize();
            camRight.Normalize();

            moveDirection = (camForward * moveZ + camRight * moveX).normalized;
        }
    }

    private void HandleMovement()
    {
        if (moveDirection.sqrMagnitude < 0.01f) return;

        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void HandleRotation()
    {
        if (moveDirection == Vector3.zero) return;

        Vector3 targetDirection = new Vector3(moveDirection.x, 0f, moveDirection.z);

        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}