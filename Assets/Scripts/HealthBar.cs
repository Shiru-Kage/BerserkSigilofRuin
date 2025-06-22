using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Health health;

    [Header("UI Images")]
    [SerializeField] private Image mainFillImage;
    [SerializeField] private Image delayedFillImage; 

    [Header("Damage Lag Settings")]
    [SerializeField] private float delayBeforeDrain = 0.5f;
    [SerializeField] private float drainSpeed = 1.0f;

    private float targetFill;
    private float delayTimer;

    private void Start()
    {
        if (health == null || mainFillImage == null || delayedFillImage == null) return;

        UpdateBarImmediate();

        health.Events.onTakeDamage.AddListener(TriggerDamageEffect);
        health.Events.onDeath.AddListener(TriggerDamageEffect);
    }

    private void OnDestroy()
    {
        if (health == null) return;

        health.Events.onTakeDamage.RemoveListener(TriggerDamageEffect);
        health.Events.onDeath.RemoveListener(TriggerDamageEffect);
    }

    private void Update()
    {
        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
        }
        else
        {
            if (delayedFillImage.fillAmount > targetFill)
            {
                delayedFillImage.fillAmount = Mathf.MoveTowards(
                    delayedFillImage.fillAmount,
                    targetFill,
                    drainSpeed * Time.deltaTime
                );
            }
        }
    }

    private void TriggerDamageEffect()
    {
        targetFill = (float)health.CurrentHealth / health.MaxHealth;
        mainFillImage.fillAmount = targetFill;
        delayTimer = delayBeforeDrain;
    }

    private void UpdateBarImmediate()
    {
        float fill = (float)health.CurrentHealth / health.MaxHealth;
        mainFillImage.fillAmount = fill;
        delayedFillImage.fillAmount = fill;
        targetFill = fill;
    }
}
