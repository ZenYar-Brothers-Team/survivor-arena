using System;

namespace Game.Enemy
{
    /// <summary>Roundup clock (DECISION-0155). The wait counts down only while a crowd exists; an active roundup runs a
    /// hard timeout, and only after it ends does the next random interval start. Pure and pause-safe: the caller passes
    /// <c>isRunning</c> and delta time.</summary>
    public sealed class RaidScheduler
    {
        private readonly RaidProfile _profile;
        private readonly Random _random;

        public bool IsActive { get; private set; }
        public RaidTemplateKind ActiveTemplate { get; private set; }
        public float ActiveRemainingSeconds { get; private set; }
        public float WaitRemainingSeconds { get; private set; }
        public bool CrowdPresent { get; private set; }

        public RaidScheduler(RaidProfile profile, Random random)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            // The first roundup of a run needs no countdown: it starts as soon as a crowd exists.
            WaitRemainingSeconds = 0f;
        }

        /// <summary>Returns true on the tick the countdown reaches zero (the caller then calls <see cref="Start"/>).</summary>
        public bool Tick(float deltaSeconds, bool isRunning, int nearbyCount)
        {
            if (!isRunning || deltaSeconds <= 0f) return false;
            if (IsActive)
            {
                ActiveRemainingSeconds -= deltaSeconds;
                if (ActiveRemainingSeconds <= 0f) End();
                return false;
            }
            CrowdPresent = nearbyCount >= _profile.TriggerEnemyCount;
            if (!CrowdPresent) return false;
            WaitRemainingSeconds -= deltaSeconds;
            return WaitRemainingSeconds <= 0f;
        }

        public bool Start(RaidTemplateKind kind)
        {
            if (IsActive) return false;
            IsActive = true;
            ActiveTemplate = kind;
            ActiveRemainingSeconds = _profile.DurationOf(kind);
            return true;
        }

        public void End()
        {
            if (!IsActive) return;
            IsActive = false;
            ActiveRemainingSeconds = 0f;
            WaitRemainingSeconds = NextInterval();
        }

        private float NextInterval() => _profile.MinIntervalSeconds +
            (float)_random.NextDouble() * (_profile.MaxIntervalSeconds - _profile.MinIntervalSeconds);
    }
}
