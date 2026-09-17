using System;

namespace Game.Enemy
{
    public sealed class EnemyProjectileLifetime
    {
        public float RemainingSeconds { get; private set; }
        public bool IsExpired => RemainingSeconds <= 0f;

        public EnemyProjectileLifetime(float seconds)
        {
            if (seconds <= 0f || float.IsNaN(seconds) || float.IsInfinity(seconds))
                throw new ArgumentOutOfRangeException(nameof(seconds));
            RemainingSeconds = seconds;
        }

        public bool Tick(float deltaTime, bool isSimulating)
        {
            if (deltaTime < 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            if (isSimulating)
                RemainingSeconds = Math.Max(0f, RemainingSeconds - deltaTime);
            return IsExpired;
        }
    }
}
