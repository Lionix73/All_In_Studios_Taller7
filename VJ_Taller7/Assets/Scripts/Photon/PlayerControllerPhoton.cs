using System.Collections;
using Unity.Cinemachine;
using Fusion.Addons.KCC;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Fusion;
using UnityEngine.Windows;
using Projectiles;
public class PlayerControllerPhoton : NetworkBehaviour
{

    [Header("KCC")]
    [SerializeField] KCC kcc;
    [SerializeField] KCCProcessor dashProcessor;
    //[SerializeField] private AudioSource source;
    [Header("Movement Settings")]
    [SerializeField] private Vector3 jumpImpulse = new(0f, 10f, 0f);
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
    [SerializeField] private float dashImpulse = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCD = 2f;
    [SerializeField] private float doubleJumpCD = 2f;
    [SerializeField] private float slideImpulse = 7.5f;
    [SerializeField] private float slideCD = 1.5f;
    [SerializeField] private float jumpDuration = 1.4f;
    [SerializeField] private float doubleJumpDuration = 1.4f;
    [SerializeField] private float jumpCooldown = 1f;

    private bool isDashing = false;
    private bool canDash = true;
    bool rayDash;
    private Vector3 dashDirection;

    [Header("Components")]
    [SerializeField] private CapsuleCollider[] colliders;
    public Transform cameraTransform;
    public Transform playerHead;
    [System.Obsolete] public CinemachineCamera freeLookCamera;

    [Header("Camera Settings")]
    [SerializeField] private Transform camTarget;
    [SerializeField] private float lookSensitivity = 0.15f;
    [SerializeField] private float currentFOV = 65;
    [SerializeField] private float aimFOV = 55;
    [SerializeField] private float tFOV = 1;
    [SerializeField] private float rotationSpeed = 10f;
    private bool wasAiming;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private float aimInput;
    [Networked] public bool IsRunning { get; set; }
    [Networked] public bool IsCrouching { get; set; }
    [Networked] public bool IsSliding { get; set; }
    [Networked] public bool IsEmoting { get; set; }
    [Networked] public bool IsJumping { get; set; }
    [Networked] public bool IsDoubleJumping { get; set; }
    private bool IsGrounded => kcc.Data.IsGrounded;

    private bool CanSlide => IsRunning && !IsSliding;
    private bool CanCrouch => kcc.Data.IsGrounded;
    private bool CanJump => kcc.Data.IsGrounded;

    private float speedX;
    private float speedY;

    private WeaponBase _weapon;


    private int jumpCount = 0;
    [Networked, OnChangedRender(nameof(Jumped))] private int JumpSync { get; set; } //Synchronize sound in all clients

    private Vector3 slideDirection;
    private float slideTimer = 0f;

    public float DoubleJumpCDFactor => (DoubleJumpCD.RemainingTime(Runner) ?? 0f) / doubleJumpCD; //Returns the remaining of cooldown in a range of 0 to 1
    [Networked] private TickTimer DashCD { get; set; }
    [Networked] private TickTimer DoubleJumpCD { get; set; }
    [Networked] private TickTimer SlideCD { get; set; }

    private PlayerInput playerInput;
    [Networked] private NetworkButtons PreviousButtons { get; set; }

    private InputManager inputManager;
    private Vector2 baseLookRotation;

    public bool IsReady; //Server is the only one who cares about this

    private void Awake()
    {
        _weapon = GetComponentInChildren<WeaponBase>();
    }
    public override void Spawned()
    {
        //kcc.SetGravity(Physics.gravity.y * 2f);
        colliders = gameObject.GetComponentsInChildren<CapsuleCollider>();
        playerInput = GetComponent<PlayerInput>();
        GameObject camera = FindFirstObjectByType<CinemachineCamera>().gameObject;
        freeLookCamera = camera.GetComponent<CinemachineCamera>();
        cameraTransform = camera.transform;
        
            if(HasInputAuthority)
            {
                inputManager = Runner.GetComponent<InputManager>();
                inputManager.LocalPlayer = this;
                freeLookCamera.Target.TrackingTarget = camTarget;
                kcc.Settings.ForcePredictedLookRotation = true;
            Debug.Log("Camara Encontrada");
            }
    }


