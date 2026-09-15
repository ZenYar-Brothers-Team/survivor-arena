using System;

namespace Game.ActiveSkill
{
    public sealed class ProjectileLifetime
    {
        public float RemainingSeconds { get; private set; }
        public bool IsExpired => RemainingSeconds <= 0f;

        public ProjectileLifetime(float lifetimeSeconds)
        {
            if (float.IsNaN(lifetimeSeconds) || float.IsInfinity(lifetimeSeconds) || lifetimeSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(lifetimeSeconds), "Lifetime must be finite and greater than zero.");

            RemainingSeconds = lifetimeSeconds;
        }

        public bool Tick(float deltaTime, bool isRunning)
        {
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), "Delta time must be finite and non-negative.");
            if (!isRunning || IsExpired)
                return IsExpired;

            RemainingSeconds = Math.Max(0f, RemainingSeconds - deltaTime);
            return IsExpired;
        }
    }
}
