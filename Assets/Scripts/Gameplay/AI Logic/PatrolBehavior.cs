using UnityEngine;
using System;

public class PatrolBehavior
{
    private Vector2 pointA;
    private Vector2 pointB;
    private Vector2 patrolTarget;
    private float waitTime = 0.5f;
    private float patrolDistance = 3f;
    private float arrivalThreshold = 0.1f;
    private float waitTimer;
    private bool waiting;
    private Transform transform;
    private EnvironmentDetector detector;

    public Vector2 CurrentDirection { get; private set; }
    private Vector2 lastDirection = Vector2.right;
    public Vector2 DetectionDirection => lastDirection;

    public PatrolBehavior(Transform t, EnvironmentDetector d, Vector2 patrolDirection = default, float distance = 3f, float wait = 0.5f, float threshold = 0.1f)
    {
        transform = t;
        detector = d;
        patrolDistance = distance;
        waitTime = wait;
        arrivalThreshold = threshold;

        Vector2 start = transform.position;
        pointA = start;
        pointB = pointA + (patrolDirection == Vector2.zero ? Vector2.right : patrolDirection.normalized) * patrolDistance;
        patrolTarget = pointB;
        CurrentDirection = (patrolTarget - pointA).normalized;
        lastDirection = CurrentDirection;
    }

    public Vector2 UpdateBehavior(Vector2 bodyPos, Vector2 feetPos, bool isGrounded, bool canJump, ref bool hasJumped, Action jumpAction)
    {
        if (waiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                waiting = false;
                patrolTarget = (patrolTarget == pointA) ? pointB : pointA;

                Vector2 newDirection = patrolTarget - (Vector2)transform.position;
                newDirection.y = 0f;
                CurrentDirection = newDirection.normalized;
                lastDirection = CurrentDirection;

                Debug.Log("Patrol flipped. New direction: " + CurrentDirection);
            }

            return Vector2.zero;
        }

        Vector2 direction = patrolTarget - feetPos;
        direction.y = 0f;
        direction.Normalize();

        if (direction != Vector2.zero)
            lastDirection = direction;

        bool shouldJump = detector.ShouldJump(direction, feetPos);

        if (isGrounded && shouldJump && !hasJumped && canJump)
        {
            jumpAction.Invoke();
            hasJumped = true;
            return Vector2.zero;
        }

        if (isGrounded && !shouldJump)
            hasJumped = false;

        float horizontalDistance = Mathf.Abs(patrolTarget.x - feetPos.x);
        if (horizontalDistance < arrivalThreshold && isGrounded)
        {
            transform.position = new Vector3(patrolTarget.x, transform.position.y, transform.position.z);

            waiting = true;
            waitTimer = waitTime;
            return Vector2.zero;
        }

        return direction;
    }
}
