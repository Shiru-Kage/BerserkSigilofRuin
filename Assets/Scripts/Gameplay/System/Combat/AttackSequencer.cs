using UnityEngine;
using System.Collections;

public class AttackSequencer : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip[] attackAnimations;

    [Header("Attack Timings")]
    [SerializeField] private float[] attackDurations;

    private int currentAttackIndex = 0;
    private bool isAttacking = false;

    private void Start()
    {
        if (attackAnimations.Length != attackDurations.Length)
        {
            Debug.LogError("Attack animations and durations arrays must be of the same length.");
        }
    }

    public void StartAttack()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            currentAttackIndex = 0; 
            StartCoroutine(PlayAttackSequence());
        }
    }

    private IEnumerator PlayAttackSequence()
    {
        while (currentAttackIndex < attackAnimations.Length)
        {
            animator.Play(attackAnimations[currentAttackIndex].name);

            yield return new WaitForSeconds(attackDurations[currentAttackIndex]);
            currentAttackIndex++;
        }
        isAttacking = false;
    }
}
