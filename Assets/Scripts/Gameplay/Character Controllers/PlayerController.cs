using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerController : MonoBehaviour, ICharacterAnimatorData
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check Settings")]
    [SerializeField] private Vector2 groundCheckOffset = new(0f, -0.6f);
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private Collider2D col;
    private InputSystem_Actions input;
    private CharacterAttack characterAttack;
    private AttackSequencer comboSystem;

    public Vector2 MoveInput { get; private set; }
    public bool IsGrounded { get; private set; }
    public Vector2 Velocity => rb.velocity;
    public event Action OnAttack;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        characterAttack = GetComponent<CharacterAttack>();
        comboSystem = GetComponent<AttackSequencer>();

        input = InputManager.GetInputActions();
        if (input == null)
        {
            Debug.LogError("InputManager not initialized before PlayerController.");
            enabled = false;
            return;
        }

        input.Player.Attack.performed += HandleAttack;
        input.Player.Jump.performed += HandleJump;

        comboSystem.OnComboAttack += TriggerComboAttack;
    }

    private void OnDestroy()
    {
        input.Player.Jump.performed -= HandleJump;
        input.Player.Attack.performed -= HandleAttack;

        comboSystem.OnComboAttack -= TriggerComboAttack;
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

    public void HandleAttack(InputAction.CallbackContext context)
    {
        comboSystem.HandleAttack();
    }

    private void TriggerComboAttack()
    {
        OnAttack?.Invoke();
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
        Vector2 direction = Vector2.down; 

        float distance = groundCheckRadius; 

        RaycastHit2D hitLeft = Physics2D.Raycast(origin + Vector2.left * 0.5f, direction, distance, groundLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(origin + Vector2.right * 0.5f, direction, distance, groundLayer);

        return hitLeft.collider != null || hitRight.collider != null;
    }

    public void OnCharacterDeath()
    {
        input.Disable();
        rb.velocity = Vector2.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 origin = (Vector2)transform.position + groundCheckOffset;
        Vector2 direction = Vector2.down;
        float distance = groundCheckRadius; 

        Gizmos.color = Color.yellow;

        Vector2 leftRayOrigin = origin + Vector2.left * 0.5f;
        Gizmos.DrawLine(leftRayOrigin, leftRayOrigin + direction * distance);

        Vector2 rightRayOrigin = origin + Vector2.right * 0.5f;
        Gizmos.DrawLine(rightRayOrigin, rightRayOrigin + direction * distance);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, origin + direction * distance);
    }
}
