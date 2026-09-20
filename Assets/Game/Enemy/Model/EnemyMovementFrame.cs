using UnityEngine;

namespace Game.Enemy
{
    public readonly struct EnemyMovementFrame
    {
        public Vector2 Velocity { get; }
        public EnemyMovementPhase Phase { get; }
        public Vector2 TelegraphDirection { get; }

        public bool IsTelegraphing => Phase == EnemyMovementPhase.TelegraphingDash;

        public EnemyMovementFrame(Vector2 velocity, EnemyMovementPhase phase, Vector2 telegraphDirection = default)
        {
            Velocity = velocity;
            Phase = phase;
            TelegraphDirection = telegraphDirection;
        }
    }
}
