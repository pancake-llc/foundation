namespace Pancake.Component
{
    public static class DamageHelper
    {
        public static void InflictDamage(IDamageable target, float baseDamage, IDamageStrategy strategy)
        {
            // Calculate the final damage using the strategy
            // It is also possible to handle the application of control effects inside the CalculateDamage of each specific strategy type.
            float finalDamage = strategy.OnExecute(baseDamage);

            // Apply the final damage to the target
            target.TakeDamage(finalDamage);
        }
    }
}