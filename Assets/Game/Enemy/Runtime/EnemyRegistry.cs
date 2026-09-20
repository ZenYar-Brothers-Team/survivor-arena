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

        private static readonly HashSet<IEnemyLifeTarget> Enemies = new HashSet<IEnemyLifeTarget>();

        public static int Count
        {
            get
            {
                RemoveDestroyedEntries();
                return Enemies.Count;
            }
        }

        public static void Register(IEnemyLifeTarget enemy)
        {
            if (enemy != null)
                Enemies.Add(enemy);
        }

        public static void Unregister(IEnemyLifeTarget enemy)
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
                if (IsAvailable(enemy) && enemy.IsAlive && enemy is EnemyRuntime runtime)
                    destination.Add(runtime);
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
                    if (!IsAvailable(enemy) || !enemy.IsAlive || !(enemy is EnemyRuntime))
                        continue;

                    var distance = (enemy.Position - origin).sqrMagnitude;
                    if (distance >= nearestDistance)
                        continue;

                    nearestDistance = distance;
                    nearest = (EnemyRuntime)enemy;
                }

                RemoveDestroyedEntries();
                return nearest != null;
            }
        }

        public static void CopyTargetsTo(List<IEnemyDamageReceiver> destination, EnemyTargetCategories categories = EnemyTargetCategories.All)
        {
            if (destination == null) throw new System.ArgumentNullException(nameof(destination));
            using var guard = PerfGuard.Measure("EnemyRegistry.CopyTargetsTo", NearestSearchWarningMilliseconds);
            destination.Clear();
            foreach (var enemy in Enemies)
                if (IsAvailable(enemy) && enemy.IsAlive && Matches(enemy.Category, categories)) destination.Add(enemy);
            RemoveDestroyedEntries();
        }

        public static bool TryFindNearestTarget(Vector2 origin, out IEnemyDamageReceiver nearest, EnemyTargetCategories categories = EnemyTargetCategories.All)
        {
            using var guard = PerfGuard.Measure("EnemyRegistry.TryFindNearestTarget", NearestSearchWarningMilliseconds);
            nearest = null;
            var best = float.PositiveInfinity;
            foreach (var enemy in Enemies)
            {
                if (!IsAvailable(enemy) || !enemy.IsAlive || !Matches(enemy.Category, categories)) continue;
                var distance = (enemy.Position - origin).sqrMagnitude;
                if (distance >= best) continue;
                best = distance;
                nearest = enemy;
            }
            RemoveDestroyedEntries();
            return nearest != null;
        }

        private static bool IsAvailable(IEnemyLifeTarget target) => target != null &&
            (!(target is Object obj) || obj != null);

        private static bool Matches(EnemyCategory category, EnemyTargetCategories filter)
        {
            var flag = category switch
            {
                EnemyCategory.Boss => EnemyTargetCategories.Boss,
                EnemyCategory.Traveler => EnemyTargetCategories.Traveler,
                _ => EnemyTargetCategories.Ordinary
            };
            return (filter & flag) != 0;
        }

        private static void RemoveDestroyedEntries()
        {
            Enemies.RemoveWhere(enemy => !IsAvailable(enemy));
        }
    }
}
