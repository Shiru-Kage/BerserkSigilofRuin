using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float deathDelay = 1.0f;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    [Header("Health Events")]
    [SerializeField] private HealthEvents events;
    public HealthEvents Events { get => events; set => events = value; }

    private bool _isDead;
    private ICharacterAnimatorData animatorData;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        _isDead = false;

        animatorData = GetComponent<ICharacterAnimatorData>();
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
        if (_isDead || amount <= 0 || CurrentHealth >= maxHealth) return;

        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
    }

    public void RestoreFullHealth() //Future Implementation
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
