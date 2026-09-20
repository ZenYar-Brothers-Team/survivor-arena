using System;
using Game.Combat;
using Game.Run;

namespace Game.Character
{
    public sealed class CharacterRunBinding : IDisposable
    {
        private readonly Health _health;
        private readonly RunModel _run;
        private bool _disposed;

        public CharacterRunBinding(Health health, RunModel run)
        {
            _health = health ?? throw new ArgumentNullException(nameof(health));
            _run = run ?? throw new ArgumentNullException(nameof(run));
            _health.Died += HandleDeath;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _health.Died -= HandleDeath;
            _disposed = true;
        }

        private void HandleDeath()
        {
            _run.Kill();
        }
    }
}
