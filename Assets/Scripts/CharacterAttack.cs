using UnityEngine;

public class CharacterAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private Vector2 attackOffset = new Vector2(0.5f, 0f);
    [SerializeField] private LayerMask targetLayers;

    private float lastAttackTime;
    private SpriteRenderer spriteRenderer;
    private ICharacterAnimatorData characterData;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            Debug.LogWarning("CharacterAttack: SpriteRenderer not found. Flip detection won't work.");

        characterData = GetComponent<ICharacterAnimatorData>();
        if (characterData != null)
            characterData.OnAttack += TryAttack;
    }

    public void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        PerformAttack();
        lastAttackTime = Time.time;
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
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            Vector2 previewPos = (Vector2)transform.position + attackOffset;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(previewPos, attackRange);
        }
        else
        {
            Vector2 direction = spriteRenderer != null && spriteRenderer.flipX ? Vector2.left : Vector2.right;
            Vector2 attackPos = (Vector2)transform.position + new Vector2(attackOffset.x * direction.x, attackOffset.y);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPos, attackRange);
        }
    }
}
