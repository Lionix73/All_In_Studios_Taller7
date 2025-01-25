using UnityEngine;
using UnityEngine.InputSystem;

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

    private float lastCameraYRotation = 0f; // Almacena la última rotación en el eje Y de la cámara
    private bool isRotating = false; // Controla si el personaje está ejecutando la animación de rotación
    private Vector2 moveInput;
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

    private void Update()
    {
        HandleMovement();
        HandleAnimations();
    }

    private void HandleMovement()
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

        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        Vector3 worldInputDir = transform.TransformDirection(moveDirection);

        if (moveDirection.z > 0.05f)
        {
            forward = true;
            backward = false;
        }
        else if (moveDirection.z < -0.05f)
        {
            forward = false;
            backward = true;
        }
        else
        {
            forward = false;
            backward = false;
        }


        if (moveDirection.x > 0.05f)
        {
            right = true;
            left = false;
        }
        else if (moveDirection.x < -0.05f)
        {
            right = false;
            left = true;
        }
        else
        {
            right = false;
            left = false;
        }

        if (moveDirection != Vector3.zero)
        {
            isCrouching = false;

            // Obtener la dirección orientada según la cámara
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            // Asegurarnos de que los vectores estén en el plano horizontal (sin componente Y)
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Calcular la dirección final del movimiento en el espacio local de la cámara
            Vector3 desiredMoveDirection = (cameraRight * moveDirection.x + cameraForward * moveDirection.z).normalized;

            float speed = isRunning ? runSpeed : walkSpeed;
            // Mover al personaje en la dirección calculada
            rb.MovePosition(rb.position + desiredMoveDirection * speed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDirection);

            if(forward == true && right == false && left == false)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }    

            // Determinar el ángulo entre el personaje y la cámara
            //float angleToCamera = Vector3.Angle(transform.forward, desiredMoveDirection);

            //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        if (moveDirection == Vector3.zero)
        {
            isRunning = false;
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
        Debug.Log("Movimiento detectado");

        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log("Salto detectado");

        if (context.performed && jumpCount < maxJumps)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z); // Reset Y velocity
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
    private bool CheckForCameraRotation(float angle)
    {
        // Obtener la rotación actual de la cámara en el eje Y
        float currentCameraYRotation = cameraTransform.eulerAngles.y;

        // Calcular la diferencia en rotación respecto a la última posición
        float rotationDifference = Mathf.Abs(Mathf.DeltaAngle(lastCameraYRotation, currentCameraYRotation));

        if (rotationDifference > angle)
        {
            // Ajustar la rotación del personaje para alinearse con la cámara
            Vector3 newForward = cameraTransform.forward;
            newForward.y = 0f;
            //transform.rotation = Quaternion.LookRotation(newForward);

            // Actualizar la última rotación de la cámara
            lastCameraYRotation = currentCameraYRotation;

            return true;
        }
        return false;
    }
}
