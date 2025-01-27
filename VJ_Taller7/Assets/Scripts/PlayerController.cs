using System.Collections;
using System.Linq;
using TreeEditor;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Splines.Interpolators;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float slideSpeed = 10f;
    public float slideDistance = 5f;
    public float crouchHeight = 0.9f;
    public float normalHeight = 1.8f;

    [Header("Jump Settings")]
    public float jumpForce = 5f;
    public int maxJumps = 2;

    [Header("Animator")]
    public Animator animator;

    [Header("Components")]
    public Rigidbody rb;
    public CapsuleCollider playerCollider;
    public Transform cameraTransform;
    [System.Obsolete] public CinemachineCamera freeLookCamera;

    [Header("Camera Settings")]
    [SerializeField] private float currentFOV = 65;
    [SerializeField] private float aimFOV = 55;
    [SerializeField] private float tFOV = 1;
    [SerializeField] private float rotationSpeed = 10f; // Velocidad de rotación
    private bool wasAiming;

    private Vector3 previousCameraForward; // Dirección previa de la cámara
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float aimInput;
    private bool isRunning = false;
    private bool isCrouching = false;
    private bool isSliding = false;
    private bool canSlide = true;
    private bool canCrouch = true;

    private bool forward = true;
    private bool backward = true;
    private bool right = true;
    private bool left = true;

    private int jumpCount = 0;

    private Vector3 slideDirection;
    private float slideTimer = 0f;

    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        previousCameraForward = cameraTransform.forward;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandleMovement();
        HandleAnimations();
        adjustFOV();
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        Vector3 worldInputDir = transform.TransformDirection(moveDirection);

        MovDirection(moveDirection);

        if (moveDirection != Vector3.zero)
        {
            isCrouching = false;

            // Obtener la direccion orientada segun la camara
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            // Asegurarnos de que los vectores esten en el plano horizontal (sin componente Y)
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 desiredMoveDirection = (cameraRight * moveDirection.x + cameraForward * moveDirection.z);

            Vector3 characterMoveDir = (Vector3.right * moveDirection.x + Vector3.forward * moveDirection.z);

            //float lookOrbitXValue = freeLookCamera.Controllers[0].InputValue;
            Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDirection);

            float speed = isRunning ? runSpeed : walkSpeed;

            if (forward == true && right == false && left == false || aimInput == 1)
            {
                cameraTransform.SetParent(null);
                rb.MovePosition(rb.position + desiredMoveDirection * speed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
            else
            {
                cameraTransform.SetParent(this.transform);
                rb.MovePosition(rb.position + characterMoveDir * speed * Time.deltaTime);
            }

        }
        else
        {
            isRunning = false;
        }
    }

    private void MovDirection(Vector3 movDir)
    {
        if (movDir.z > 0.05f)
        {
            forward = true;
            backward = false;
        }
        else if (movDir.z < -0.05f)
        {
            forward = false;
            backward = true;
        }
        else
        {
            forward = false;
            backward = false;
        }


        if (movDir.x > 0.05f)
        {
            right = true;
            left = false;
        }
        else if (movDir.x < -0.05f)
        {
            right = false;
            left = true;
        }
        else
        {
            right = false;
            left = false;
        }
    }

    private void Slide()
    {
        if (isSliding)
        {
            slideTimer += Time.deltaTime * slideSpeed;
            rb.linearVelocity = slideDirection * slideSpeed;

            if (slideTimer >= slideDistance)
            {
                isSliding = false;
                isCrouching = false;
                canSlide = true;
                slideTimer = 0f;
                ResetColliderHeight();
            }
            return;
        }
    }

    private void HandleAnimations()
    {
        bool isMoving = moveInput.sqrMagnitude > 0.1f;

        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isCrouching", isCrouching);
        animator.SetBool("isSliding", isSliding);
        animator.SetBool("Forward", forward);
        animator.SetBool("Backward", backward);
        animator.SetBool("Left", left);
        animator.SetBool("Right", right);
    }

    private void ResetColliderHeight()
    {
        playerCollider.height = normalHeight;
        playerCollider.center.Set(0f, 0.9f, 0f);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        aimInput = context.ReadValue<float>();
    }

    public void adjustFOV()
    {
        if (aimInput == 1)
        {
            freeLookCamera.Lens.FieldOfView = Mathf.Lerp(aimFOV, currentFOV, tFOV * Time.deltaTime);
            wasAiming = true;
            isRunning = false;
        }
        else if (aimInput == 0 && wasAiming == true)
        {
            freeLookCamera.Lens.FieldOfView = Mathf.Lerp(currentFOV, aimFOV, tFOV * Time.deltaTime);
            wasAiming = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && jumpCount < maxJumps)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z); // Reset Y velocity
            rb.AddForce(rb.linearVelocity * jumpForce + Vector3.up * 5, ForceMode.Impulse);
            jumpCount++;
            animator.SetBool("jump", true);
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isRunning = !isRunning;
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed && canCrouch)
        {
            isCrouching = !isCrouching;
            canCrouch = false;

            if (isCrouching)
            {
                animator.SetTrigger("crouch");
                //playerCollider.height = crouchHeight;
                //playerCollider.center.Set(0f, 0.45f, 0f);
            }
            else
            {
                //ResetColliderHeight();
                //animator.SetTrigger("uncrouch");
            }

            Invoke(nameof(ResetCrouchFlag), 0.5f); // Adjust to match animation duration
        }
    }

    public void OnSlide(InputAction.CallbackContext context)
    {
        Debug.Log("Slide detectado");

        if (context.performed && isRunning && canSlide)
        {
            isSliding = true;
            canSlide = false;
            slideDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
            animator.SetTrigger("slide");
        }
    }

    private void ResetCrouchFlag()
    {
        canCrouch = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            jumpCount = 0;
            animator.SetBool("jump", false);
        }
    }
}