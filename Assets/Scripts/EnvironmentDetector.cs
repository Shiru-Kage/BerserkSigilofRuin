using UnityEngine;

public class EnvironmentDetector : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float obstacleRayLength = 1f;
    [SerializeField] private float ledgeRayLength = 1f;
    [SerializeField] private Vector2 obstacleRayOffset = new(0.3f, 0.5f);
    [SerializeField] private Vector2 ledgeRayOffset = new(0.3f, 0f);

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask obstacleLayer;

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

    private void OnDrawGizmosSelected()
    {
        float dirX = transform.localScale.x >= 0 ? 1f : -1f;

        Vector2 obstacleOffset = new Vector2(obstacleRayOffset.x * dirX, obstacleRayOffset.y);
        Vector2 ledgeOffset = new Vector2(ledgeRayOffset.x * dirX, ledgeRayOffset.y);

        Vector2 obstacleOrigin = (Vector2)transform.position + obstacleOffset;
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(obstacleOrigin, obstacleOrigin + Vector2.right * dirX * obstacleRayLength);

        Vector2 ledgeOrigin = (Vector2)transform.position + ledgeOffset;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(ledgeOrigin, ledgeOrigin + Vector2.down * ledgeRayLength);
    }
}
