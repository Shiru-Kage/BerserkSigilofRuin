using UnityEngine;

public class InCombatTracker : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private float combatTimeout = 5f;

    public bool IsInCombat => isInCombat;

    private float combatTimer = 0f;
    private bool isInCombat = false;

    private void Update()
    {
        if (isInCombat)
        {
            combatTimer -= Time.deltaTime;
            if (combatTimer <= 0f)
            {
                isInCombat = false;
                Debug.Log("Exited combat.");
            }
        }
    }

    public void NotifyCombatActivity()
    {
        if (!isInCombat)
        {
            isInCombat = true;
            Debug.Log("Entered combat.");
        }
        combatTimer = combatTimeout;
    }
}
