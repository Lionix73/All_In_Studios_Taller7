﻿using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CheckTerrainHeight))]
public class PlayerController : MonoBehaviour
{
    #region Visible Variables
    [Header("Movement Settings")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Slide-Crouch")]
    [SerializeField] private float slideDuration = 3f;
    [SerializeField] private float slideCrouchCooldown = 0.5f;

    [Header("Jump")]
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private float jumpVerticalForce = 10f;
    [SerializeField] private float jumpHorizontalForce = 5f;
    [SerializeField] private float jumpDuration = 1.4f;

    [Header("Components")]
    [SerializeField] private CapsuleCollider playerCollider;
    [SerializeField] private CapsuleCollider crouchCollider;

    [Header("VFX")]
    [SerializeField] private ParticleSystem slideVFX;
    [SerializeField] private ParticleSystem jumpVFX;

    [Header("Camera Settings")]
    [SerializeField] private Transform camTarget;
    [SerializeField] private CinemachineCamera freeLookCamera;

    [Header("Rigs")]
    [SerializeField] private MultiAimConstraint aimRig;
    [SerializeField] private TwoBoneIKConstraint gripRig;
    #endregion

    #region Private Floats
    private float aimInput;
    private float speed;
    #endregion

    #region Private Ints
    private int jumpCount = 0;
    #endregion

    #region Private Bools
    private bool wasAiming;
    private bool isRunning;
    private bool isCrouching;
    private bool isSliding;
    private bool isAiming;
    private bool isMoving;
    private bool isJumping;

    private bool canSlide = true;
    private bool canCrouch = true;
    private bool canJump = true;
    private bool canMelee = true;
    #endregion

    #region Private Vectors
    private Vector2 moveInput;
    private Vector3 desiredMoveDirection;
    #endregion

    #region Private Components
    private CheckTerrainHeight checkTerrainHeight;
    private FOVHandler fov;
    private Rigidbody rb;
    private Transform cameraTransform;
    private Melee melee;
    private SensibilitySettings sensibilitySettings;
    #endregion

    #region Events
    public delegate void JumpEvent();
    public event JumpEvent JumpingEvent;

    public delegate void MeleeEvent();
    public event MeleeEvent MeleeAttackEvent;

    public delegate void SlideEvent();
    public event SlideEvent SlidingEvent;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        checkTerrainHeight = GetComponent<CheckTerrainHeight>();
        melee = GetComponentInChildren<Melee>();
        fov = GetComponent<FOVHandler>();
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        freeLookCamera = GameObject.FindGameObjectWithTag("FreeLookCamera").GetComponent<CinemachineCamera>();
        cameraTransform = freeLookCamera.transform;
        sensibilitySettings = freeLookCamera.GetComponent<SensibilitySettings>();
        freeLookCamera.Target.TrackingTarget = camTarget;
    }

    private void Update()
    {
        PlayerAiming();
        AdjustRigs();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleRotation()
    {
        if (!canMove) return;

        if (moveInput.magnitude > 0.05f || aimInput > 0.05f)
        {
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    #region -----Movement-----
    private void HandleMovement()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        isMoving = moveInput.sqrMagnitude > 0.1f;

        desiredMoveDirection = transform.right * moveDirection.x + transform.forward * moveDirection.z;

        speed = isRunning ? runSpeed : walkSpeed;

        if (moveDirection != Vector3.zero)
        {
            rb.MovePosition(rb.position + speed * Time.deltaTime * desiredMoveDirection);
        }
        else
        {
            isRunning = false;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!canMove)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = context.ReadValue<Vector2>();
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isRunning = !isRunning;
            isCrouching = false;
        }
    }

    public void BlockMovement()
    {
        canMove = !canMove;
    }
    #endregion

    #region -----Aim-----
    public void OnAim(InputAction.CallbackContext context)
    {
        aimInput = context.ReadValue<float>();

        if (aimInput > 0.1f)
        {
            isAiming = true;
        }
        else if (aimInput < 0.1f)
        {
            isAiming = false;
        }
    }

    public void PlayerAiming()
    {
        if (!canMove) return;

        if (isAiming)
        {
            if (!wasAiming && sensibilitySettings != null)
                sensibilitySettings.AdjustSensiDuringAim();

            if (fov != null)
                fov.AimFOV();
            
            wasAiming = true;
            isRunning = false;
        }
        else if (!isAiming)
        {
            if (wasAiming && sensibilitySettings != null)
                sensibilitySettings.AdjustSensiNoAim();

            if (fov != null)
                fov.NormalFOV();

            wasAiming = false;
        }
    }
    #endregion

    #region -----Adjust Rigs-----
    /// <summary>
    /// Ajusta los rigs que se tienen para el agarre del arma
    /// con la otra mano y el punto donde se apunta
    /// </summary>
    private void AdjustRigs()
    {
        if (aimRig == null || gripRig == null) return;

        else if (wasAiming)
        {
            aimRig.weight = 1.0f;
            gripRig.weight = 1.0f;
        }
        else
        {
            aimRig.weight = 0;
            gripRig.weight = 0.5f;
        }

        if (moveInput.magnitude != 0)
        {
            aimRig.weight = 1.0f;
        }
    }
    #endregion

    #region -----Jump-----
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!canMove) return;

