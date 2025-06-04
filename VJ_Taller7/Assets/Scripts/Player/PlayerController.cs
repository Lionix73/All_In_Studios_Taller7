using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;

public class PlayerController : MonoBehaviour
{
    #region Visible Variables
    [Header("Movement Settings")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 5f;

    [Header("Slide")]
    [SerializeField] private float slideDuration = 3f;
    [SerializeField] private float slideCooldown = 0.5f;
    
    [Header("Jump")]
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private float jumpVerticalForce = 10f;
    [SerializeField] private float jumpHorizontalForce = 5f;
    [SerializeField] private float jumpDuration = 1.4f;

    [Header("Components")]
    [SerializeField] private CapsuleCollider playerCollider;
    [SerializeField] private CapsuleCollider crouchCollider;
    [SerializeField] private Transform cameraTransform;
    public CinemachineCamera freeLookCamera;

    [Header("VFX")]
    [SerializeField] private ParticleSystem slideVFX;
    [SerializeField] private ParticleSystem jumpVFX;

    [Header("Camera Settings")]
    [SerializeField] private Transform camTarget;
    [SerializeField] private float currentFOV = 65;
    [SerializeField] private float aimFOV = 55;
    [SerializeField] private float tFOV = 1;
    [SerializeField] private float rotationSpeed = 10f;

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
    private int animationLayerToShow = 0;
    #endregion

    #region Private Bools
    private bool wasAiming;
    private bool isRunning = false;
    private bool isCrouching = false;
    private bool isSliding = false;
    private bool canSlide = true;
    private bool canCrouch = true;
    private bool canJump = true;
    private bool playerAllowedToJump = true;
    private bool isEmoting = false;
    private bool isAiming = false;
    private bool isMoving;
    private bool usingRifle = true;
    private bool isJumping;
    private bool wasOnGround;
    private bool isDashing = false;
    private bool canDash = true;
    private bool canMelee = true;
    private bool canShoot = true;
    private bool canReload = true;
    #endregion

    #region Private Vectors
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 slideDirection;
    private Vector3 desiredMoveDirection;
    #endregion

    #region Private Components
    private PlayerInput playerInput;
    private GunManager gunManager;
    private CheckTerrainHeight checkTerrainHeight;
    private Rig rig;
    private Rigidbody rb;
    private Melee melee;
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
        playerInput = GetComponent<PlayerInput>();
        gunManager = FindAnyObjectByType<GunManager>();
        checkTerrainHeight = GetComponent<CheckTerrainHeight>();
        rig = GetComponentInChildren<Rig>();
        melee = GetComponentInChildren<Melee>();
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        freeLookCamera = GameObject.FindGameObjectWithTag("FreeLookCamera").GetComponent<CinemachineCamera>();
        cameraTransform = freeLookCamera.transform;
        freeLookCamera.Target.TrackingTarget = camTarget;
    }

    private void Update()
    {
        AdjustFOV();
        AdjustRigs();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        if (!canMove) return;

        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        isMoving = moveInput.sqrMagnitude > 0.1f;

        desiredMoveDirection = (transform.right * moveDirection.x + transform.forward * moveDirection.z);

        speed = isRunning ? runSpeed : walkSpeed;

        if (moveDirection != Vector3.zero)
        {
            rb.MovePosition(rb.position + desiredMoveDirection * speed * Time.deltaTime);
        }
        else
        {
            isRunning = false;
        }
    }

    private void HandleRotation()
    {
        if(!canMove) return;

        if (moveInput.magnitude > 0.05f || aimInput > 0.05f)
        {
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

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

        if (context.started) gunManager.CheckZoomIn();
        if (context.canceled) gunManager.CheckZoomOut();
    }
    
    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isRunning = !isRunning;
            isCrouching = false;
        }
    }

    #region -----FOV-----
    public void AdjustFOV()
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
    public void SetAimFOV(float gunAimFOV)
    {
        aimFOV = gunAimFOV;
        //Debug.Log(gunAimFOV);
    }
    #endregion

    #region -----Adjust Rigs-----
    /// <summary>
    /// Ajusta los rigs que se tienen para el agarre del arma
    /// con la otra mano y el punto donde se apunta
    /// </summary>
    private void AdjustRigs()
    {
        if (aimRig==null || gripRig == null) return;

        else if (wasAiming)
        {
            aimRig.weight=1.0f;
            gripRig.weight = 1.0f;
        }
        else 
        {
            aimRig.weight=0; 
            gripRig.weight = 0.5f;
        }

        if (moveInput.magnitude!=0)
        {
            aimRig.weight = 1.0f;
        }
    }
    #endregion

    #region -----Jump-----
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!playerAllowedToJump) return;

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
        if(context.performed && canMelee)
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
        if (context.performed && canCrouch && checkTerrainHeight.isGrounded)
        {
            ExchangeColliders();
            isCrouching = !isCrouching;
            canCrouch = false;

            if(isRunning && canSlide && !isSliding)
            {
                SlidingEvent?.Invoke();
                StartCoroutine(Slide());
            }

            Invoke(nameof(ResetCrouchFlag), 0.5f);
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
        yield return new WaitForSeconds(slideCooldown);

        canSlide = true;
        ExchangeColliders();
        Invoke(nameof(ResetCrouchFlag), 0.5f);
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

    private void ResetCrouchFlag()
    {
        canCrouch = true;
    }
    #endregion

    #region -----Getters Setters-----
    #region Bools
    public bool PlayerCanMove { get => canMove; set => canMove = value; }

    public bool PlayerIsMoving { get => isMoving; set => isMoving = value; }

    public bool PlayerIsEmoting { get => isEmoting; set => isEmoting = value; }

    public bool PlayerIsJumping { get => isJumping; set => isJumping = value; }

    public bool PlayerIsRunning { get => isRunning; set => isRunning = value; }

    public bool PlayerIsCrouching { get => isCrouching; set => isCrouching = value; }

    public bool PlayerIsSliding { get => isSliding; set => isSliding = value; }

    public bool PlayerInGround { get => checkTerrainHeight.isGrounded; set => checkTerrainHeight.isGrounded = value; }

    public bool PlayerCanJump { get => playerAllowedToJump; set => playerAllowedToJump = value; }

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
