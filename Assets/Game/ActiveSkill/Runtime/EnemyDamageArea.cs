using System.Collections.Generic;
using Game.Enemy;
using Game.Diagnostics;
using UnityEngine;
using UnityEngine.Pool;

namespace Game.ActiveSkill
{
    /// <summary>
    /// Physics-query area damage for skills. Main-thread only; buffers are rented per call so lethal callbacks may
    /// re-enter. Queries all Default-layer colliders (there is no dedicated enemy layer yet — DECISION-0056).
    /// </summary>
    public static class EnemyDamageArea
    {
        /// <summary>Damages each live receiver overlapping one circle once; knockback points away from the center.</summary>
        /// <summary>Is <paramref name="offset"/> inside an ellipse of half-width <paramref name="radius"/>
        /// and half-height radius × <paramref name="verticalScale"/>? Boundary counts as inside.</summary>
        public static bool InsideEllipse(Vector2 offset, float radius, float verticalScale)
        {
            var x = offset.x / radius;
            var y = offset.y / (radius * verticalScale);
            return x * x + y * y <= 1f + 1e-4f;
        }

        public static int Apply(
            Vector2 center,
            float radius,
            EnemyDamageRequest damage,
            IEnemyDamageReceiver directTarget = null,
            float verticalScale = 1f)
        {
            using var guard = PerfGuard.Measure("EnemyDamageArea.Apply", 2f);
            // Each invocation rents its own buffers, including nested calls from lethal callbacks.
            var colliders = ListPool<Collider2D>.Get();
            var damaged = HashSetPool<IEnemyDamageReceiver>.Get();
            try
            {
                ApplyOnce(directTarget, damage, damaged);
                if (radius <= 0f) return damaged.Count;
                Physics2D.OverlapCircle(center, radius, ContactFilter2D.noFilter, colliders);
                for (var i = 0; i < colliders.Count; i++)
                {
                    if (colliders[i] == null) continue;
                    // Flattened ground area (DECISION-0058): the body's nearest point must lie in the ellipse.
                    if (verticalScale < 1f && !InsideEllipse(colliders[i].ClosestPoint(center) - center, radius, verticalScale)) continue;
                    var receiver = colliders[i].GetComponentInParent<IEnemyDamageReceiver>();
                    var radial = receiver != null ? receiver.Position - center : Vector2.zero;
                    ApplyOnce(receiver, damage.WithDirection(radial.x, radial.y), damaged);
                }
                return damaged.Count;
            }
            finally
            {
                ListPool<Collider2D>.Release(colliders);
                HashSetPool<IEnemyDamageReceiver>.Release(damaged);
            }
        }

        /// <summary>
        /// Same result as calling <see cref="Apply"/> once per circle in <paramref name="circleCenters"/> order, but with a
        /// single physics query covering all circles (<paramref name="queryCenter"/>, <paramref name="queryRadius"/> must
        /// contain every circle). Each circle damages a receiver at most once, so a receiver under two circles takes two
        /// hits exactly as with separate calls; knockback points away from the hitting circle. Overlap uses the collider
        /// shape (<see cref="Collider2D.ClosestPoint"/>), matching <c>OverlapCircle</c> rather than center distance.
        /// </summary>
        /// <returns>Total hits applied across all circles.</returns>
        public static int ApplyCircles(Vector2 queryCenter, float queryRadius, IReadOnlyList<Vector2> circleCenters,
            float circleRadius, EnemyDamageRequest damage)
        {
            if (circleCenters == null) throw new System.ArgumentNullException(nameof(circleCenters));
            if (circleRadius <= 0f || circleCenters.Count == 0) return 0;
            var colliders = ListPool<Collider2D>.Get();
            var receivers = ListPool<IEnemyDamageReceiver>.Get();
            var damaged = HashSetPool<IEnemyDamageReceiver>.Get();
            try
            {
                Physics2D.OverlapCircle(queryCenter, queryRadius, ContactFilter2D.noFilter, colliders);
                // Resolve each collider's receiver once instead of once per circle.
                for (var i = 0; i < colliders.Count; i++)
                    receivers.Add(colliders[i] != null ? colliders[i].GetComponentInParent<IEnemyDamageReceiver>() : null);
                var hits = 0;
                var radiusSquared = circleRadius * circleRadius;
                for (var circle = 0; circle < circleCenters.Count; circle++)
                {
                    var center = circleCenters[circle];
                    damaged.Clear();
                    for (var i = 0; i < colliders.Count; i++)
                    {
                        var collider = colliders[i];
                        var receiver = receivers[i];
                        // A lethal hit from an earlier circle may disable or destroy the collider (pool return).
                        if (receiver == null || collider == null || !collider.enabled || !collider.gameObject.activeInHierarchy) continue;
                        if ((collider.ClosestPoint(center) - center).sqrMagnitude > radiusSquared) continue;
                        var radial = receiver.Position - center;
                        var before = damaged.Count;
                        ApplyOnce(receiver, damage.WithDirection(radial.x, radial.y), damaged);
                        hits += damaged.Count - before;
                    }
                }
                return hits;
            }
            finally
            {
                ListPool<Collider2D>.Release(colliders);
                ListPool<IEnemyDamageReceiver>.Release(receivers);
                HashSetPool<IEnemyDamageReceiver>.Release(damaged);
            }
        }

        private static void ApplyOnce(
            IEnemyDamageReceiver receiver,
            EnemyDamageRequest damage,
            HashSet<IEnemyDamageReceiver> damaged)
        {
            if (receiver == null || !receiver.IsAlive || !damaged.Add(receiver))
                return;

            receiver.ApplyDamage(damage);
        }
    }
}
