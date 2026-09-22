using System;
using Game.Combat;

namespace Game.Character
{
    /// <summary>Feeds the health ratio into stats without making Health depend on character rules.</summary>
    public sealed class CharacterHealthStatBinding : IDisposable
    {
        private readonly Health _health;
        private readonly CharacterStats _stats;

        public CharacterHealthStatBinding(Health health, CharacterStats stats)
        {
            _health = health ?? throw new ArgumentNullException(nameof(health));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            Refresh(0f, 0f);
            _health.HealthChanged += Refresh;
        }

        public void Dispose() => _health.HealthChanged -= Refresh;

        private void Refresh(float previous, float current)
        {
            _stats.UpdateHealthRatio(_health.CurrentHealth / _health.MaxHealth);
        }
    }
}
