using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;
using FMOD;


public class PlayerController : MonoBehaviour
{
    #region VariablesAndComponents
    [Header("Movement Settings")]
    public bool canMove = true;
    public float walkSpeed = 2f;
    public float runSpeed = 5f;

    [Header("Animator")]
    public Animator animator;
    [SerializeField] private FloatDampener speedX;
    [SerializeField] private FloatDampener speedY;
    [SerializeField] private FloatDampener layersDampener1;
    [SerializeField] private FloatDampener layersDampener2;

    [Header("Slide")]
    [SerializeField] private float slideDuration = 3f;
    [SerializeField] private float slideCooldown = 0.5f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    
    [Header("Jump")]
    public int maxJumps = 2;
    public float jumpVerticalForce = 10f;
    public float jumpHorizontalForce = 5f;
    [SerializeField] private float jumpDuration = 1.4f;
    [SerializeField] private float jumpCooldown = 1f;
    [SerializeField] private float SlopeJumpDownforce = 50f;

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle;
    [SerializeField] private GameObject stepRayLower;
    private RaycastHit slopeHit;

    [Header("Components")]
    private Rigidbody rb;
    public CapsuleCollider playerCollider;
    public CapsuleCollider crouchCollider;
    public Transform cameraTransform;
    [System.Obsolete] public CinemachineCamera freeLookCamera;
    [SerializeField] private ParticleSystem slideVFX;
    [SerializeField] private ParticleSystem jumpVFX;
    [SerializeField] private MeshTrail dashVFX;

    [Header("Camera Settings")]
    [SerializeField] private float currentFOV = 65;
    [SerializeField] private float aimFOV = 55;
    [SerializeField] private float tFOV = 1;
    [SerializeField] private float rotationSpeed = 10f;
    private bool wasAiming;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool isGrounded;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [SerializeField] private MultiAimConstraint aimRig;
    [SerializeField] private TwoBoneIKConstraint gripRig;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private float aimInput;
    private bool isRunning = false;
    private bool isCrouching = false;
    private bool isSliding = false;
    private bool canSlide = true;
    private bool canCrouch = true;
    private bool canJump = true;
    private bool isEmoting = false;
    private bool isAiming = false;
    private bool usingRifle = true;
    private bool isJumping;

    private int jumpCount = 0;

    private bool isDashing = false;
    private bool canDash = true;
    private bool canMelee = true;
    private bool canShoot = true;
    private bool canReload = true;

    private int animationLayerToShow = 0;

    private Vector3 slideDirection;
    private Vector3 desiredMoveDirection;

    private PlayerInput playerInput;
    private GunManager gunManager;
    private SoundManager soundManager;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
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
        UpdateAnimLayer();
        adjustFOV();

