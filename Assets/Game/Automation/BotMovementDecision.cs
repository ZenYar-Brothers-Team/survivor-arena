using UnityEngine;

namespace Game.Automation
{
    /// <summary>Direction enters ordinary PlayerMover; Stuck/CoverageIncomplete are diagnostic, not game outcomes.</summary>
    public readonly struct BotMovementDecision
    {
        public Vector2 Direction { get; }
        public bool Stuck { get; }
        public bool CoverageIncomplete { get; }
        public BotMovementDecision(Vector2 direction, bool stuck = false, bool coverageIncomplete = false)
        {
            Direction = direction;
            Stuck = stuck;
            CoverageIncomplete = coverageIncomplete;
        }
    }
}
