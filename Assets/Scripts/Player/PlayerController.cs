using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private const string MovingParameter = "IsMoving";

    private CharacterController characterController;
    private Vector3 moveDirection;

    private bool canMove = true;
    private bool drivesAnimator;

    private Transform mainCameraTransform;

    public Vector3 startPosition;
    public Quaternion startRotation;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
        startPosition = transform.position;
        startRotation = transform.rotation;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }
    }

    private void Start()
    {
        drivesAnimator = CanDriveAnimator();
    }

    public void SetMovement(bool state)
    {
        canMove = state;
    }

    private void Update()
    {
        if (!canMove)
        {
            UpdateAnimator(false);
            return;
        }

        HandleInput();
        HandleMovement();
        HandleRotation();
        UpdateAnimator(moveDirection.sqrMagnitude >= 0.01f);
    }

    private bool CanDriveAnimator()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return false;
        }

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == MovingParameter)
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateAnimator(bool isMoving)
    {
        if (drivesAnimator)
        {
            animator.SetBool(MovingParameter, isMoving);
        }
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