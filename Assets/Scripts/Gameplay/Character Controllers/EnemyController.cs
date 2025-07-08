using UnityEngine;
using System;

public class EnemyController : MonoBehaviour, ICharacterAnimatorData
{
    [Header("Patrol Settings")]
    [SerializeField] private Vector2 patrolDirection = Vector2.right;
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTimeAtEdge = 0.5f;
    [SerializeField] private float patrolArrivalThreshold = 0.1f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float jumpCooldown = 0.2f;

    [Header("Chase Settings")]
    [SerializeField] private float stoppingDistance = 0.5f;
    [SerializeField] private float chaseMemoryDuration = 2f;

    private float chaseTimer = 0f;
    private bool isChasing = false;
    private Transform currentTarget;

    private Rigidbody2D rb;
    private Collider2D col;
    private EnvironmentDetector detector;
    private CharacterAttack characterAttack;

    private PatrolBehavior patrolBehavior;
    private ChaseBehavior chaseBehavior;
    private StuckHandler stuckHandler;

    private float jumpCooldownTimer;
    private bool hasJumped = false;

    private Vector2 _moveInput;
    public Vector2 MoveInput => _moveInput;
    public Vector2 Velocity => rb.velocity;
    public bool IsGrounded => detector.IsGrounded(FeetPosition);
    public event Action OnAttack;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        detector = GetComponent<EnvironmentDetector>();
        characterAttack = GetComponent<CharacterAttack>();

        patrolBehavior = new PatrolBehavior(
            transform,
            detector,
            patrolDirection,
            patrolDistance,
            waitTimeAtEdge,
            patrolArrivalThreshold
        );

        chaseBehavior = new ChaseBehavior(detector, characterAttack, stoppingDistance);
        chaseBehavior.OnAttack += () => OnAttack?.Invoke();

        stuckHandler = new StuckHandler(detector);
    }

    private void Update()
    {
        if (jumpCooldownTimer > 0f)
            jumpCooldownTimer -= Time.deltaTime;

        Vector2 detectionDirection = isChasing && currentTarget != null
            ? ((Vector2)currentTarget.position - rb.position).normalized
            : patrolBehavior.DetectionDirection;

        Transform detectedTarget = detector.GetTarget(detectionDirection);

        if (detectedTarget != null)
        {
            currentTarget = detectedTarget;
            isChasing = true;
            chaseTimer = chaseMemoryDuration;
        }
        else if (isChasing)
        {
            chaseTimer -= Time.deltaTime;
            if (chaseTimer <= 0f)
            {
                isChasing = false;
                currentTarget = null;
            }
        }

        if (isChasing && currentTarget != null)
        {
            Vector2 direction = chaseBehavior.UpdateBehavior(
                currentTarget.position,
                rb.position,
                IsGrounded,
                CanJump,
                ref hasJumped,
                Jump
            );
            _moveInput = direction;
        }
        else
        {
            Vector2 direction = patrolBehavior.UpdateBehavior(
                rb.position,
                FeetPosition,
                IsGrounded,
                CanJump,
                ref hasJumped,
                Jump
            );
            _moveInput = direction;
        }

        rb.velocity = new Vector2(_moveInput.x * moveSpeed, rb.velocity.y);

        Vector2 forward = isChasing && currentTarget != null
            ? ((Vector2)currentTarget.position - rb.position).normalized
            : patrolBehavior.CurrentDirection;

        stuckHandler.Update(rb.position, IsGrounded, forward, FeetPosition, CanJump, Jump);
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpCooldownTimer = jumpCooldown;
    }

    private bool CanJump => jumpCooldownTimer <= 0f && Mathf.Abs(rb.velocity.y) < 0.01f;

    public void Disable()
    {
        _moveInput = Vector2.zero;
        rb.velocity = Vector2.zero;
        enabled = false;
    }

    private Vector2 FeetPosition => rb.position + Vector2.down * col.bounds.extents.y;

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            Vector2 currentFeet = transform.position + Vector3.down * GetComponent<Collider2D>().bounds.extents.y;
            Vector2 previewPointA = currentFeet;
            Vector2 previewPointB = previewPointA + patrolDirection.normalized * patrolDistance;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(previewPointA, previewPointB);
            Gizmos.DrawWireSphere(previewPointA, 0.1f);
            Gizmos.DrawWireSphere(previewPointB, 0.1f);
        }
    }
}
