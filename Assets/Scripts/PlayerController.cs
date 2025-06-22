using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private Vector2 groundCheckOffset = Vector2.zero;

    private Rigidbody2D rb;
    private Collider2D col;
    private InputSystem_Actions input;

    public Vector2 MoveInput { get; private set; }
    public bool IsGrounded { get; private set; }
    public Vector2 Velocity => rb.velocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        input = InputManager.GetInputActions();
        if (input == null)
        {
            Debug.LogError("InputManager not initialized before PlayerController.");
            enabled = false;
            return;
        }

        input.Player.Jump.performed += HandleJump;
    }

    private void OnDestroy()
    {
        input.Player.Jump.performed -= HandleJump;
    }

    private void Update()
    {
        MoveInput = input.Player.Move.ReadValue<Vector2>();
        IsGrounded = CheckGrounded();
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(MoveInput.x * moveSpeed, rb.velocity.y);
    }

    private void HandleJump(InputAction.CallbackContext context)
    {
        if (IsGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    private bool CheckGrounded()
    {
        Vector2 origin = (Vector2)transform.position + groundCheckOffset;
        return Physics2D.OverlapBox(origin, groundCheckSize, 0f, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 origin = (Vector2)transform.position + groundCheckOffset;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(origin, groundCheckSize);
    }
}