        animator.SetBool("ShortGun", gunManager.CurrentGun.Type == GunType.BasicPistol || gunManager.CurrentGun.Type == GunType.Revolver ? true : false);
        AdjustRigs();
    }

    private void FixedUpdate()
    {
        CheckGround();
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        if (!canMove)
        {
            soundManager.StopSound("Walk", "Run");
            return;
        }

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

        rb.useGravity = !OnSlope();

        if (OnSlope() && !isJumping)
        {
            rb.AddForce(GetSlopeMovement() * speed * 10f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
            {
                rb.AddForce(Vector3.down * SlopeJumpDownforce, ForceMode.Force);
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

    #region Animation
    private void HandleAnimations()
    {
        speedX.Update();
        speedY.Update();
        layersDampener1.Update();
        layersDampener2.Update();
        ChangeAnimLayer(SelectAnimLayer());

        bool isMoving = moveInput.sqrMagnitude > 0.1f;
        speedX.TargetValue = moveInput.x;
        speedY.TargetValue = moveInput.y;

        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isCrouching", isCrouching);
        animator.SetBool("isSliding", isSliding);
        animator.SetBool("isEmoting", isEmoting);

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

        for (int i = 0; i < 5; i++)
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
            if (isRunning)
            {
                return 3;
            }
            else
            {
                if (isAiming)
                {
                    return 4;
                }
                else
                {
                    return 1;
                }
            }
        }
        else
        {
            if (isRunning)
            {
                return 2;
            }
            else
            {
                if (isAiming)
                {
                    return 4;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
    private IEnumerator AnimLayerCountdown(string layer, float delay)
    {
        float timer = 0f;
        int layerI = animator.GetLayerIndex(layer);
        animator.SetLayerWeight(layerI, 1);

        while (timer < delay)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        animator.SetLayerWeight(layerI, 0);

        //Math.Clamp(timer, 0, 1);
        //while(animator.GetLayerWeight(layerI) > 0)
        //{
        //    timer -= Time.deltaTime;

        //    animator.SetLayerWeight(layerI, timer);
        //    yield return null;
        //}
    }

    #endregion

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

    public void OnChangeGun(InputAction.CallbackContext context)
    {
        if(context.performed) 
        {
            animator.SetTrigger("ChangeGun");
        }
    }

    public void OnFire(InputAction.CallbackContext context) //Lo pase al game manager ATT: Jacobo
    {
        if (!canShoot) return;

        if (context.started)
        {
            switch(gunManager.CurrentGun.Type)
            {
                case GunType.Rifle:
                    soundManager.PlaySound("rifleFire");
                    animator.SetBool("ShootBurst", true);
                    break;
                case GunType.BasicPistol:
                    soundManager.PlaySound("pistolFire");
                    animator.SetTrigger("ShootOnce");
                    break;
                case GunType.Revolver:
                    soundManager.PlaySound("revolverFire");
                    animator.SetTrigger("ShootOnce");
                    break;
                case GunType.Shotgun:
                    soundManager.PlaySound("shotgunFire");
                    animator.SetTrigger("ShootOnce");
                    break;
                case GunType.Sniper:
                    soundManager.PlaySound("sniperFire");
                    animator.SetTrigger("ShootOnce");
                    break;
            }
        }

        if (context.canceled)
        {
            soundManager.StopSound("rifleFire");
            animator.SetBool("ShootBurst", false);
        }
    }

    public void OnReload(InputAction.CallbackContext context) //Lo pase al game manager ATT: Jacobo
    {
        if(context.performed && canReload)
        {
            animator.SetTrigger("Reload");

            StartCoroutine(AnimLayerCountdown("Reload", 4f));

            switch (gunManager.CurrentGun.Type)
            {
                case GunType.Rifle:
                    soundManager.PlaySound("rifleReload");
                    StartCoroutine(Reload(2));
                    break;
                case GunType.BasicPistol:
                    soundManager.PlaySound("pistolReload");
                    StartCoroutine(Reload(2.12f));
                    break;
                case GunType.Revolver:
                    soundManager.PlaySound("revolverReload");
                    StartCoroutine(Reload(4.3f));
                    break;
                case GunType.Shotgun:
                    soundManager.PlaySound("shotgunReload");
                    StartCoroutine(Reload(5.4f));
                    break;
                case GunType.Sniper:
                    soundManager.PlaySound("sniperReload");
                    StartCoroutine(Reload(1.45f));
                    break;
            }
        }
    }

    private IEnumerator Reload(float delay)
    {
        soundManager.StopSound("rifleFire");
        animator.SetBool("ShootBurst", false);

        canReload = false;
        canShoot = false;
        yield return new WaitForSeconds(delay);
        canShoot = true;
        canReload = true;
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
    public void SetAimFOV(float gunAimFOV){
        aimFOV = gunAimFOV;
        UnityEngine.Debug.Log(gunAimFOV);
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

    #region Jump
    public void OnJump(InputAction.CallbackContext context)
    {
        if(isGrounded) 
        { 
            canJump = true;
            jumpCount = 0;
        }

        if (jumpCount > 1 || aimInput > 0.05f)
        {
            canJump = false;
            return;
        }
        else { canJump = true;}

        if (context.performed && jumpCount < maxJumps && canJump)
        {
            animator.applyRootMotion = false;

            jumpCount++;
            animator.SetTrigger("Jump");
            animator.SetBool("isJumping", true);
            canJump = false;
            isJumping = true;

            StartCoroutine(Jump());
        }
    }

    private IEnumerator Jump()
    {
        jumpVFX.Play();

        soundManager.StopSound("Walk", "Run");
        soundManager.PlaySound("Jump");

        rb.linearVelocity = Vector3.zero;
        rb.AddForce((desiredMoveDirection * jumpHorizontalForce) + (jumpVerticalForce * Vector3.up), ForceMode.Impulse);

        yield return new WaitForSeconds(jumpDuration);

        animator.SetBool("isJumping", false);
        isJumping = false;
        canJump = true;
    }
    #endregion

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

    #region Melee
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
        StartCoroutine(AnimLayerCountdown("Melee", 1.2f));
        
        canMove = false;
        canMelee = false;

        GetComponentInChildren<Melee>().ActivateMelee();

        float timer = 0f;
        while (timer < 1.1f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        canMove = true;
        canMelee = true;
        GetComponentInChildren<Melee>().DeactivateMelee();
    }
    #endregion

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
        slideVFX.Play();

        if (isSliding)
        {
            soundManager.StopSound("Run");
            
            yield return new WaitForSeconds(slideDuration);

            isSliding = false;
            isCrouching = false;
            yield return new WaitForSeconds(slideCooldown);

            canSlide = true;
            ResetCrouchFlag();
            ExchangeColliders();
        }
    }

    public void OnSlide()
    {
        isSliding = true;
        canSlide = false;
    }

    private void ResetCrouchFlag()
    {
        canCrouch = true;
    }

    public void OnDash()
    {
        if (!canDash || isCrouching || isSliding) return;

        canDash = false;
        isDashing = true;

        soundManager.StopSound("Walk", "Run");
        soundManager.PlaySound("Dash");

        dashVFX.StartTrail();

        StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        animator.applyRootMotion = false;
        rb.useGravity = false;

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(desiredMoveDirection.normalized * dashSpeed, ForceMode.Impulse);

        yield return new WaitForSeconds(dashDuration);
        animator.applyRootMotion = true;
        isDashing = false;
        rb.useGravity = true;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void CheckGround()
    {
        bool wasOnGround = isGrounded;

        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        isGrounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

        if (isGrounded && !wasOnGround)
        {
            soundManager.StopSound("Falling");
            soundManager.PlaySound("Landing");

            animator.applyRootMotion = true;
        }

        wasOnGround = isGrounded;

        CheckHeight();
    }

    private void CheckHeight()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 50f))
        {
            if (!isGrounded && this.rb.linearVelocity.y < 0.1f && this.rb.linearVelocity.y > -0.1f && hit.distance > 30) soundManager.PlaySound("Falling");
        }
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerCollider.height * 0.5f + 0.3f))
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

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (isGrounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius);
    }
}