        if (context.performed && jumpCount < maxJumps)
        {
            JumpingEvent?.Invoke();
            StartCoroutine(Jump());
        }
    }

    private IEnumerator Jump()
    {
        jumpCount++;
        isJumping = true;
        jumpVFX.Play();

        rb.linearVelocity = Vector3.zero;
        Vector3 force = (desiredMoveDirection.normalized * jumpHorizontalForce) + (jumpVerticalForce * Vector3.up);
        rb.AddForce(force, ForceMode.Impulse);

        yield return new WaitForSeconds(jumpDuration);

        isJumping = false;
    }
    #endregion

    #region -----Melee-----
    public void OnMelee(InputAction.CallbackContext context)
    {
        if (!canMove) return;

        if (context.performed && canMelee)
        {
            MeleeAttackEvent?.Invoke();
            StartCoroutine(Melee());
        }
    }

    private IEnumerator Melee()
    {
        canMelee = false;
        melee.ActivateMelee();
        yield return new WaitForSeconds(melee.MeleeWindow);
        canMelee = true;
        melee.DeactivateMelee();
    }
    #endregion

    #region -----Crouch / Slide-----
    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (!canMove) return;

        if (context.performed && canCrouch && checkTerrainHeight.IsGrounded)
        {
            ExchangeColliders();
            isCrouching = !isCrouching;
            canCrouch = false;

            if (isRunning && canSlide && !isSliding)
            {
                SlidingEvent?.Invoke();
                StartCoroutine(Slide());
            }
            else if (!isRunning)
            {
                Invoke(nameof(ResetCrouchFlag), slideCrouchCooldown);
            }
        }
    }

    private IEnumerator Slide()
    {
        slideVFX.Play();

        isSliding = true;
        canSlide = false;
        yield return new WaitForSeconds(slideDuration);

        isSliding = false;
        isCrouching = false;

        canSlide = true;
        ExchangeColliders();
        Invoke(nameof(ResetCrouchFlag), slideCrouchCooldown);
    }

    private void ExchangeColliders()
    {
        if (playerCollider != null && crouchCollider != null)
        {
            if (playerCollider.enabled)
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

    private void ResetCrouchFlag()
    {
        canCrouch = true;
    }
    #endregion

    #region -----Getters Setters-----

    #region Bools
    public bool PlayerCanMove { get => canMove; set => canMove = value; }

    public bool PlayerIsMoving { get => isMoving; set => isMoving = value; }

    public bool PlayerIsJumping { get => isJumping; set => isJumping = value; }

    public bool PlayerIsRunning { get => isRunning; set => isRunning = value; }

    public bool PlayerIsCrouching { get => isCrouching; set => isCrouching = value; }

    public bool PlayerIsSliding { get => isSliding; set => isSliding = value; }

    public bool PlayerInGround { get => checkTerrainHeight.IsGrounded; }

    public bool PlayerIsAiming { get => isAiming; set => isAiming = value; }
    #endregion

    #region Ints
    public int JumpCount { get => jumpCount; set => jumpCount = value; }

    public int MaxJumps { get => maxJumps; set => maxJumps = value; }
    #endregion

    #region Floats
    public float JumpDuration { get => jumpDuration; set => jumpDuration = value; }

    public float SlideDuration { get => slideDuration; set => slideDuration = value; }

    public float PlayerSpeed { get => speed; set => speed = value; }
    #endregion

    #region Vectors
    public Vector2 MovementDirection { get => desiredMoveDirection; }

    public Vector2 MovInput { get => moveInput; }
    #endregion

    #endregion
}
