using System;
using System.Collections.Generic;
using Game.Diagnostics;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>Main-thread snapshot source for currently active enemy projectiles; pooled instances unregister on clear.</summary>
    public static class EnemyProjectileRegistry
    {
        private static readonly HashSet<EnemyProjectileRuntime> Active = new HashSet<EnemyProjectileRuntime>();

        public static void Register(EnemyProjectileRuntime projectile)
        {
            if (projectile != null) Active.Add(projectile);
        }

        public static void Unregister(EnemyProjectileRuntime projectile)
        {
            if (projectile != null) Active.Remove(projectile);
        }

        public static void CopyActiveTo(List<EnemyProjectileRuntime> destination)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            using var guard = PerfGuard.Measure("EnemyProjectileRegistry.CopyActiveTo", 2f);
            destination.Clear();
            foreach (var projectile in Active)
                if (projectile != null && projectile.IsActive) destination.Add(projectile);
            Active.RemoveWhere(projectile => projectile == null || !projectile.IsActive);
        }
    }
}
