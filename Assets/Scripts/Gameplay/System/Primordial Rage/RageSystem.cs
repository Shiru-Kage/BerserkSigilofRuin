using UnityEngine;
using UnityEngine.Events;

public class RageSystem : MonoBehaviour
{
    [Header("Rage Settings")]
    [SerializeField] private float rage = 0f;
    [SerializeField] private float rageMax = 100f;
    [SerializeField] private float rageIncreasePerHit = 5f;
    [SerializeField] private float rageIncreaseOnDamage = 10f;
    [SerializeField] private float rageDrainRate = 25f;
    [SerializeField] private bool isBerserk = false;

    public float CurrentRage => rage;
    public float MaxRage => rageMax;

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Health playerHealth;
    [SerializeField] private BerserkState berserkState;
    [SerializeField] private CharacterAnimator characterAnimator;

    [Header("Berserk Settings")]
    [SerializeField] private float berserkHpDrainPerSecond = 5f;
    [SerializeField] private UnityEvent onEnterBerserk;
    [SerializeField] private UnityEvent onExitBerserk;

    private InCombatTracker combatTracker;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (playerHealth == null)
            playerHealth = GetComponent<Health>();

        if (berserkState == null)
            berserkState = GetComponent<BerserkState>();

        if (characterAnimator == null)
            characterAnimator = GetComponent<CharacterAnimator>();

        combatTracker = GetComponent<InCombatTracker>();

        if (playerHealth != null)
            playerHealth.Events.onTakeDamage.AddListener(OnPlayerTakeDamage);
    }

    private void Update()
    {
        if (isBerserk)
        {
            BerserkUpdate();
        }
        else
        {
            CheckThresholds();
        }
    }

    private void CheckThresholds()
    {
        if (rage >= rageMax) // 100% Rage
        {
            EnterBerserk();
        }
        else if (rage >= rageMax * 0.9f) // 90% Rage
        {
            ApplyMerits(90);
            ApplyDemerits(90);
        }
        else if (rage >= rageMax * 0.75f) // 75% Rage
        {
            ApplyMerits(75);
            ApplyDemerits(75);
        }
        else if (rage >= rageMax * 0.5f) // 50% Rage
        {
            ApplyMerits(50);
            ApplyDemerits(50);
        }
        else if (rage >= rageMax * 0.25f) // 25% Rage
        {
            ApplyMerits(25);
            ApplyDemerits(25);
        }
    }

    private void BerserkUpdate()
    {
        rage -= rageDrainRate * Time.deltaTime;
        playerHealth.TakeDamage(Mathf.RoundToInt(berserkHpDrainPerSecond * Time.deltaTime));

        if (rage <= 0)
        {
            ExitBerserk();
        }
    }

    private void EnterBerserk()
    {
        isBerserk = true;
        berserkState.EnterBerserk();

        if (characterAnimator != null)
        {
            characterAnimator.SetCharacterSource(berserkState);
        }

        onEnterBerserk?.Invoke();
        Debug.Log("ENTERED BERSERK");
    }

    private void ExitBerserk()
    {
        isBerserk = false;
        rage = 0;
        berserkState.ExitBerserk();

        if (characterAnimator != null)
        {
            characterAnimator.SetCharacterSource(playerController);
        }

        onExitBerserk?.Invoke();
        Debug.Log("EXITED BERSERK");
    }

    private void AddRage(float amount)
    {
        if (!isBerserk && combatTracker != null && combatTracker.IsInCombat)
        {
            rage += amount;
            rage = Mathf.Clamp(rage, 0, rageMax);
        }
    }

    public void AddRageFromHit()
    {
        AddRage(rageIncreasePerHit);
    }

    private void OnPlayerTakeDamage()
    {
        if (combatTracker != null)
            combatTracker.NotifyCombatActivity();

        AddRage(rageIncreaseOnDamage);
    }

    private void ApplyMerits(float ragePercentage)
    {
        switch (ragePercentage)
        {
            case 25:
                Debug.Log("25% Rage - Increased Attack Speed!");
                break;

            case 50:
                Debug.Log("50% Rage - Increased Damage Output!");
                break;

            case 75:
                Debug.Log("75% Rage - Increased Critical Chance!");
                break;

            case 90:
                Debug.Log("90% Rage - Increased Strength but with limited control!");
                break;
        }
    }

    private void ApplyDemerits(float ragePercentage)
    {
        switch (ragePercentage)
        {
            case 25:
                Debug.Log("25% Rage - Increased Attack Speed!");
                break;

            case 50:
                Debug.Log("50% Rage - Increased Damage Output!");
                break;

            case 75:
                Debug.Log("75% Rage - Increased Critical Chance!");
                break;

            case 90:
                Debug.Log("90% Rage - Decreased Defense!");
                break;
        }
    }
}
