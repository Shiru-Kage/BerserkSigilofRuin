using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerController : MonoBehaviour, ICharacterAnimatorData
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float dashSpeed = 15f; 
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Ground Check Settings")]
    [SerializeField] private Vector2 groundCheckOffset = new(0f, -0.6f);
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Attack Cooldown Settings")]
    [SerializeField] private float baseAttackCooldown = 0.5f; 
    private float attackCooldownTimer = 0f; 
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f; 
    private bool isInBarrageMode = false;

    private Rigidbody2D rb;
    private Collider2D col;
    private InputSystem_Actions input;
    private CharacterAttack characterAttack;
    private AttackSequencer comboSystem;
    private CharacterStats characterStats;
    private CharacterAnimator characterAnimator; 

    public Vector2 MoveInput { get; private set; }
    public bool IsGrounded { get; private set; }
    public Vector2 Velocity => rb.velocity;
    public Vector2 FeetPosition => (Vector2)transform.position + groundCheckOffset;
    public event Action OnAttack;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        characterAttack = GetComponent<CharacterAttack>();
        comboSystem = GetComponent<AttackSequencer>();
        characterStats = GetComponent<CharacterStats>();
        characterAnimator = GetComponent<CharacterAnimator>();

        input = InputManager.GetInputActions();
        if (input == null)
        {
            Debug.LogError("InputManager not initialized before PlayerController.");
            enabled = false;
            return;
        }

        input.Player.Attack.performed += HandleAttack;
        input.Player.Jump.performed += HandleJump;
        input.Player.Dash.performed += HandleDash;

        comboSystem.OnComboAttack += TriggerComboAttack;

        input.Enable();
    }

    private void OnDestroy()
    {
        input.Player.Jump.performed -= HandleJump;
        input.Player.Attack.performed -= HandleAttack;
        input.Player.Dash.performed -= HandleDash;

        comboSystem.OnComboAttack -= TriggerComboAttack;
    }

    private void Update()
    {
        MoveInput = input.Player.Move.ReadValue<Vector2>();
        IsGrounded = CheckGrounded();

        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        if (dashCooldownTimer > 0f) 
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isInBarrageMode)
        {
            if (!isDashing)
            {
                rb.velocity = new Vector2(MoveInput.x * moveSpeed, rb.velocity.y);
            }
            else
            {
                if (MoveInput != Vector2.zero)
                {
                    Vector2 dashDirection = new Vector2(MoveInput.x, MoveInput.y).normalized;
                    rb.velocity = dashDirection * dashSpeed;
                }
                else
                {
                    Vector2 dashDirection = characterAnimator.IsFacingLeft() ? Vector2.left : Vector2.right;
                    rb.velocity = dashDirection * dashSpeed;
                }
            }
        }
    }

    public void HandleAttack(InputAction.CallbackContext context)
    {
        if (attackCooldownTimer <= 0f)
        {
            comboSystem.HandleAttack();
            attackCooldownTimer = CalculateAttackCooldown();
        }
    }

    private void TriggerComboAttack(int comboIndex)
    {
        OnAttack?.Invoke();
    }

    private void HandleJump(InputAction.CallbackContext context)
    {
        if (!isInBarrageMode && IsGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    private void HandleDash(InputAction.CallbackContext context)
    {
        if (dashCooldownTimer <= 0f)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;  

            if (MoveInput != Vector2.zero)
            {
                Vector2 dashDirection = new Vector2(MoveInput.x, MoveInput.y).normalized;
                rb.velocity = dashDirection * dashSpeed;
            }
            else
            {
                Vector2 dashDirection = characterAnimator.IsFacingLeft() ? Vector2.left : Vector2.right;
                rb.velocity = dashDirection * dashSpeed;
            }
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

    private float CalculateAttackCooldown()
    {
        float attackSpeed = characterStats.GetCharacterData().atkSpeed.GetValue();

        float cooldown = baseAttackCooldown / (1f + (attackSpeed / 100f));  
        return Mathf.Max(cooldown, 0.1f);
    }

    public void OnCharacterDeath()
    {
        input.Disable();
        rb.velocity = Vector2.zero;
    }

    public void SetAIMovementOverride(bool isBarrageMode)
    {
        isInBarrageMode = isBarrageMode;

        if (isInBarrageMode)
            input.Disable();
        else
            input.Enable();
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
    public int CurrentComboCount => comboSystem != null ? comboSystem.CurrentComboCount : 1;
}
