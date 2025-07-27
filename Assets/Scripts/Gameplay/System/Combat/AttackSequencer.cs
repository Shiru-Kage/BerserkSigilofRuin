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
            Debug.Log("Combo reset time exceeded. Resetting combo.");
            ResetCombo();  
        }
    }

    public void HandleAttack()
    {
        if (comboTimer <= 0f) 
        {
            Debug.Log("Combo timer expired. Resetting combo and starting with Attack1.");
            ResetCombo();
        }

        if (!isComboActive)
        {
            isComboActive = true;
            currentComboCount = 1;
            Debug.Log("Starting new combo with Attack1.");
        }
        else
        {
            currentComboCount = Mathf.Min(currentComboCount + 1, maxComboCount);
            Debug.Log($"Combo incremented to Attack{currentComboCount}.");
        }

        if (attackTriggers.Length >= currentComboCount)
        {
            OnComboAttack?.Invoke();
            Debug.Log($"Combo attack triggered: {attackTriggers[currentComboCount - 1]}");
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
