using UnityEngine;
using System.Collections;

public class CharacterAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private Vector2 attackOffset = new Vector2(0.5f, 0f);
    [SerializeField] private LayerMask targetLayers;
    private SpriteRenderer spriteRenderer;
    private ICharacterAnimatorData characterData;
    private InCombatTracker combatTracker;
    private RageSystem rageSystem;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        combatTracker = GetComponent<InCombatTracker>();
        rageSystem = GetComponent<RageSystem>();
        characterData = GetComponent<ICharacterAnimatorData>();
    }

    private void PerformAttack()
    {
        Vector2 direction = spriteRenderer != null && spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Vector2 attackPos = (Vector2)transform.position + new Vector2(attackOffset.x * direction.x, attackOffset.y);

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, attackRange, targetLayers);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out Health health))
            {
                health.TakeDamage(attackDamage);

                if (combatTracker != null)
                {
                    combatTracker.NotifyCombatActivity();
                }

                if (rageSystem != null)
                {
                    rageSystem.AddRageFromHit();
                }
            }
            else
            {
                Debug.Log($"PerformAttack: hit {hit.name} has no Health component.");
            }
        }
    }

    public bool IsTargetInRange(Transform target)
    {
        if (target == null) return false;

        Vector2 direction = spriteRenderer != null && spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Vector2 attackPos = (Vector2)transform.position + new Vector2(attackOffset.x * direction.x, attackOffset.y);

        float distance = Vector2.Distance(attackPos, target.position);
        return distance <= attackRange;
    }

    public void ForceAttackNow()
    {
        PerformAttack();
    }


    
    private void OnDrawGizmosSelected()
    {
        Vector2 direction = Application.isPlaying && spriteRenderer != null && spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Vector2 attackPos = (Vector2)transform.position + new Vector2(attackOffset.x * direction.x, attackOffset.y);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos, attackRange);
    }
}
