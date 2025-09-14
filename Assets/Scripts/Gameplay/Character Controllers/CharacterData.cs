using UnityEngine;

[CreateAssetMenu(fileName = "New Character Data", menuName = "Character/Character Data")]
public class CharacterData : ScriptableObject
{
    public int maxHealth = 100;
    public int currentHealth;
    public Stats hp;
    public Stats attack;
    public Stats atkSpeed;
    public Stats defense;
    public Stats speed;

    public void Initialize()
    {
        currentHealth = maxHealth;
    }
}
