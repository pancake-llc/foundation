namespace Pancake.Component
{
    public interface IDamageable
    {
        float MaxHp { get; }
        float Hp { get; }
        bool IsAlive { get; }
        void TakeDamage(float damage);
        void Die();
        void Dispose();
    }
}