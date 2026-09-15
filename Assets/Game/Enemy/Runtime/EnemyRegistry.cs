using System.Collections.Generic;
using Game.Diagnostics;
using UnityEngine;

namespace Game.Enemy
{
    public static class EnemyRegistry
    {
        // Linear scan over every registered enemy; fine at current fixture scale, but
        // warns if it starts costing real time once enemy counts grow (see AGENTS.md).
        private const float NearestSearchWarningMilliseconds = 1f;

        private static readonly HashSet<EnemyRuntime> Enemies = new HashSet<EnemyRuntime>();

        public static int Count
        {
            get
            {
                RemoveDestroyedEntries();
                return Enemies.Count;
            }
        }

        public static void Register(EnemyRuntime enemy)
        {
            if (enemy != null)
                Enemies.Add(enemy);
        }

        public static void Unregister(EnemyRuntime enemy)
        {
            if (enemy != null)
                Enemies.Remove(enemy);
        }

        public static void CopyAliveTo(List<EnemyRuntime> destination)
        {
            if (destination == null)
                throw new System.ArgumentNullException(nameof(destination));

            destination.Clear();
            foreach (var enemy in Enemies)
            {
                if (enemy != null && enemy.IsAlive)
                    destination.Add(enemy);
            }

            RemoveDestroyedEntries();
        }

        public static bool TryFindNearest(Vector2 origin, out EnemyRuntime nearest)
        {
            using (PerfGuard.Measure("EnemyRegistry.TryFindNearest", NearestSearchWarningMilliseconds))
            {
                nearest = null;
                var nearestDistance = float.PositiveInfinity;
                foreach (var enemy in Enemies)
                {
                    if (enemy == null || !enemy.IsAlive)
                        continue;

                    var distance = (enemy.Position - origin).sqrMagnitude;
                    if (distance >= nearestDistance)
                        continue;

                    nearestDistance = distance;
                    nearest = enemy;
                }

                RemoveDestroyedEntries();
                return nearest != null;
            }
        }

        private static void RemoveDestroyedEntries()
        {
            Enemies.RemoveWhere(enemy => enemy == null);
        }
    }
}
