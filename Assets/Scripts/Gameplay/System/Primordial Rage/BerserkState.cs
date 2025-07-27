using System.Collections;
using UnityEngine;

public class BerserkState : MonoBehaviour
{
    [Header("Berserk State Settings")]
    [SerializeField] private float attackIntervalFactor = 1.5f;
    [SerializeField] private float attackCooldown = 0.5f;
    private bool isInBerserkState = false;
    private bool canAttack = true;

    private CharacterAttack characterAttack;
    private RageSystem rageSystem;
    private AttackSequencer comboSystem; 

    private void Awake()
    {
        characterAttack = GetComponent<CharacterAttack>();
        rageSystem = GameObject.FindAnyObjectByType<RageSystem>();

        comboSystem = GetComponent<AttackSequencer>(); 
    }

    private void Update()
    {
        if (isInBerserkState)
        {
            if (rageSystem.CurrentRage <= 0)
            {
                EndBerserkState();
            }

            if (canAttack)
            {
                StartCoroutine(RandomAttackInterval());
            }
        }
    }
    public void EnterBerserkState()
    {
        isInBerserkState = true;
    }

    private void EndBerserkState()
    {
        isInBerserkState = false;
        Debug.Log("Berserk State ended due to Rage Bar depletion.");
    }

    private IEnumerator RandomAttackInterval()
    {
        canAttack = false;

        float randomInterval = Random.Range(1f, attackIntervalFactor);

        yield return new WaitForSeconds(randomInterval);

        TriggerRandomAttack();

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    private void TriggerRandomAttack()
    {
        if (comboSystem != null)
        {
            comboSystem.HandleAttack();
        }

        if (characterAttack != null)
        {
            characterAttack.TryAttack(); 
        }
    }
}
