using System;
using UnityEngine;

public class BerserkState : MonoBehaviour, ICharacterAnimatorData
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField, Range(0f, 1f)] private float barrageTriggerChance = 0.3f;
    [SerializeField] private float barrageCheckCooldown = 2f;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer; 

    private Rigidbody2D rb;
    private PlayerController playerController;
    private ChaseBehavior chaseBehavior;
    private EnvironmentDetector detector;
    private CharacterAttack characterAttack;
    private PlayerDetector playerDetector;
    private AttackSequencer comboSystem;
    private CharacterAnimator animator;
    private StuckHandler stuckHandler;

    private Transform currentTarget;
    private bool isInBerserkState;
    private bool hasJumped;
    private bool isInBarrageMode;
    private bool waitingForBarrageAnimation = false;

    private float barrageCooldownTimer = 0f;
    private static readonly int barrageHash = Animator.StringToHash("Barrage");

    public Vector2 MoveInput { get; private set; } = Vector2.zero;
    public Vector2 Velocity => rb.velocity;
    public bool IsGrounded => playerController.IsGrounded;
    public event Action OnAttack;

    private Action playerAttackForwarder;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        characterAttack = GetComponent<CharacterAttack>();
        playerController = GetComponent<PlayerController>();
        detector = GetComponent<EnvironmentDetector>();
        playerDetector = GetComponent<PlayerDetector>();
        comboSystem = GetComponent<AttackSequencer>();
        animator = GetComponentInChildren<CharacterAnimator>();
        stuckHandler = new StuckHandler(detector);

        if (!characterAttack || !detector || !playerDetector || !playerController || !animator)
        {
            Debug.LogError("Missing required components.");
            enabled = false;
            return;
        }

        chaseBehavior = new ChaseBehavior(detector, characterAttack, 1.4f);
        chaseBehavior.OnAttack += HandleAttack;

        playerAttackForwarder = () =>
        {
            if (!isInBarrageMode)
                OnAttack?.Invoke();
        };
        playerController.OnAttack += playerAttackForwarder;
    }

    private void OnDestroy()
    {
        if (chaseBehavior != null)
            chaseBehavior.OnAttack -= HandleAttack;

        if (playerController != null && playerAttackForwarder != null)
            playerController.OnAttack -= playerAttackForwarder;
    }

    private void FixedUpdate()
    {
        if (!isInBerserkState) return;

        if (isInBarrageMode)
        {
            if (currentTarget == null)
            {
                ExitBarrageMode();
                return;
            }

            if (waitingForBarrageAnimation)
            {
                Animator anim = animator.Animator;
                if (anim != null)
                {
                    AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
                    if (anim.IsInTransition(0) || stateInfo.IsName("Barrage"))
                    {
                        rb.velocity = new Vector2(0, rb.velocity.y);
                        return;
                    }
                    else
                    {
                        waitingForBarrageAnimation = false;
                        ClearRedGlow();
                        Debug.Log("[BerserkState] Barrage animation finished. Beginning chase.");
                    }
                }
            }

            Vector2 moveDir = chaseBehavior.UpdateBehavior(
                currentTarget.position,
                transform.position,
                IsGrounded,
                canJump: false,
                ref hasJumped,
                jumpAction: null
            );

            MoveInput = moveDir;
            rb.velocity = new Vector2(Mathf.Clamp(moveDir.x, -1f, 1f) * moveSpeed, rb.velocity.y);

            Vector2 forward = (currentTarget.position - transform.position).normalized;
            stuckHandler.Update(transform.position, IsGrounded, forward, playerController.FeetPosition, true, PerformJump);

            if (characterAttack.IsTargetInRange(currentTarget))
            {
                OnAttack?.Invoke(); 
                ExitBarrageMode(); 
            }
        }
        else
        {
            MoveInput = playerController.MoveInput;
            rb.velocity = new Vector2(MoveInput.x * moveSpeed, rb.velocity.y);

            barrageCooldownTimer -= Time.fixedDeltaTime;
            if (barrageCooldownTimer <= 0f)
            {
                float roll = UnityEngine.Random.value;
                Debug.Log($"[BerserkState] Barrage chance roll: {roll} < {barrageTriggerChance}");

                if (roll < barrageTriggerChance)
                {
                    TryEnterBarrageMode();
                }

                barrageCooldownTimer = barrageCheckCooldown;
            }
        }
    }

    private void PerformJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        hasJumped = true;
    }

    private void HandleAttack()
    {
        OnAttack?.Invoke();
    }

    private void TryEnterBarrageMode()
    {
        currentTarget = playerDetector.FindClosestTarget();

        if (currentTarget == null)
            return;

        isInBarrageMode = true;
        waitingForBarrageAnimation = true;
        playerController.SetAIMovementOverride(true);
        chaseBehavior.SetTarget(currentTarget);
        hasJumped = false;

        if (animator != null && animator.Animator != null)
        {
            animator.Animator.SetTrigger(barrageHash);
            Debug.Log("[BerserkState] Barrage animation triggered.");
            ApplyRedGlow();
        }
    }

    private void ExitBarrageMode()
    {
        isInBarrageMode = false;
        waitingForBarrageAnimation = false;
        currentTarget = null;
        chaseBehavior.ClearTarget();
        playerController.SetAIMovementOverride(false);

        ClearRedGlow();
        Debug.Log("[BerserkState] Exited barrage mode.");
    }

    public void EnterBerserk()
    {
        isInBerserkState = true;
        animator.SetCharacterSource(this);
        playerController.SetAIMovementOverride(false);
        barrageCooldownTimer = 0f;
    }

    public void ExitBerserk()
    {
        isInBerserkState = false;
        isInBarrageMode = false;
        waitingForBarrageAnimation = false;
        currentTarget = null;

        animator.SetCharacterSource(playerController);

        playerController.SetAIMovementOverride(false);
        chaseBehavior.ClearTarget();

        rb.velocity = Vector2.zero;
        MoveInput = Vector2.zero;

        ClearRedGlow();
    }

    private void ApplyRedGlow()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.color = Color.red;
    }

    private void ClearRedGlow()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.color = Color.white;

    }

    public int CurrentComboCount => comboSystem != null ? comboSystem.CurrentComboCount : 1;
}
