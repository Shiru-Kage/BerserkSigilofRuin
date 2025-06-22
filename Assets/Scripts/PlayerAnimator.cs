using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController playerController;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Vector2 move = playerController.MoveInput;
        Vector2 velocity = playerController.Velocity;
        bool isGrounded = playerController.IsGrounded;

        if (move.x != 0)
            spriteRenderer.flipX = move.x < 0;

        animator.SetFloat("MoveX", Mathf.Abs(move.x));
        animator.SetFloat("YVelocity", velocity.y);
        animator.SetBool("IsGrounded", isGrounded);
    }
}
