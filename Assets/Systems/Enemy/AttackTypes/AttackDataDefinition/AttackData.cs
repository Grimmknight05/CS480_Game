using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Attack")]
public class AttackData : ScriptableObject
{
    public  float attackRange = 2f; // Distance to attack player
    public float attackCooldown = 1f; // Time between attacks
    public int attackDamage = 10; // Damage dealt per attack
    public StatusEffect[] effects;
}