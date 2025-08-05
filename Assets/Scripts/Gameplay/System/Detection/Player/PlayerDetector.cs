using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Offset from this transform's position where the detection ray starts.")]
    [SerializeField] private Vector2 detectionRayOffset = new(0.5f, 0.5f);
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private float loseInterestDelay = 2f;

    private Transform lastSeenTarget;
    private float loseTimer;

    public Transform GetTarget(Vector2 fallbackDirection)
    {
        Vector2 origin = (Vector2)transform.position + detectionRayOffset;
        Vector2 direction = lastSeenTarget != null
            ? ((Vector2)lastSeenTarget.position - origin).normalized
            : fallbackDirection.normalized;

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
            {
                lastSeenTarget = null;
            }
        }

        return lastSeenTarget;
    }

    public void ForgetTarget()
    {
        lastSeenTarget = null;
        loseTimer = 0f;
    }

    public Transform FindClosestTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, targetLayer);
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (var hit in hits)
        {
            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = hit.transform;
            }
        }

        return closest;
    }

    public bool HasTarget => lastSeenTarget != null;

    public Transform LastSeenTarget => lastSeenTarget;

    private void OnDrawGizmosSelected()
    {
        Vector2 origin = (Vector2)transform.position + detectionRayOffset;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + Vector2.right * detectionRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
