using System;
using Game.Content;

namespace Game.Combat
{
    public sealed class Health : IDisposable
    {
        private readonly IHealthProfile _profile;
        private float _lastMaxHealth;
        private bool _disposed;

        public float CurrentHealth { get; private set; }
        public float MaxHealth => _profile.MaxHealth;
        public bool IsDead { get; private set; }

        public event Action<float, float> HealthChanged;
        public event Action<float> Damaged;
        public event Action Died;

        public Health(IHealthProfile profile)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            CurrentHealth = profile.MaxHealth;
            _lastMaxHealth = profile.MaxHealth;
            _profile.Changed += HandleProfileChanged;
        }

        public float TakeDamage(float amount) => TakeDamageMeasured(amount).Actual;

        public HealthChange TakeDamageMeasured(float amount)
        {
            NumericValidation.ValidateNonNegativeFinite(amount, nameof(amount));
            if (IsDead || amount == 0f)
                return new HealthChange(amount, 0f, 0f, false);

            var previousHealth = CurrentHealth;
            var scaledDamage = amount * _profile.IncomingDamageMultiplier;
            NumericValidation.ValidateNonNegativeFinite(scaledDamage, nameof(scaledDamage));
            CurrentHealth = Math.Max(0f, CurrentHealth - scaledDamage);
            var appliedDamage = previousHealth - CurrentHealth;
            var change = new HealthChange(amount, scaledDamage, appliedDamage, false);

            if (appliedDamage <= 0f)
                return change;

            var causedDeath = CurrentHealth <= 0f;
            if (causedDeath) IsDead = true;

            HealthChanged?.Invoke(previousHealth, CurrentHealth);
            Damaged?.Invoke(appliedDamage);

            if (causedDeath)
            {
                Died?.Invoke();
            }

            return change;
        }

        public float Heal(float amount) => HealMeasured(amount).Actual;

        public HealthChange HealMeasured(float amount)
        {
            NumericValidation.ValidateNonNegativeFinite(amount, nameof(amount));
            if (IsDead || amount == 0f)
                return new HealthChange(amount, 0f, 0f, true);

            var previousHealth = CurrentHealth;
            var scaledHealing = amount * _profile.HealthRestorationMultiplier;
            NumericValidation.ValidateNonNegativeFinite(scaledHealing, nameof(scaledHealing));
            CurrentHealth = Math.Min(MaxHealth, CurrentHealth + scaledHealing);
            var appliedHealing = CurrentHealth - previousHealth;
            var change = new HealthChange(amount, scaledHealing, appliedHealing, true);

            if (appliedHealing > 0f)
                HealthChanged?.Invoke(previousHealth, CurrentHealth);

            return change;
        }

        public float Regenerate(float deltaTime, bool isRunning)
        {
            NumericValidation.ValidateNonNegativeFinite(deltaTime, nameof(deltaTime));
            if (!isRunning || IsDead || deltaTime == 0f)
                return 0f;

            return Heal(_profile.HealthRegenerationPerSecond * deltaTime);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _profile.Changed -= HandleProfileChanged;
            _disposed = true;
        }

        private void HandleProfileChanged()
        {
            if (_lastMaxHealth == MaxHealth) return;
            var previousHealth = CurrentHealth;
            var healthRatio = _lastMaxHealth > 0f ? CurrentHealth / _lastMaxHealth : 0f;
            _lastMaxHealth = MaxHealth;
            CurrentHealth = Math.Min(MaxHealth, Math.Max(0f, healthRatio * MaxHealth));
            if (Math.Abs(CurrentHealth - previousHealth) > float.Epsilon)
                HealthChanged?.Invoke(previousHealth, CurrentHealth);
        }
    }
}
