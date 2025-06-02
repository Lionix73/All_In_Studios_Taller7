using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;
using FMOD;


public class PlayerController : MonoBehaviour
{
    #region Visible Variables
    [Header("Movement Settings")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 5f;

    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private FloatDampener speedX;
    [SerializeField] private FloatDampener speedY;
    [SerializeField] private FloatDampener layersDampener1;
    [SerializeField] private FloatDampener layersDampener2;

    [Header("Slide")]
    [SerializeField] private float slideDuration = 3f;
    [SerializeField] private float slideCooldown = 0.5f;
    
    [Header("Jump")]
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private float jumpVerticalForce = 10f;
    [SerializeField] private float jumpHorizontalForce = 5f;
    [SerializeField] private float jumpDuration = 1.4f;

    [Header("Components")]
    [SerializeField] private Rigidbody rb;
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
    private ThisObjectSounds soundManager;
    private CheckTerrainHeight checkTerrainHeight;
    private Health health;
    private Rig rig;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        gunManager = FindAnyObjectByType<GunManager>();
        soundManager = GetComponent<ThisObjectSounds>();
        checkTerrainHeight = GetComponent<CheckTerrainHeight>();
        health = GetComponent<Health>();
        rig = GetComponentInChildren<Rig>();
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        cameraTransform = GameObject.FindGameObjectWithTag("FreeLookCamera").transform;
        freeLookCamera = GameObject.FindGameObjectWithTag("FreeLookCamera").GetComponent<CinemachineCamera>();
        freeLookCamera.Target.TrackingTarget = camTarget;
    }

    private void Update()
    {
        HandleAnimations();
        UpdateAnimLayer();
        adjustFOV();

        AdjustRigs();
        ChangeRigWeightDuringEmote();
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
            animator.SetBool("isEmoting", false);
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

    #region -----Animations / Animation Layers Management------
    private void HandleAnimations()
    {
        animator.SetBool("ShortGun", gunManager.CurrentGun.Type == GunType.BasicPistol || gunManager.CurrentGun.Type == GunType.Revolver ? true : false);

        speedX.Update();
        speedY.Update();
        layersDampener1.Update();
        layersDampener2.Update();
        ChangeAnimLayer(SelectAnimLayer());

        speedX.TargetValue = moveInput.x;
        speedY.TargetValue = moveInput.y;

        animator.SetBool("isGrounded", checkTerrainHeight.isGrounded);
        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isCrouching", isCrouching);
        animator.SetBool("isSliding", isSliding);
        isEmoting = animator.GetBool("isEmoting");

        animator.SetFloat("SpeedX", speedX.CurrentValue);
        animator.SetFloat("SpeedY", speedY.CurrentValue);
    }

    private void ChangeAnimLayer(int index)
    {
        if (animationLayerToShow == index) return;

        animationLayerToShow = index;

        if(Mathf.Abs(layersDampener1.TargetValue - layersDampener1.CurrentValue) <= 0.05f)
        {
            if(layersDampener1.TargetValue == 0)
            {
                layersDampener1.TargetValue = 1;
                layersDampener2.TargetValue = 0;
            }
            else
            {
                layersDampener1.TargetValue = 0;
                layersDampener2.TargetValue = 1;
            }
        }
    }

    private void UpdateAnimLayer()
    {
        animator.SetLayerWeight(animationLayerToShow, layersDampener1.TargetValue == 1 ? layersDampener1.CurrentValue : layersDampener2.CurrentValue);
        int layersAmount = animator.GetLayerIndex("Aim");

        for (int i = 0; i <= layersAmount; i++)
        {
            if (i != animationLayerToShow && animator.GetLayerWeight(i) > 0.05f)
            {
                animator.SetLayerWeight(i, layersDampener1.TargetValue == 0 ? layersDampener1.CurrentValue : layersDampener2.CurrentValue);
            }

            if (i != animationLayerToShow && animator.GetLayerWeight(i) < 0.05f && animator.GetLayerWeight(i) > 0f)
            {
                animator.SetLayerWeight(i, 0);
            }
        }
    }

    private int SelectAnimLayer()
    {
        if (gunManager.CurrentGun.Type == GunType.BasicPistol || gunManager.CurrentGun.Type == GunType.Revolver)
        {
            return isAiming ? 2 : 1;
        }
        else
        {
            return isAiming ? 2 : 0;
        }
    }

    private void SelectGunType()
    {
        switch (gunManager.CurrentGun.Type)
        {
            case GunType.Rifle:
                animator.SetFloat("GunType", 1f);
                break;

            case GunType.BasicPistol:
                animator.SetFloat("GunType", 2f);
                break;

            case GunType.Revolver:
                animator.SetFloat("GunType", 3f);
                break;

            case GunType.Shotgun:
                animator.SetFloat("GunType", 4f);
                break;

            case GunType.Sniper:
                animator.SetFloat("GunType", 5f);
                break;

            case GunType.ShinelessFeather:
                animator.SetFloat("GunType", 6f);
                break;

            case GunType.GoldenFeather:
                animator.SetFloat("GunType", 7f);
                break;

            case GunType.GranadeLaucher:
                animator.SetFloat("GunType", 8f);
                break;

            case GunType.AncientTome:
                animator.SetFloat("GunType", 9f);
                break;

            case GunType.Crossbow:
                animator.SetFloat("GunType", 10f);
                break;

            case GunType.MysticCanon:
                animator.SetFloat("GunType", 11f);
                break;
        }
    }
    #endregion

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

    public void OnChangeGun(InputAction.CallbackContext context)
    {
        if(context.performed) 
        {
            animator.SetTrigger("ChangeGun");
        }
    }

    #region -----FOV-----
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
    public void SetAimFOV(float gunAimFOV){
        aimFOV = gunAimFOV;
        UnityEngine.Debug.Log(gunAimFOV);
    }
    #endregion

    #region -----Adjust Rigs-----
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

    private void ChangeRigWeightDuringEmote()
    {
        if (isEmoting)
            rig.weight = 0f;
        else 
            rig.weight = 1f;
    }
    #endregion

    #region -----Jump-----
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!playerAllowedToJump) return;

