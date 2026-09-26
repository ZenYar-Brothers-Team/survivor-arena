using System;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// Dedicated physics layer for every <see cref="EnemyRuntime"/> (ordinary enemies, bosses, Travelers), so skill area
    /// queries skip XP drops, projectiles, pickups and walls before any component lookup (DECISION-0056).
    /// </summary>
    public static class EnemyPhysicsLayer
    {
        public const string Name = "Enemy";

        public static int Index
        {
            get
            {
                var index = LayerMask.NameToLayer(Name);
                if (index < 0) throw new InvalidOperationException($"Physics layer '{Name}' is missing (DECISION-0056).");
                return index;
            }
        }

        /// <summary>Same as <see cref="ContactFilter2D.noFilter"/> (triggers included) but limited to the enemy layer.</summary>
        public static ContactFilter2D CreateQueryFilter()
        {
            var filter = ContactFilter2D.noFilter;
            filter.SetLayerMask(1 << Index);
            return filter;
        }
    }
}
