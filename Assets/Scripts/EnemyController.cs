using UnityEngine;
using System;

public class EnemyController : MonoBehaviour, ICharacterAnimatorData
{
    [Header("Patrol Settings")]
    [SerializeField] private Vector2 patrolDirection = Vector2.right;
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTimeAtEdge = 0.5f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float jumpCooldown = 0.2f;

    private Rigidbody2D rb;
    private EnvironmentDetector detector;

    private Vector2 pointA;
    private Vector2 pointB;
    private Vector2 target;
    private bool waiting;
    private float waitTimer;

    private Vector2 _moveInput;
    private Vector2 _lastPosition;
    private float jumpCooldownTimer;
    private bool hasJumped = false;

    public Vector2 MoveInput => _moveInput;
    public Vector2 Velocity => rb.velocity;
    public bool IsGrounded => detector.IsGrounded();

    public event Action OnAttack;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        detector = GetComponent<EnvironmentDetector>();

        Vector2 start = rb.position;
        pointA = start;
        pointB = start + patrolDirection.normalized * patrolDistance;
        target = pointB;

        _lastPosition = start;
    }

    private void Update()
    {
        if (waiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                waiting = false;
                target = (target == pointA) ? pointB : pointA;
                patrolDirection = -patrolDirection;
            }

            _moveInput = Vector2.zero;
            return;
        }

        if (jumpCooldownTimer > 0f)
            jumpCooldownTimer -= Time.deltaTime;

        PatrolLogic();
        _lastPosition = rb.position;
    }

    private void PatrolLogic()
    {
        Vector2 direction = (target - rb.position).normalized;
        _moveInput = direction;

        bool shouldJump = detector.ShouldJump(direction);

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

        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

        if (Vector2.Distance(rb.position, target) < 0.05f)
        {
            rb.velocity = Vector2.zero;
            rb.MovePosition(target);
            waiting = true;
            waitTimer = waitTimeAtEdge;
        }
    }


    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpCooldownTimer = jumpCooldown;
    }

    public void TriggerAttack()
    {
        OnAttack?.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 start = Application.isPlaying ? pointA : (Vector2)transform.position;
        Vector2 end = start + patrolDirection.normalized * patrolDistance;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(start, 0.1f);
        Gizmos.DrawWireSphere(end, 0.1f);
    }
}