    public override void FixedUpdateNetwork()
    {

        
        HandleAnimations();
        HandleMovement();
        //HandleRotation();
        
    }

    private void HandleMovement()
    {
        if (GetInput(out NetworkInputData input))
        {
            CheckEmote(input);
            CheckJump(input);
            kcc.AddLookRotation(input.LookDelta * lookSensitivity);
            HandleRotation();

            CheckRun(input);
            //bool isMoving = input.Direction.x > 0f || input.Direction.y > 0f;
            float speed = IsRunning ? runSpeed : walkSpeed;
            CheckCrouch(input);
            
            SetInputDirection(input, speed);
            CheckDash(input);
            adjustFOV(input);
            FireWeapon(input);

            PreviousButtons = input.Buttons;
            baseLookRotation = kcc.GetLookRotation();
        }
    }
    private void CheckJump(NetworkInputData input)
    {
        if(input.Buttons.WasPressed(PreviousButtons, InputButton.Jump))
        {
            if (CanJump)
            {
                IsJumping = true;
                animator.SetBool("isJumping", IsJumping);
                GetComponent<NetworkMecanimAnimator>().SetTrigger(125937960);
                StartCoroutine(Jump());
                kcc.Jump(jumpImpulse);
                JumpSync++;
                //animator.SetBool("isJumping", true);
                //animator.SetTrigger("Jump");

            }
            else if(DoubleJumpCD.ExpiredOrNotRunning(Runner))
            {
                GetComponent<NetworkMecanimAnimator>().SetTrigger(129565385);
                animator.SetTrigger("DoubleJump");
                kcc.Jump(jumpImpulse);
                DoubleJumpCD = TickTimer.CreateFromSeconds(Runner, doubleJumpCD);
                JumpSync++;
            }
        }
    }
    private IEnumerator Jump()
    {
        float jumpTimer = 0f;

        while (jumpTimer < jumpDuration)
        {
            jumpTimer += Runner.DeltaTime;
            yield return null;
        }
        
        IsJumping = false;
        animator.SetBool("isJumping", IsJumping);
    }
    private void CheckDash(NetworkInputData input)
    {
        if (input.Buttons.WasPressed(PreviousButtons, InputButton.Dash))
        {
            if (IsCrouching || IsSliding) 
                return;
            if (DashCD.ExpiredOrNotRunning(Runner))
            {
                isDashing = true;
                kcc.AddModifier(dashProcessor);
                Debug.Log(dashProcessor);
                Vector3 worldDirection = kcc.FixedData.TransformRotation * input.Direction.X0Y();
                Debug.Log(worldDirection);
                kcc.Jump(worldDirection * dashImpulse);
                kcc.SetDynamicVelocity(kcc.FixedData.DynamicVelocity - Vector3.Scale(kcc.FixedData.DynamicVelocity, worldDirection.normalized));
                kcc.AddExternalImpulse(worldDirection * dashImpulse);
                Debug.Log("Dash");
                kcc.RemoveModifier(dashProcessor);
                Debug.Log(dashProcessor);
                DashCD = TickTimer.CreateFromSeconds(Runner, dashCD);
            }

        }

    }
    private void SetInputDirection(NetworkInputData input, float speed)
    {
        Vector3 worldDirection = kcc.FixedData.TransformRotation * input.Direction.X0Y()*speed;
        kcc.SetInputDirection(worldDirection);

    }
    private void CheckRun(NetworkInputData input)
    {
        if (input.Buttons.WasPressed(PreviousButtons, InputButton.Run))
        {
            Debug.Log("Trying to run");
            IsRunning = !IsRunning;
            IsCrouching = false;
        }
    }
    private void CheckCrouch(NetworkInputData input)
    {
        if (input.Buttons.WasPressed(PreviousButtons, InputButton.Crouch) && CanCrouch)
        {
            Debug.Log("Trying to crouch");
            ExchangeColliders();
            if (!IsRunning)
            {
                IsCrouching = !IsCrouching;
            }
            else if (IsRunning && !IsSliding)
            {
                Debug.Log("Slide");
                OnSliding(input);
                
            }

        }
    }

