using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Animation Data Source")]
    [SerializeField] private MonoBehaviour characterSource;

    private ICharacterAnimatorData data;
    private AttackSequencer comboSystem; 
    private int lastAttackHash = Animator.StringToHash("Attack1"); 

    private void Awake()
    {
        data = characterSource as ICharacterAnimatorData;
        comboSystem = GetComponent<AttackSequencer>(); 

        if (data != null)
        {
            data.OnAttack += TriggerComboAttack;
        }
        else
        {
            Debug.LogError($"{characterSource} does not implement ICharacterAnimatorData.");
        }

        if (comboSystem == null)
        {
            Debug.Log("No AttackSequencer found. Defaulting to Attack1.");
            animator.SetTrigger("Attack1");
        }
    }

    private void OnDestroy()
    {
        if (data != null)
        {
            data.OnAttack -= TriggerComboAttack;
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
        if (comboSystem == null)
        {
            animator.SetTrigger("Attack1");
            return;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Attack1") && !stateInfo.IsName("Attack1") && !stateInfo.IsName("Attack2") && !stateInfo.IsName("Attack3"))
        {
            Debug.Log("Animation not yet finished. Waiting before next attack.");
            return;
        }

        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");
        animator.ResetTrigger("Attack3");

        int comboCount = comboSystem.GetCurrentComboCount(); 

        switch (comboCount)
        {
            case 1:
                animator.SetTrigger("Attack1");
                lastAttackHash = Animator.StringToHash("Attack1");
                break;
            case 2:
                animator.SetTrigger("Attack2");
                lastAttackHash = Animator.StringToHash("Attack2");
                break;
            case 3:
                animator.SetTrigger("Attack3");
                lastAttackHash = Animator.StringToHash("Attack3");
                break;
            default:
                break;
        }

        if (comboCount == 3)
        {
            comboSystem.ResetComboImmediately(); 
        }
    }

    public void TriggerDeath()
    {
        animator.SetTrigger("Dead");
    }
}
