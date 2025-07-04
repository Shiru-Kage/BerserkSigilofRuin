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

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (playerHealth == null)
            playerHealth = GetComponent<Health>();

        if (playerController != null)
        {
            playerController.OnAttack += OnPlayerAttackAttempt;
        }

        if (playerHealth != null)
        {
            playerHealth.Events.onTakeDamage.AddListener(OnPlayerTakeDamage);
        }
    }

    private void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnAttack -= OnPlayerAttackAttempt;
        }
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
        if (rage >= 25f && rage < 50f)
        {
            // optional threshold logic
        }
        else if (rage >= 50f && rage < 75f)
        {
            // optional threshold logic
        }
        else if (rage >= 75f && rage < 90f)
        {
            // optional threshold logic
        }
        else if (rage >= 90f && rage < 100f)
        {
            // optional threshold logic
        }

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
        if (!isBerserk)
        {
            rage += amount;
            rage = Mathf.Clamp(rage, 0, rageMax);
        }
    }

    private void OnPlayerAttackAttempt()
    {
        AddRage(rageIncreasePerHit);
    }

    private void OnPlayerTakeDamage()
    {
        AddRage(rageIncreaseOnDamage);
    }
}
