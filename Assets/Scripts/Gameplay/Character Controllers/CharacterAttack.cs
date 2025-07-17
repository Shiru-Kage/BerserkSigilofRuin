using UnityEngine;
using System.Collections;

public class CharacterAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float attackDelay = 0.25f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private Vector2 attackOffset = new Vector2(0.5f, 0f);
    [SerializeField] private LayerMask targetLayers;

    public float LastAttackTime => lastAttackTime;
    public float AttackCooldown => attackCooldown;

    private float lastAttackTime;
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

        if (characterData != null)
            characterData.OnAttack += TryAttack;
    }

    public void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown)
        {
            return;
        }

        lastAttackTime = Time.time;
        StartCoroutine(DelayedAttack());
    }

    private IEnumerator DelayedAttack()
    {
        yield return new WaitForSeconds(attackDelay);
        PerformAttack();
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

    public bool CanAttack()
    {
        return Time.time - lastAttackTime >= attackCooldown;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 direction = Application.isPlaying && spriteRenderer != null && spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Vector2 attackPos = (Vector2)transform.position + new Vector2(attackOffset.x * direction.x, attackOffset.y);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos, attackRange);
    }
}