    private void CheckEmote(NetworkInputData input)
    {
        if (input.Buttons.WasPressed(PreviousButtons,InputButton.Emote))
        { 
            IsEmoting = !IsEmoting;
        }
    }
    private void ExchangeColliders()
    {
        if (colliders != null)
        {
            Debug.Log("Cambio de colliders");
            colliders[0].enabled = !IsCrouching;
            colliders[1].enabled = IsCrouching;
        }
    }
    private void OnSliding(NetworkInputData input)
    {
        if (SlideCD.ExpiredOrNotRunning(Runner) && CanSlide)
        {
            Vector3 worldDirection = kcc.FixedData.TransformRotation * input.Direction.X0Y();
            Debug.Log(worldDirection);
            kcc.Jump(worldDirection * dashImpulse);
            SlideCD = TickTimer.CreateFromSeconds(Runner, slideCD);
            IsSliding = true;
            StartCoroutine(Slide());
        }
    }
    private IEnumerator Slide()
    {
        if (IsSliding)
        {
            float slideTimer = 0f;

            while (slideTimer < slideDuration)
            {
                slideTimer += Runner.DeltaTime;
                yield return null;
            }

            IsSliding = false;
            IsCrouching = false;
            //ExchangeColliders();
        }
    }


    public override void Render()
    {
        //HandleAnimations();
        if (kcc.Settings.ForcePredictedLookRotation && HasInputAuthority)
        {
            Vector2 predictedLookRotation = baseLookRotation + inputManager.AccumulatedMouseDelta * lookSensitivity;
            kcc.SetLookRotation(predictedLookRotation);
        }
            HandleRotation();
    }
    private void HandleRotation()
    {
        camTarget.localRotation = Quaternion.Euler(kcc.GetLookRotation().x,0f, 0f);
    }

    private void HandleAnimations()
    {
        bool isMoving = false;
        if (GetInput(out NetworkInputData input))
        { 
            isMoving = input.Direction.sqrMagnitude > 0f && kcc.Data.IsGrounded;
            speedX = input.Direction.x;
            speedY = input.Direction.y;

            if (input.Direction.sqrMagnitude < 0.1f && IsRunning)
                IsRunning = false;

        }

        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isRunning", IsRunning);
        animator.SetBool("isCrouching", IsCrouching);
        animator.SetBool("isSliding", IsSliding);
        animator.SetBool("isEmoting", IsEmoting);
        animator.SetBool("isGrounded", IsGrounded);

        animator.SetFloat("SpeedY", speedY);
        animator.SetFloat("SpeedX", speedX);
    }



    /*public void OnMove(InputAction.CallbackContext context)
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
    }*/

    public void adjustFOV(NetworkInputData input)
    {
        if (input.Buttons.WasPressed(PreviousButtons, InputButton.OnAim))
        {
            freeLookCamera.Lens.FieldOfView = Mathf.Lerp(currentFOV, aimFOV, tFOV * Time.deltaTime);
            wasAiming = !wasAiming;
            if (wasAiming == false)
            {
                freeLookCamera.Lens.FieldOfView = Mathf.Lerp(aimFOV, currentFOV, tFOV * Time.deltaTime);
               // IsRunning = false;
            }
            else
            {
                freeLookCamera.Lens.FieldOfView = Mathf.Lerp(currentFOV, aimFOV, tFOV * Time.deltaTime);
            }
        }
    }

    public void FireWeapon(NetworkInputData input)
    {
        if (input.Buttons.WasPressed(PreviousButtons, InputButton.Fire))
        {
            Debug.Log("Disparar");
            _weapon.Fire();
        }
    }
    private void Jumped()
    {
        //source.Play();
    }



[Rpc(RpcSources.InputAuthority, RpcTargets.InputAuthority | RpcTargets.StateAuthority)] // The ui update is actually allowed to run locally when the player indicates their readiness
    public void RPC_SetReady()
    {
        IsReady = true;
        if (HasInputAuthority) { }
            //UIManager.Singleton.DidSetReady();
    }
    public void Teleport(Vector3 position, Quaternion rotation)
    {
        kcc.SetPosition(position);
        kcc.SetLookRotation(rotation);
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_PlayerName(string name)
    {
        //Name = name;
    }

}