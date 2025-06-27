using UnityEngine;

public class EnvironmentDetector : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float obstacleRayLength = 1f;
    [SerializeField] private float ledgeRayLength = 1f;

    [SerializeField] private Vector2 obstacleRayOffset = new(0.3f, 0.5f);
    [SerializeField] private Vector2 ledgeRayOffset = new(0.3f, 0.1f);
    

    [Header("Obstacle Ray Settings")]
    [Tooltip("How many horizontal rays are cast upwards to detect obstacles.")]
    [SerializeField, Range(1, 10)] private int obstacleRayCount = 3;

    [Tooltip("Vertical spacing between each horizontal obstacle ray.")]
    [SerializeField] private float obstacleRaySpacing = 0.25f;

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

    public bool IsGrounded(Vector2 feetPosition)
    {
        Vector2 size = new(0.4f, 0.05f);
        return Physics2D.OverlapBox(feetPosition, size, 0f, groundLayer);
    }

    public bool ShouldJump(Vector2 direction, Vector2 feetPosition)
    {
        float dirX = Mathf.Sign(direction.x);
        if (dirX == 0) dirX = 1f;

        bool obstacleDetected = false;

        // Multi-ray obstacle detection (from feet upward)
        for (int i = 0; i < obstacleRayCount; i++)
        {
            Vector2 rayOrigin = feetPosition + new Vector2(obstacleRayOffset.x * dirX, obstacleRayOffset.y + i * obstacleRaySpacing);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, obstacleRayLength, obstacleLayer);
            Debug.DrawRay(rayOrigin, Vector2.right * dirX * obstacleRayLength, Color.magenta);

            if (hit.collider != null)
            {
                obstacleDetected = true;
                break;
            }
        }

        // Ledge detection (unchanged)
        Vector2 ledgeOrigin = feetPosition + new Vector2(ledgeRayOffset.x * dirX, ledgeRayOffset.y);
        RaycastHit2D hitLedge = Physics2D.Raycast(ledgeOrigin, Vector2.down, ledgeRayLength, groundLayer);
        bool ledgeDetected = hitLedge.collider == null;
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

        Vector2 feetPosition = (Vector2)transform.position + Vector2.down * GetComponent<Collider2D>().bounds.extents.y;
        Vector2 detectOrigin = (Vector2)transform.position + detectionRayOffset;

        // Draw multiple horizontal obstacle rays
        for (int i = 0; i < obstacleRayCount; i++)
        {
            Vector2 rayOrigin = feetPosition + new Vector2(obstacleRayOffset.x * dirX, obstacleRayOffset.y + i * obstacleRaySpacing);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.right * dirX * obstacleRayLength);
        }

        // Ledge detection ray
        Vector2 ledgeOrigin = feetPosition + new Vector2(ledgeRayOffset.x * dirX, ledgeRayOffset.y);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(ledgeOrigin, ledgeOrigin + Vector2.down * ledgeRayLength);

        // Target detection ray
        Gizmos.color = Color.red;
        Gizmos.DrawLine(detectOrigin, detectOrigin + Vector2.right * detectionRange);
    }
}
