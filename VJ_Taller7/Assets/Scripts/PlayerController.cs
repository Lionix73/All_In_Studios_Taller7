using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float slideDuration = 3f;
    public float crouchHeight = 0.9f;
    public float normalHeight = 1.8f;

    [Header("Jump Settings")]
    public float jumpForce = 5f;
    public int maxJumps = 2;

    [Header("Animator")]
    public Animator animator;

    [Header("Movimiento y Dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    private bool isDashing = false;
    private bool canDash = true;
    private Vector3 dashDirection;

    [Header("Components")]
    public Rigidbody rb;
    public CapsuleCollider playerCollider;
    public Transform cameraTransform;
    [System.Obsolete] public CinemachineCamera freeLookCamera;

    [Header("Camera Settings")]
    [SerializeField] private float currentFOV = 65;
    [SerializeField] private float aimFOV = 55;
    [SerializeField] private float tFOV = 1;
    [SerializeField] private float rotationSpeed = 10f;
    private bool wasAiming;

    private Vector3 previousCameraForward;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float aimInput;
    private bool isRunning = false;
    private bool isCrouching = false;
    private bool isSliding = false;
    private bool canSlide = true;
    private bool canCrouch = true;

    private float speedX;
    private float speedY;

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
        //Slide();
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 desiredMoveDirection = (transform.right * moveDirection.x + transform.forward * moveDirection.z);

        //float lookOrbitXValue = freeLookCamera.Controllers[0].InputValue;
        Quaternion targetRotation = Quaternion.LookRotation(cameraForward);

        float speed = isRunning ? runSpeed : walkSpeed;

        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            if (moveInput.y > 0.05f && moveInput.x < 0.05f && moveInput.x > -0.05f || moveInput.y < -0.05f && moveInput.x < 0.05f && moveInput.x > -0.05f)
            {
                cameraTransform.SetParent(null);
            }
            else
            {
                //cameraTransform.SetParent(this.transform);
            }

            rb.MovePosition(rb.position + desiredMoveDirection * speed * Time.deltaTime);
        }
        else
        {
            isRunning = false;
        }

        if(aimInput > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void HandleAnimations()
    {
        bool isMoving = moveInput.sqrMagnitude > 0.1f;
        speedX = moveInput.x;
        speedY = moveInput.y;

        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isCrouching", isCrouching);
        animator.SetBool("isSliding", isSliding);

        animator.SetFloat("SpeedX", speedX);
        animator.SetFloat("SpeedY", speedY);
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
        if (aimInput > 0.1f)
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
            isCrouching = false;
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed && canCrouch)
        {
            isCrouching = !isCrouching;
            canCrouch = false;

            if(isRunning && canSlide && !isSliding)
            {
                OnSlide();
                StartCoroutine(Slide());

            }
            else if(isCrouching && !isRunning)
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

    private IEnumerator Slide()
    {
        if (isSliding)
        {
            float slideTimer = 0f;

            while (slideTimer < slideDuration)
            {
                slideTimer += Time.deltaTime;
                yield return null;
            }

            isSliding = false;
            isCrouching = false;
            yield return new WaitForSeconds(dashCooldown);

            canSlide = true;
            ResetCrouchFlag();
            //ResetColliderHeight();
        }
    }

    public void OnSlide()
    {
        isSliding = true;
        canSlide = false;
        animator.applyRootMotion = true;
        animator.SetTrigger("slide");
    }

    private void ResetCrouchFlag()
    {
        canCrouch = true;
    }

    public void OnDash()
    {
        if (!canDash || isCrouching || isSliding || moveInput.magnitude < 0.05f) return;

        canDash = false;
        isDashing = true;

        Vector3 movDir = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        Vector3 moveDirection = (transform.right * movDir.x + transform.forward * movDir.z);

        dashDirection = moveDirection;

        StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        float dashTime = 0f;

        while (dashTime < dashDuration)
        {
            rb.MovePosition(rb.position + dashDirection * dashSpeed * Time.deltaTime);
            dashTime += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
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