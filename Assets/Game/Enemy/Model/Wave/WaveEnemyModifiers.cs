using Game.Content;

namespace Game.Enemy
{
    // Per-phase stat multipliers applied to every enemy the phase spawns. Each
    // stat is independent, so a phase can trade one against another (faster but
    // frailer) instead of scaling everything up together.
    public sealed class WaveEnemyModifiers
    {
        public static WaveEnemyModifiers Identity { get; } = new WaveEnemyModifiers();

        public float HealthMultiplier { get; }
        public float SpeedMultiplier { get; }
        public float ContactDamageMultiplier { get; }
        public float AttackDamageMultiplier { get; }
        public bool IsIdentity =>
            HealthMultiplier == 1f && SpeedMultiplier == 1f &&
            ContactDamageMultiplier == 1f && AttackDamageMultiplier == 1f;

        public WaveEnemyModifiers(
            float healthMultiplier = 1f,
            float speedMultiplier = 1f,
            float contactDamageMultiplier = 1f,
            float attackDamageMultiplier = 1f)
        {
            NumericValidation.ValidatePositive(healthMultiplier, nameof(healthMultiplier));
            NumericValidation.ValidateNonNegative(speedMultiplier, nameof(speedMultiplier));
            NumericValidation.ValidateNonNegative(contactDamageMultiplier, nameof(contactDamageMultiplier));
            NumericValidation.ValidateNonNegative(attackDamageMultiplier, nameof(attackDamageMultiplier));

            HealthMultiplier = healthMultiplier;
            SpeedMultiplier = speedMultiplier;
            ContactDamageMultiplier = contactDamageMultiplier;
            AttackDamageMultiplier = attackDamageMultiplier;
        }
    }
}
