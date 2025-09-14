using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("Character Reference")]
    [SerializeField] private CharacterData characteStatData;
    public CharacterData GetCharacterData() => characteStatData;
    void Start()
    {
        characteStatData.Initialize();
    }

    public void TakeDamage(int attack)
    {
        attack -= characteStatData.hp.GetValue();
        attack = Mathf.Clamp(attack, 0, int.MaxValue);

        characteStatData.currentHealth -= attack;
        Debug.Log("This object has been attacked for " + attack);

        if (characteStatData.currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log("Player has died.");
        //PlayerManager.instance.KillPlayer();
    }
}
