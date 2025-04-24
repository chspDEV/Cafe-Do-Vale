public interface IHealth
{
    short MaxHealth { get; }
    short CurrentHealth { get; }
    bool IsAlive { get; }

    void TakeDamage(short amount);
    void Heal(short amount);
    void Die();
    void Revive();
}
