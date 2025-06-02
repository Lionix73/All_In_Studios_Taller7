using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
public class CheckTerrainHeight : MonoBehaviour
{
    [SerializeField][Range(1, 100)]
    [Tooltip("Minimum height to play the falling sound")]
    private int heightThreshold;

    [SerializeField][Range(-30, 0)]
    [Tooltip("Minimum Y speed to play the landing sound")]
    private int velocityThreshold;

    #region Ground Variables
    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool isGrounded;

    [Tooltip("Useful for rough ground")]
    [SerializeField] private float GroundedOffset = 0.05f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    [SerializeField] private float GroundedRadius = 0.1f;

    [Tooltip("What layers the character uses as ground")]
    [SerializeField] private LayerMask GroundLayers;

    private bool wasOnGround;
    private float maxVerticalVelocity = 0;
    #endregion

    #region Slope Variables
    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle = 45f;
    [SerializeField] private float SlopeJumpDownforce = 50f;
    private RaycastHit slopeHit;
    #endregion

    #region Components
    [Header("Components")]
    [SerializeField] private CapsuleCollider playerCollider;
    private ThisObjectSounds _soundManager;
    private PlayerController _playerController;
    private Rigidbody _rb;
    private Animator _animator;
    #endregion

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _soundManager = GetComponent<ThisObjectSounds>();
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        CheckGround();
        CheckHeight();
        HandleGravityAndSlope();
    }

    private void HandleGravityAndSlope()
    {
        _rb.useGravity = !isGrounded ? true : !OnSlope();

        if (OnSlope() && !_playerController.PlayerIsJumping)
        {
            _rb.AddForce(GetSlopeMovement() * _playerController.PlayerSpeed * 10f, ForceMode.Force);

            if (_rb.linearVelocity.y > 0)
            {
                _rb.AddForce(Vector3.down * SlopeJumpDownforce, ForceMode.Force);
            }
        }
    }

    #region Terrain and Height Check
    private void CheckGround()
    {
        wasOnGround = isGrounded;
        maxVerticalVelocity = _rb.linearVelocity.y < maxVerticalVelocity ? _rb.linearVelocity.y : maxVerticalVelocity;

        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        isGrounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

        if (isGrounded && !wasOnGround)
        {
            _soundManager.StopSound("Falling");
            _animator.applyRootMotion = true;
            _playerController.JumpCount = 0;

            if (maxVerticalVelocity < velocityThreshold)
            {
                _soundManager.PlaySound("Landing");
            }
            maxVerticalVelocity = 0;
        }

        wasOnGround = isGrounded;

    }

    private void CheckHeight()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 50f))
        {
            if (!isGrounded && hit.distance > heightThreshold && _rb.linearVelocity.y < 0f) _soundManager.PlaySound("Falling");
        }
    }
    #endregion

    #region Slope Check
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
        return Vector3.ProjectOnPlane(_playerController.MovementDirection, slopeHit.normal).normalized;
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
    #endregion
}
