using UnityEngine;

public class EnvironmentDetector : MonoBehaviour
{
    [Header("Obstacle Detection Settings")]
    [SerializeField] private float obstacleRayLength = 1f;
    [SerializeField] private Vector2 obstacleRayOffset = new(0.3f, 0.5f);
    [SerializeField, Range(1, 10)] private int obstacleRayCount = 3;
    [SerializeField] private float obstacleRaySpacing = 0.25f;

    [Header("Ledge Detection Settings")]
    [SerializeField] private float ledgeRayLength = 1f;
    [SerializeField] private Vector2 ledgeRayOffset = new(0.3f, 0.1f);

    [Header("Environment Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask obstacleLayer;

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

        Vector2 ledgeOrigin = feetPosition + new Vector2(ledgeRayOffset.x * dirX, ledgeRayOffset.y);
        RaycastHit2D hitLedge = Physics2D.Raycast(ledgeOrigin, Vector2.down, ledgeRayLength, groundLayer);
        Debug.DrawRay(ledgeOrigin, Vector2.down * ledgeRayLength, Color.cyan);

        bool ledgeDetected = hitLedge.collider == null || hitLedge.distance > ledgeRayLength * 0.9f;

        return obstacleDetected || ledgeDetected;
    }

    public bool IsObstacleInFront(Vector2 direction, Vector2 feetPosition)
    {
        float dirX = Mathf.Sign(direction.x);
        if (dirX == 0) dirX = 1f;

        for (int i = 0; i < obstacleRayCount; i++)
        {
            Vector2 rayOrigin = feetPosition + new Vector2(obstacleRayOffset.x * dirX, obstacleRayOffset.y + i * obstacleRaySpacing);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, obstacleRayLength, obstacleLayer);
            Debug.DrawRay(rayOrigin, Vector2.right * dirX * obstacleRayLength, Color.green);

            if (hit.collider != null)
                return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        float dirX = transform.localScale.x >= 0 ? 1f : -1f;
        Vector2 feetPosition = (Vector2)transform.position + Vector2.down * GetComponent<Collider2D>().bounds.extents.y;

        for (int i = 0; i < obstacleRayCount; i++)
        {
            Vector2 rayOrigin = feetPosition + new Vector2(obstacleRayOffset.x * dirX, obstacleRayOffset.y + i * obstacleRaySpacing);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.right * dirX * obstacleRayLength);
        }

        Vector2 ledgeOrigin = feetPosition + new Vector2(ledgeRayOffset.x * dirX, ledgeRayOffset.y);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(ledgeOrigin, ledgeOrigin + Vector2.down * ledgeRayLength);
    }
}
