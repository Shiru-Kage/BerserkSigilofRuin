using UnityEngine;
using System;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public Animator Animator => animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Animation Data Source")]
    [SerializeField] private MonoBehaviour characterSource;

    private ICharacterAnimatorData data;
    private static readonly int attack1Hash = Animator.StringToHash("Attack1");
    private static readonly int attack2Hash = Animator.StringToHash("Attack2");
    private static readonly int attack3Hash = Animator.StringToHash("Attack3");

    private int comboIndex = 1;
    private Action onAttackAction;

    private void Awake()
    {
        data = characterSource as ICharacterAnimatorData;

        if (data != null)
        {
            onAttackAction = () =>
            {
                TriggerComboAttack();
            };
            data.OnAttack += onAttackAction;
        }
        else
        {
            Debug.LogError($"{characterSource} does not implement ICharacterAnimatorData.");
        }
    }

    private void OnDestroy()
    {
        if (data != null && onAttackAction != null)
        {
            data.OnAttack -= onAttackAction;
        }
    }

    private void Update()
    {
        if (data == null) return;

        Vector2 move = data.MoveInput;
        Vector2 velocity = data.Velocity;
        bool grounded = data.IsGrounded;

        if (move.x != 0)
            spriteRenderer.flipX = move.x < 0;

        animator.SetFloat("MoveX", Mathf.Abs(move.x));
        animator.SetFloat("YVelocity", velocity.y);
        animator.SetBool("IsGrounded", grounded);
    }

    public void TriggerComboAttack()
    {
        if (!animator.HasParameter(attack2Hash) || !animator.HasParameter(attack3Hash))
        {
            animator.SetTrigger(attack1Hash);
            return;
        }


        switch (comboIndex)
        {
            case 1:
                animator.SetTrigger(attack1Hash);
                break;
            case 2:
                animator.SetTrigger(attack2Hash);
                break;
            case 3:
                animator.SetTrigger(attack3Hash);
                break;
            default:
                animator.SetTrigger(attack1Hash);
                break;
        }

        comboIndex++;
        if (comboIndex > 3)
            comboIndex = 1;
    }

    public void TriggerDeath()
    {
        animator.SetTrigger("Dead");
    }

    public void SetCharacterSource(ICharacterAnimatorData newSource)
    {
        if (data != null && onAttackAction != null)
            data.OnAttack -= onAttackAction;

        data = newSource;

        if (data != null)
        {
            onAttackAction = () =>
            {
                TriggerComboAttack();
            };
            data.OnAttack += onAttackAction;
        }
    }
}
public static class AnimatorExtensions
{
    public static bool HasParameter(this Animator animator, int hash)
    {
        foreach (var param in animator.parameters)
        {
            if (param.nameHash == hash)
                return true;
        }
        return false;
    }
}
