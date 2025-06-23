using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Animation Data Source")]
    [SerializeField] private MonoBehaviour characterSource;

    private ICharacterAnimatorData data;

    private void Awake()
    {
        data = characterSource as ICharacterAnimatorData;

        if (data != null)
            data.OnAttack += TriggerAttack;
        else
            Debug.LogError($"{characterSource} does not implement ICharacterAnimatorData.");
    }

    private void OnDestroy()
    {
        if (data != null)
            data.OnAttack -= TriggerAttack;
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

    private void TriggerAttack()
    {
        animator.SetTrigger("Attack");
    }
}