        if (context.performed && jumpCount < maxJumps)
        {
            animator.applyRootMotion = false;

            jumpCount++;
            animator.SetTrigger("Jump");
            animator.SetBool("isJumping", true);
            isJumping = true;

            StartCoroutine(Jump());
        }
    }

    private IEnumerator Jump()
    {
        jumpVFX.Play();

        rb.linearVelocity = Vector3.zero;
        rb.AddForce((desiredMoveDirection * jumpHorizontalForce) + (jumpVerticalForce * Vector3.up), ForceMode.Impulse);

        yield return new WaitForSeconds(jumpDuration);

        animator.SetBool("isJumping", false);
        isJumping = false;
    }
    #endregion

    #region -----Melee-----
    public void OnMelee(InputAction.CallbackContext context) 
    {
        if(context.performed && canMelee)
        {
            StartCoroutine(Melee());
        }
    }

    private IEnumerator Melee()
    {
        animator.SetTrigger("Melee");
        
        canMove = false;
        canMelee = false;

        GetComponentInChildren<Melee>().ActivateMelee();

        yield return new WaitForSeconds(1.1f);

        canMove = true;
        canMelee = true;
        GetComponentInChildren<Melee>().DeactivateMelee();
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
                soundManager.PlaySound("Slide");
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

        //yield return new WaitForSeconds(slideDuration);
        float timer = 0f;
        while (timer < slideDuration)
        {
            soundManager.StopSound("Run");
            timer += Time.deltaTime;
            yield return null;
        }

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
    public bool PlayerCanMove
    {
        get => canMove;
        set => canMove = value;
    }

    public bool PlayerIsMoving
    {
        get => isMoving;
        set => isMoving = value;
    }

    public bool PlayerIsEmoting
    {
        get => isEmoting;
        set => isEmoting = value;
    }

    public bool PlayerIsJumping
    {
        get => isJumping;
        set => isJumping = value;
    }

    public int JumpCount
    {
        get => jumpCount;
        set => jumpCount = value;
    }

    public bool PlayerRunning
    {
        get => isRunning;
        set => isRunning = value;
    }

    public bool PlayerInGround
    {
        get => checkTerrainHeight.isGrounded;
        set => checkTerrainHeight.isGrounded = value;
    }

    public bool PlayerCanJump
    {
        get => playerAllowedToJump;
        set => playerAllowedToJump = value;
    }

    public int MaxJumps
    {
        get => maxJumps;
        set => maxJumps = value;
    }

    public float PlayerSpeed
    {
        get => speed;
        set => speed = value;
    }

    public Vector2 MovementDirection
    {
        get => desiredMoveDirection;
    }
    #endregion
}
