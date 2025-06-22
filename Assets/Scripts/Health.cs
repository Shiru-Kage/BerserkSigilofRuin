using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private bool destroyOnDeath = true;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    [Header("Health Events")]
    [SerializeField] private HealthEvents events;
    public HealthEvents Events { get => events; set => events = value; }

    private bool _isDead;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        _isDead = false;
    }

    public void TakeDamage(int amount)
    {
        if (_isDead || amount <= 0) return;

        CurrentHealth -= amount;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            _isDead = true;
            events.onDeath?.Invoke();
            if (destroyOnDeath)
                Destroy(gameObject);
        }
        else
        {
            events.onTakeDamage?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (_isDead || amount <= 0 || CurrentHealth >= maxHealth) return;

        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
    }

    public void Kill()
    {
        if (!_isDead)
            TakeDamage(CurrentHealth);
    }

    public void RestoreFullHealth()
    {
        if (!_isDead)
            CurrentHealth = maxHealth;
    }
}

[System.Serializable]
public class HealthEvents
{
    public UnityEvent onTakeDamage;
    public UnityEvent onDeath;
}
