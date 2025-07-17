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
        if (rage >= rageMax)
        {
            EnterBerserk();
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
        onEnterBerserk?.Invoke();
        Debug.Log("ENTERED BERSERK");
    }

    private void ExitBerserk()
    {
        isBerserk = false;
        rage = 0;
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
}
