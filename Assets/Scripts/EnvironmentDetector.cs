using UnityEngine;

public class EnvironmentDetector : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float obstacleRayLength = 1f;
    [SerializeField] private float ledgeRayLength = 1f;

    [SerializeField] private Vector2 obstacleRayOffset = new(0.3f, 0.5f);
    [SerializeField] private Vector2 ledgeRayOffset = new(0.3f, 0f);

    [Header("Detection Settings")]
    [SerializeField] private Vector2 detectionRayOffset = new(0.5f, 0.5f);
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private LayerMask targetLayer;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Target Memory Settings")]
    [SerializeField] private float loseInterestDelay = 2f;

    private Transform lastSeenTarget;
    private float loseTimer;

    public bool IsGrounded()
    {
        Vector2 origin = (Vector2)transform.position + Vector2.down * 0.1f;
        Vector2 size = new(0.4f, 0.05f);
        return Physics2D.OverlapBox(origin, size, 0f, groundLayer);
    }

    public bool ShouldJump(Vector2 direction)
    {
        float dirX = Mathf.Sign(direction.x);
        if (dirX == 0) dirX = 1f;

        Vector2 obstacleOrigin = (Vector2)transform.position + new Vector2(obstacleRayOffset.x * dirX, obstacleRayOffset.y);
        Vector2 ledgeOrigin = (Vector2)transform.position + new Vector2(ledgeRayOffset.x * dirX, ledgeRayOffset.y);

        RaycastHit2D hitObstacle = Physics2D.Raycast(obstacleOrigin, Vector2.right * dirX, obstacleRayLength, obstacleLayer);
        RaycastHit2D hitLedge = Physics2D.Raycast(ledgeOrigin, Vector2.down, ledgeRayLength, groundLayer);

        bool obstacleDetected = hitObstacle.collider != null;
        bool ledgeDetected = hitLedge.collider == null;

        Debug.DrawRay(obstacleOrigin, Vector2.right * dirX * obstacleRayLength, Color.magenta);
        Debug.DrawRay(ledgeOrigin, Vector2.down * ledgeRayLength, Color.cyan);

        return obstacleDetected || ledgeDetected;
    }

    public Transform GetTarget(Vector2 fallbackDirection)
    {
        Vector2 origin = (Vector2)transform.position + detectionRayOffset;
        Vector2 direction;

        if (lastSeenTarget != null)
        {
            direction = ((Vector2)lastSeenTarget.position - origin).normalized;
        }
        else
        {
            direction = fallbackDirection.normalized;
        }

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, detectionRange, targetLayer);
        Debug.DrawRay(origin, direction * detectionRange, Color.red);

        if (hit.collider != null)
        {
            lastSeenTarget = hit.transform;
            loseTimer = loseInterestDelay;
        }
        else
        {
            loseTimer -= Time.deltaTime;
            if (loseTimer <= 0f)
                lastSeenTarget = null;
        }

        return lastSeenTarget;
    }

    private void OnDrawGizmosSelected()
    {
        float dirX = transform.localScale.x >= 0 ? 1f : -1f;

        Vector2 obstacleOrigin = (Vector2)transform.position + new Vector2(obstacleRayOffset.x * dirX, obstacleRayOffset.y);
        Vector2 ledgeOrigin = (Vector2)transform.position + new Vector2(ledgeRayOffset.x * dirX, ledgeRayOffset.y);
        Vector2 detectOrigin = (Vector2)transform.position + detectionRayOffset;

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(obstacleOrigin, obstacleOrigin + Vector2.right * dirX * obstacleRayLength);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(ledgeOrigin, ledgeOrigin + Vector2.down * ledgeRayLength);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(detectOrigin, detectOrigin + Vector2.right * detectionRange);
    }
}
