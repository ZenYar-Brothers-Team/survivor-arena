using UnityEngine;

namespace Game.Automation
{
    /// <summary>Visible enemy, projectile or hazardous circle. Velocity is world units per simulation second.</summary>
    public readonly struct BotThreat
    {
        public Vector2 Position { get; }
        public Vector2 Velocity { get; }
        public float Radius { get; }
        public float Weight { get; }
        public bool IsEnemy { get; }

        public BotThreat(Vector2 position, Vector2 velocity, float radius, float weight, bool isEnemy = false)
        {
            Position = position;
            Velocity = velocity;
            Radius = radius;
            Weight = weight;
            IsEnemy = isEnemy;
        }
    }
}
