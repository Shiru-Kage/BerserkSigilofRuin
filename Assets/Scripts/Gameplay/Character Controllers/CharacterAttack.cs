using UnityEngine;
using UnityEngine.Events;
using System.Collections;


public class CharacterAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private Vector2 attackOffset = new Vector2(0.5f, 0f);
    [SerializeField] private LayerMask targetLayers;
    private SpriteRenderer spriteRenderer;
    private ICharacterAnimatorData characterData;
    private InCombatTracker combatTracker;
    private RageSystem rageSystem;

    public UnityEvent<string> OnTargetLayerChanged;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        combatTracker = GetComponent<InCombatTracker>();
        rageSystem = GetComponent<RageSystem>();
        characterData = GetComponent<ICharacterAnimatorData>();
    }

    public void AddTargetLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);

        if (layer != -1)
        {
            targetLayers |= (1 << layer);
            Debug.Log("Layer added: " + layerName);
        }
        else
        {
            Debug.LogError("Invalid Layer Name: " + layerName);
        }
    }

    public void RemoveTargetLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName); 

        if (layer != -1) 
        {
            targetLayers &= ~(1 << layer);
            Debug.Log("Layer removed: " + layerName);
        }
        else
        {
            Debug.LogError("Invalid Layer Name: " + layerName);
        }
    }

    private void PerformAttack()
    {
        if (characterStats == null) return;

        int attackDamage = characterStats.GetCharacterData().attack.GetValue();

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
        Invoke("ResetAttackState", 0.1f);
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