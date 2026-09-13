using System;

namespace Game.Character
{
    public sealed class CharacterHealth : IDisposable
    {
        private readonly CharacterStats _stats;
        private bool _disposed;

        public float CurrentHealth { get; private set; }
        public float MaxHealth => _stats.MaxHealth;
        public bool IsDead { get; private set; }

        public event Action<float, float> HealthChanged;
        public event Action Died;

        public CharacterHealth(CharacterStats stats)
        {
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            CurrentHealth = stats.MaxHealth;
            _stats.Changed += HandleStatsChanged;
        }

        public float TakeDamage(float amount)
        {
            ValidateNonNegativeFinite(amount, nameof(amount));
            if (IsDead || amount == 0f)
                return 0f;

            var previousHealth = CurrentHealth;
            var scaledDamage = amount * _stats.IncomingDamageMultiplier;
            CurrentHealth = Math.Max(0f, CurrentHealth - scaledDamage);
            var appliedDamage = previousHealth - CurrentHealth;

            if (appliedDamage <= 0f)
                return 0f;

            HealthChanged?.Invoke(previousHealth, CurrentHealth);

            if (CurrentHealth <= 0f)
            {
                IsDead = true;
                Died?.Invoke();
            }

            return appliedDamage;
        }

        public float Heal(float amount)
        {
            ValidateNonNegativeFinite(amount, nameof(amount));
            if (IsDead || amount == 0f || CurrentHealth >= MaxHealth)
                return 0f;

            var previousHealth = CurrentHealth;
            var scaledHealing = amount * _stats.HealthRestorationMultiplier;
            CurrentHealth = Math.Min(MaxHealth, CurrentHealth + scaledHealing);
            var appliedHealing = CurrentHealth - previousHealth;

            if (appliedHealing > 0f)
                HealthChanged?.Invoke(previousHealth, CurrentHealth);

            return appliedHealing;
        }

        public float Regenerate(float deltaTime, bool isRunning)
        {
            ValidateNonNegativeFinite(deltaTime, nameof(deltaTime));
            if (!isRunning || IsDead || deltaTime == 0f)
                return 0f;

            return Heal(_stats.HealthRegenerationPerSecond * deltaTime);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _stats.Changed -= HandleStatsChanged;
            _disposed = true;
        }

        private void HandleStatsChanged()
        {
            if (CurrentHealth <= MaxHealth)
                return;

            var previousHealth = CurrentHealth;
            CurrentHealth = MaxHealth;
            HealthChanged?.Invoke(previousHealth, CurrentHealth);
        }

        private static void ValidateNonNegativeFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                throw new ArgumentOutOfRangeException(parameterName, "Value must be finite and non-negative.");
        }
    }
}
