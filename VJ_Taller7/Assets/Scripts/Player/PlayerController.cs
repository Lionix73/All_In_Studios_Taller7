using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
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
    [SerializeField] private FloatDampener speedX;
    [SerializeField] private FloatDampener speedY;

    [Header("Movimiento y Dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float jumpDuration = 1.4f;
    [SerializeField] private float jumpCooldown = 1f;

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle;
    [SerializeField] private GameObject stepRayLower;
    private RaycastHit slopeHit;
    
    private bool isDashing = false;
    private bool canDash = true;
    bool rayDash;
    private Vector3 dashDirection;

    [Header("Components")]
    public Rigidbody rb;
    public CapsuleCollider playerCollider;
    public Transform cameraTransform;
    public Transform playerHead;
    [System.Obsolete] public CinemachineCamera freeLookCamera;

    [Header("Camera Settings")]
    [SerializeField] private float currentFOV = 65;
    [SerializeField] private float aimFOV = 55;
    [SerializeField] private float tFOV = 1;
    [SerializeField] private float rotationSpeed = 10f;
    private bool wasAiming;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private float aimInput;
    private bool isRunning = false;
    private bool isCrouching = false;
    private bool isSliding = false;
    private bool canSlide = true;
    private bool canCrouch = true;
    private bool canJump = true;

    private bool isGrounded;
    private int jumpCount = 0;

    private Vector3 slideDirection;
    private Vector3 desiredMoveDirection;
    private float slideTimer = 0f;

    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandleAnimations();
        adjustFOV();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;

        desiredMoveDirection = (transform.right * moveDirection.x + transform.forward * moveDirection.z);

        float speed = isRunning ? runSpeed : walkSpeed;

        if (moveDirection != Vector3.zero)
        {
            rb.MovePosition(rb.position + desiredMoveDirection * speed * Time.deltaTime);
        }
        else
        {
            isRunning = false;
        }

        if(!isSliding)
        {
            rb.useGravity = !OnSlope();
        }

        if (OnSlope())
        {
            rb.AddForce(GetSlopeMovement() * speed * 10f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
    }

    private void HandleRotation()
    {
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void HandleAnimations()
    {
        speedX.Update();
        speedY.Update();

        bool isMoving = moveInput.sqrMagnitude > 0.1f;
        speedX.TargetValue = moveInput.x;
        speedY.TargetValue = moveInput.y;

        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isCrouching", isCrouching);
        animator.SetBool("isSliding", isSliding);

        animator.SetFloat("SpeedX", speedX.CurrentValue);
        animator.SetFloat("SpeedY", speedY.CurrentValue);
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
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.linearVelocity *= 1.2f;
            rb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
            jumpCount++;

            if(canJump)
            {
                animator.SetBool("isJumping", true);
                animator.SetTrigger("Jump");
                canJump = false;

                StartCoroutine(Jump());
            }
        }
    }

    private IEnumerator Jump()
    {
        float jumpTimer = 0f;

        while (jumpTimer < jumpDuration)
        {
            jumpTimer += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
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

        dashDirection = desiredMoveDirection;

        StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        float dashTime = 0f;

        while (dashTime < dashDuration)
        {
            rb.useGravity = false;
            rb.AddForce(dashDirection * dashSpeed, ForceMode.Impulse);
            dashTime += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        rb.useGravity = true;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
            jumpCount = 0;
            animator.SetBool("isJumping", false);
            animator.SetBool("isGrounded", isGrounded);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
            animator.SetBool("isGrounded", isGrounded);
        }
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(stepRayLower.transform.position, Vector3.down, out slopeHit, playerCollider.height * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMovement()
    {
        return Vector3.ProjectOnPlane(desiredMoveDirection, slopeHit.normal).normalized;
    }
}