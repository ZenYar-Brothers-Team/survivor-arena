using System;

namespace Game.Enemy
{
    public sealed class EnemyHealth
    {
        public float MaxHealth { get; }
        public float CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }

        public event Action<float, float> HealthChanged;
        public event Action Died;

        public EnemyHealth(float maxHealth)
        {
            if (float.IsNaN(maxHealth) || float.IsInfinity(maxHealth) || maxHealth <= 0f)
                throw new ArgumentOutOfRangeException(nameof(maxHealth), "Max health must be finite and greater than zero.");

            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
        }

        public float TakeDamage(float amount)
        {
            if (float.IsNaN(amount) || float.IsInfinity(amount) || amount < 0f)
                throw new ArgumentOutOfRangeException(nameof(amount), "Damage must be finite and non-negative.");

            if (IsDead || amount == 0f)
                return 0f;

            var previousHealth = CurrentHealth;
            CurrentHealth = Math.Max(0f, CurrentHealth - amount);
            var appliedDamage = previousHealth - CurrentHealth;

            HealthChanged?.Invoke(previousHealth, CurrentHealth);

            if (CurrentHealth <= 0f)
            {
                IsDead = true;
                Died?.Invoke();
            }

            return appliedDamage;
        }
    }
}
