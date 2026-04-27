public interface IDamageable//Sharable damage definition for players and enemies
{
    void TakeDamage(int amount);
    void Heal(int Amount);
}