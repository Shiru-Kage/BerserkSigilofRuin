using System;
using UnityEngine;

public class AttackSequencer : MonoBehaviour
{
    [Header("Combo Settings")]
    [SerializeField] private float comboResetTime = 2f;
    [SerializeField] private int maxComboCount = 3;
    [SerializeField] private string[] attackTriggers;

    private float comboTimer;
    private int currentComboCount = 0;
    private bool isComboActive = false;

    public event Action OnComboAttack;

    private void Update()
    {
        comboTimer -= Time.deltaTime;
        if (comboTimer <= 0f && isComboActive)
        {
            ResetCombo();
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
            OnComboAttack?.Invoke();
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
}
