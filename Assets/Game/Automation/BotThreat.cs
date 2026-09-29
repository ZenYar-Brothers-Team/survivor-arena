using UnityEngine;

namespace Game.Automation
{
    /// <summary>Visible enemy, projectile or hazardous circle. Velocity is world units per simulation second.</summary>
    public readonly struct BotThreat
    {
        public Vector2 Position { get; }
        public Vector2 Velocity { get; }
        public float Radius { get; }
        public float CollisionRadius { get; }
        public float Weight { get; }
        public bool IsEnemy { get; }
        public BotThreatMotion Motion { get; }
        public float MovementSpeed { get; }
        public float PreferredDistance { get; }
        public float DistanceTolerance { get; }

        public BotThreat(Vector2 position, Vector2 velocity, float radius, float weight, bool isEnemy = false,
            BotThreatMotion motion = BotThreatMotion.Linear, float movementSpeed = 0f,
            float preferredDistance = 0f, float distanceTolerance = 0f, float? collisionRadius = null)
        {
            Position = position;
            Velocity = velocity;
            Radius = radius;
            CollisionRadius = collisionRadius ?? radius;
            Weight = weight;
            IsEnemy = isEnemy;
            Motion = motion;
            MovementSpeed = movementSpeed;
            PreferredDistance = preferredDistance;
            DistanceTolerance = distanceTolerance;
        }
    }
}
