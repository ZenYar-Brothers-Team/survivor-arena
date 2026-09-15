using System;

namespace Game.Progression
{
    public sealed class ExperienceDropTimer
    {
        public float Lifetime { get; }
        public float Elapsed { get; private set; }
        public bool IsExpired { get; private set; }

        public ExperienceDropTimer(float lifetime)
        {
            if (float.IsNaN(lifetime) || float.IsInfinity(lifetime) || lifetime <= 0f)
                throw new ArgumentOutOfRangeException(nameof(lifetime), "Lifetime must be finite and greater than zero.");

            Lifetime = lifetime;
        }

        public bool Tick(float deltaTime, bool isRunning)
        {
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), "Delta time must be finite and non-negative.");
            if (IsExpired || !isRunning)
                return false;

            Elapsed = Math.Min(Lifetime, Elapsed + deltaTime);
            if (Elapsed < Lifetime)
                return false;

            IsExpired = true;
            return true;
        }
    }
}
