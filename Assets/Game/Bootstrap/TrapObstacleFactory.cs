using System;
using UnityEngine;

namespace Game.Bootstrap
{
    /// <summary>Stationary body contact of a trap turret or barrel: blocks the player only (enemies and skills pass).</summary>
    public static class TrapObstacleFactory
    {
        public static CircleCollider2D Attach(GameObject root, float radius)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            Game.Content.NumericValidation.ValidatePositive(radius, nameof(radius));
            var playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer < 0) throw new InvalidOperationException("Trap contact requires the Player layer.");
            var collider = root.AddComponent<CircleCollider2D>();
            collider.radius = radius;
            collider.isTrigger = false;
            collider.excludeLayers = ~(1 << playerLayer);
            return collider;
        }
    }
}
