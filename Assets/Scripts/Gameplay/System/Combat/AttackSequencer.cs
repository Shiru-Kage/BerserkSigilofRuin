using System;
using UnityEngine;

public class AttackSequencer : MonoBehaviour
{
    [Header("Combo Settings")]
    [SerializeField] private float comboResetTime = 2f;
    [SerializeField] private int maxComboCount = 3;
    [SerializeField] private string[] attackTriggers;
    public int CurrentComboCount => currentComboCount;

    private float comboTimer;
    private int currentComboCount = 0;
    private bool isComboActive = false;

    public event Action<int> OnComboAttack;
    public event Action OnComboFinished; 

    private void Update()
    {
        if (isComboActive)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                ResetCombo();
            }
        }
    }

    public void HandleAttack()
    {
        if (comboTimer <= 0f)
        {
            ResetCombo();
        }

        if (!isComboActive)
        {
            isComboActive = true;
            currentComboCount = 1;
        }
        else
        {
            currentComboCount = Mathf.Min(currentComboCount + 1, maxComboCount);
        }

        int validComboIndex = Mathf.Min(currentComboCount - 1, attackTriggers.Length - 1);

        if (validComboIndex < attackTriggers.Length)
        {
            OnComboAttack?.Invoke(validComboIndex);
        }

        if (currentComboCount == maxComboCount)
        {
            OnComboFinished?.Invoke();
        }

        comboTimer = comboResetTime;
    }

    private void ResetCombo()
    {
        currentComboCount = 0;
        isComboActive = false;
    }

    public int GetCurrentComboCount()
    {
        return currentComboCount;
    }

    public void ResetComboImmediately()
    {
        ResetCombo();
    }

    public string[] GetAttackTriggers()
    {
        return attackTriggers;
    }
}
