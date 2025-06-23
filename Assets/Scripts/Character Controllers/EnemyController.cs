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

    private Rigidbody2D rb;
    private Collider2D col;
    private EnvironmentDetector detector;

    private Vector2 pointA;
    private Vector2 pointB;
    private Vector2 patrolTarget;
    private bool waiting;
    private float waitTimer;

    private Vector2 _moveInput;
    private float jumpCooldownTimer;
    private bool hasJumped = false;

    private Transform currentTarget;

    public Vector2 MoveInput => _moveInput;
    public Vector2 Velocity => rb.velocity;
    public bool IsGrounded => detector.IsGrounded(FeetPosition);

    private CharacterAttack characterAttack;
    public event Action OnAttack;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        detector = GetComponent<EnvironmentDetector>();
        characterAttack = GetComponent<CharacterAttack>();

        Vector2 start = FeetPosition;
        pointA = start;
        pointB = pointA + patrolDirection.normalized * patrolDistance;
        patrolTarget = pointB;
    }

    private void Update()
    {
        if (jumpCooldownTimer > 0f)
            jumpCooldownTimer -= Time.deltaTime;

        Transform detectedTarget = detector.GetTarget(patrolDirection);

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
            ChaseLogic();
        else
            PatrolUpdate();
    }

    private void PatrolUpdate()
    {
        if (waiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                waiting = false;
                patrolTarget = (patrolTarget == pointA) ? pointB : pointA;
                patrolDirection = -patrolDirection;
            }

            _moveInput = Vector2.zero;
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 direction = patrolTarget - FeetPosition;
        direction.y = 0f;
        direction.Normalize();

        _moveInput = direction;
        TryJump(direction);

        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

        float horizontalDistance = Mathf.Abs(patrolTarget.x - FeetPosition.x);
        if (horizontalDistance < patrolArrivalThreshold && IsGrounded)
        {
            rb.velocity = Vector2.zero;
            waiting = true;
            waitTimer = waitTimeAtEdge;
        }
    }

    private void ChaseLogic()
    {
        if (currentTarget == null) return;

        Vector2 toTarget = (Vector2)currentTarget.position - rb.position;
        float distance = toTarget.magnitude;

        if (distance <= stoppingDistance)
        {
            _moveInput = Vector2.zero;
            rb.velocity = new Vector2(0f, rb.velocity.y);

            if (characterAttack != null && Time.time - characterAttack.LastAttackTime >= characterAttack.AttackCooldown)
            {
                OnAttack?.Invoke();
            }

            return;
        }

        Vector2 direction = toTarget.normalized;
        _moveInput = direction;

        TryJump(direction);

        float horizontalSpeed = direction.x * moveSpeed;
        if (IsGrounded || Mathf.Abs(direction.x) > 0.1f)
        {
            rb.velocity = new Vector2(horizontalSpeed, rb.velocity.y);
        }
    }

    private void TryJump(Vector2 direction)
    {
        bool shouldJump = detector.ShouldJump(direction, FeetPosition);

        if (IsGrounded && shouldJump && !hasJumped && jumpCooldownTimer <= 0f)
        {
            Jump();
            hasJumped = true;
            jumpCooldownTimer = jumpCooldown;
        }

        if (IsGrounded && !shouldJump)
        {
            hasJumped = false;
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpCooldownTimer = jumpCooldown;
    }

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
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pointA, pointB);
        Gizmos.DrawWireSphere(pointA, 0.1f);
        Gizmos.DrawWireSphere(pointB, 0.1f);
    }

}