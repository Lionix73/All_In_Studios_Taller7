using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;
using static UnityEditor.Experimental.GraphView.GraphView;

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
    [SerializeField] private FloatDampener layersDampener;
    private FloatDampener layerDesactivateDamp;

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

    [Header("Components")]
    public Rigidbody rb;
    public CapsuleCollider playerCollider;
    public CapsuleCollider crouchCollider;
    public Transform cameraTransform;
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
    [SerializeField] private MultiAimConstraint aimRig;
    [SerializeField] private TwoBoneIKConstraint gripRig;
    private bool isRunning = false;
    private bool isCrouching = false;
    private bool isSliding = false;
    private bool canSlide = true;
    private bool canCrouch = true;
    private bool canJump = true;
    private bool isEmoting = false;
    private bool usingRifle = true;

    private bool isGrounded;
    private int jumpCount = 0;

    private bool isDashing = false;
    private bool canDash = true;
    private Vector3 dashDirection;

    private Vector3 slideDirection;
    private Vector3 desiredMoveDirection;

    private PlayerInput playerInput;
    private GunManager gunManager;
    private SoundManager soundManager;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        gunManager = FindAnyObjectByType<GunManager>();
        soundManager = FindAnyObjectByType<SoundManager>();
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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            if (Cursor.visible)
                InputSystem.PauseHaptics();
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                InputSystem.ResumeHaptics();
            }
        }

        AdjustRigs();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();

        CheckGround();
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;

        desiredMoveDirection = (transform.right * moveDirection.x + transform.forward * moveDirection.z);

        float speed = isRunning ? runSpeed : walkSpeed;

        if (moveDirection != Vector3.zero)
        {
            rb.MovePosition(rb.position + desiredMoveDirection * speed * Time.deltaTime);
            isEmoting = false;
            soundManager.StopSound("GangamStyle");

            if (isRunning && isGrounded)
            {
                soundManager.PlaySound("Run");
                soundManager.StopSound("Walk");
            }
            else if(isGrounded)
            {
                soundManager.PlaySound("Walk");
                soundManager.StopSound("Run");
            }
            else
            {
                soundManager.StopSound("Walk", "Run");
            }
        }
        else
        {
            isRunning = false;
            soundManager.StopSound("Walk", "Run");
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
        if (moveInput.magnitude > 0.05f || aimInput > 0.05f)
        {
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleAnimations()
    {
        speedX.Update();
        speedY.Update();
        layersDampener.Update();
        layerDesactivateDamp.Update();
        ChangeAnimLayer();

        bool isMoving = moveInput.sqrMagnitude > 0.1f;
        speedX.TargetValue = moveInput.x;
        speedY.TargetValue = moveInput.y;

        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isCrouching", isCrouching);
        animator.SetBool("isSliding", isSliding);
        animator.SetBool("isEmoting", isEmoting);
        animator.SetBool("usingRifle", usingRifle);

        animator.SetFloat("SpeedX", speedX.CurrentValue);
        animator.SetFloat("SpeedY", speedY.CurrentValue);
    }

    private void ChangeAnimLayer()
    {
        int layersAmount = animator.layerCount;
        layersDampener.TargetValue = 1;
        layerDesactivateDamp.TargetValue = 0;

        for (int i = 1; i < layersAmount; i++)
        {
            if (animator.GetLayerWeight(i) > 0)
            {
                animator.SetLayerWeight(i, layerDesactivateDamp.CurrentValue);
            }
        }

        if (aimInput > 0.1f)
        {
            animator.SetLayerWeight(4, layersDampener.CurrentValue);
            return;
        }

        if (gunManager.Gun == GunType.BasicPistol || gunManager.Gun == GunType.Revolver)
        {
            if(isRunning)
            {
                animator.SetLayerWeight(3, layersDampener.CurrentValue);
            }
            else
            {
                animator.SetLayerWeight(1, layersDampener.CurrentValue);
            }
        }
        else
        {
            if (isRunning)
            {
                animator.SetLayerWeight(2, layersDampener.CurrentValue);
            }
        }
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

    public void OnChangeGun(InputAction.CallbackContext context)
    {
        if(context.performed) 
        {
            animator.SetTrigger("ChangeGun");
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            animator.SetTrigger("Reload");
        }
    }

    public void OnEmote(InputAction.CallbackContext context)
    {
        if(context.performed && moveInput.magnitude < 0.05f)
        {
            if(!isEmoting) 
            { 
                soundManager.PlaySound("GangamStyle"); 
            }
            else 
            { 
                soundManager.StopSound("GangamStyle"); 
            }
            
            isEmoting = !isEmoting;
        }
    }

    public void adjustFOV()
    {
        if (aimInput > 0.1f)
        {
            freeLookCamera.Lens.FieldOfView = Mathf.Lerp(aimFOV, currentFOV, tFOV);
            wasAiming = true;
            isRunning = false;
        }
        else if (aimInput == 0 && wasAiming == true)
        {
            freeLookCamera.Lens.FieldOfView = Mathf.Lerp(currentFOV, aimFOV, tFOV);
            wasAiming = false;
        }

    }

    /// <summary>
    /// Ajusta los rigs que se tienen para el agarre del arma
    /// con la otra mano y el punto donde se apunta
    /// </summary>
    private void AdjustRigs(){
        if (aimRig==null || gripRig == null) return;
        else if (wasAiming){
            aimRig.weight=1.0f;
            gripRig.weight = 1.0f;
        }
        else {aimRig.weight=0; gripRig.weight = 0.5f;}

        if (moveInput.magnitude!=0){
            aimRig.weight = 1.0f;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && jumpCount < maxJumps)
        {
            soundManager.StopSound("Walk", "Run");
            soundManager.PlaySound("Jump");

            if(jumpCount == 0)
            {
                rb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
                rb.AddForce(desiredMoveDirection * 1.2f, ForceMode.Impulse);
                
                animator.SetTrigger("Jump");
            }
            else if (jumpCount == 1)
            {
                rb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
                rb.AddForce(desiredMoveDirection * 2, ForceMode.Impulse);

                animator.SetTrigger("DoubleJump");
            }
            jumpCount++;

            if(canJump)
            {
                animator.SetBool("isJumping", true);
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
        animator.SetBool("isJumping", false);
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
        if (context.performed && canCrouch && isGrounded)
        {
            ExchangeColliders();
            isCrouching = !isCrouching;
            canCrouch = false;

            if(isRunning && canSlide && !isSliding)
            {
                soundManager.PlaySound("Slide");
                OnSlide();
                StartCoroutine(Slide());
            }

            Invoke(nameof(ResetCrouchFlag), 0.5f);
        }
    }

    private void ExchangeColliders()
    {
        if(playerCollider != null && crouchCollider != null)
        {
            if(playerCollider.enabled)
            {
                crouchCollider.enabled = true;
                playerCollider.enabled = false;
            }
            else
            {
                playerCollider.enabled = true;
                crouchCollider.enabled = false;
            }
        }
    }

    private IEnumerator Slide()
    {
        if (isSliding)
        {
            float slideTimer = 0f;

            while (slideTimer < slideDuration)
            {
                soundManager.StopSound("Run");
                slideTimer += Time.deltaTime;
                yield return null;
            }

            isSliding = false;
            isCrouching = false;
            yield return new WaitForSeconds(dashCooldown);

            canSlide = true;
            ResetCrouchFlag();
            ExchangeColliders();
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

        soundManager.StopSound("Walk", "Run");
        soundManager.PlaySound("Dash");

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
        yield return new WaitForSeconds(dashDuration);
        rb.useGravity = true;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }


    private void CheckGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 50f))
        {
            if (hit.collider.gameObject.layer == 7 && isGrounded == false && hit.distance < 0.1f)
            {
                isGrounded = true;
                soundManager.StopSound("Falling");
                soundManager.PlaySound("Landing");
            }
            else
            {
                if (hit.distance > 0.05f) isGrounded = false;
            }
        }

        if (!isGrounded && rb.linearVelocity.y < 0 && rb.linearVelocity.y > -0.1f && hit.distance > 3) soundManager.PlaySound("Falling");
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            jumpCount = 0;
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
