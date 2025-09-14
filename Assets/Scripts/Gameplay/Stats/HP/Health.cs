using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float deathDelay = 1.0f;

    public int MaxHealth => characterStats != null ? characterStats.GetCharacterData().maxHealth : 100;
    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    [Header("Health Events")]
    [SerializeField] private HealthEvents events;
    public HealthEvents Events { get => events; set => events = value; }

    [Header("Health Drain Settings")]
    [SerializeField] private int drainAmount = 1;
    [SerializeField] private float drainInterval = 1f;

    private bool _isDead;
    private ICharacterAnimatorData animatorData;

    private Coroutine _drainCoroutine;

    private void Awake()
    {
        if (characterStats != null)
        {
            characterStats.GetCharacterData().Initialize();
            CurrentHealth = characterStats.GetCharacterData().currentHealth;
        }
        _isDead = false;

        animatorData = GetComponent<ICharacterAnimatorData>();
    }

    public void TakeDamage(int attack)
    {
        if (_isDead || attack <= 0) return;

        int attackDamage = attack - characterStats.GetCharacterData().hp.GetValue(); // Subtract defense from attack eventually
        attackDamage = Mathf.Clamp(attackDamage, 0, int.MaxValue);

        CurrentHealth -= attack;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            _isDead = true;

            events.onDeath?.Invoke();
            if (animatorData is MonoBehaviour mb)
            {
                var animator = mb.GetComponent<CharacterAnimator>();
                animator?.SendMessage("TriggerDeath", SendMessageOptions.DontRequireReceiver);
            }

            if (destroyOnDeath)
                Invoke(nameof(DestroySelf), deathDelay);
        }
        else
        {
            events.onTakeDamage?.Invoke();
        }
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }

    public void Heal(int amount)
    {
        if (_isDead || amount <= 0 || CurrentHealth >= MaxHealth) return;

        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
    }

    public void RestoreFullHealth() //Future Implementation
    {
        if (!_isDead)
            CurrentHealth = MaxHealth;
    }

    public void StartHealthDrain(int drainAmount, float interval)
    {
        if (_drainCoroutine != null)
            StopCoroutine(_drainCoroutine);

        _drainCoroutine = StartCoroutine(DrainHealthOverTime(drainAmount, interval));
    }

    public void StartHealthDrainFromEvent()
    {
        StartHealthDrain(drainAmount, drainInterval);
    }

    public void StopHealthDrain()
    {
        if (_drainCoroutine != null)
        {
            StopCoroutine(_drainCoroutine);
            _drainCoroutine = null;
        }
    }

    private IEnumerator DrainHealthOverTime(int drainAmount, float interval)
    {
        while (!_isDead)
        {
            TakeDamage(drainAmount);
            yield return new WaitForSeconds(interval);
        }
    }
}

[System.Serializable]
public class HealthEvents
{
    public UnityEvent onTakeDamage;
    public UnityEvent onDeath;
}
