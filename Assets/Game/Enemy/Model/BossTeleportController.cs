using System;
using Game.Content;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// Teleport-slam state machine (DECISION-0059). Tracking → Telegraphing once the player has been farther than
    /// the far distance for the far time without interruption; Telegraphing → Tracking with an Impact after the
    /// telegraph. Coming back within the far distance resets the far timer; a started telegraph always lands.
    /// Nothing advances while the run is not running.
    /// </summary>
    public sealed class BossTeleportController
    {
        private float _farElapsed;
        private float _telegraphRemaining;
        private readonly System.Random _random;

        public BossTeleportProfile Profile { get; }
        public BossTeleportPhase Phase { get; private set; }
        public float FarElapsed => _farElapsed;
        public float TelegraphRemaining => _telegraphRemaining;
        /// <summary>Landing point of the current (or last) telegraph.</summary>
        public Vector2 Landing { get; private set; }

        /// <param name="random">Landing direction source; the runtime seeds it per boss life.</param>
        public BossTeleportController(BossTeleportProfile profile, System.Random random)
        {
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public BossTeleportSignal Tick(float deltaTime, bool isRunning, Vector2 boss, Vector2 player)
        {
            NumericValidation.ValidateNonNegative(deltaTime, nameof(deltaTime));
            if (!isRunning) return BossTeleportSignal.None;
            if (Phase == BossTeleportPhase.Telegraphing)
            {
                _telegraphRemaining -= deltaTime;
                if (_telegraphRemaining > 0f) return BossTeleportSignal.None;
                _telegraphRemaining = 0f;
                _farElapsed = 0f;
                Phase = BossTeleportPhase.Tracking;
                return BossTeleportSignal.Impact;
            }
            var offset = boss - player;
            if (offset.sqrMagnitude <= Profile.FarDistance * Profile.FarDistance)
            {
                _farElapsed = 0f;
                return BossTeleportSignal.None;
            }
            _farElapsed += deltaTime;
            if (_farElapsed < Profile.FarSeconds) return BossTeleportSignal.None;
            // Uniformly random point on the landing circle around the player (user request 2026-09-25, DECISION-0059).
            var angle = (float)(_random.NextDouble() * Math.PI * 2d);
            Landing = player + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Profile.LandingDistance;
            _telegraphRemaining = Profile.TelegraphSeconds;
            Phase = BossTeleportPhase.Telegraphing;
            return BossTeleportSignal.TelegraphStarted;
        }

        /// <summary>Whether a point (the player) is caught by the slam at the current landing point.</summary>
        public bool Hits(Vector2 point) => (point - Landing).sqrMagnitude <= Profile.ImpactRadius * Profile.ImpactRadius;
    }
}
