using System.Collections.Generic;
using Game.Enemy;
using Game.Diagnostics;
using UnityEngine;
using UnityEngine.Pool;

namespace Game.ActiveSkill
{
    public static class EnemyDamageArea
    {
        public static int Apply(
            Vector2 center,
            float radius,
            EnemyDamageRequest damage,
            IEnemyDamageReceiver directTarget = null)
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
