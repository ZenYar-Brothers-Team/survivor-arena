using System.Collections.Generic;
using Game.Enemy;
using UnityEngine;

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
            var damaged = new HashSet<IEnemyDamageReceiver>();
            ApplyOnce(directTarget, damage, damaged);

            if (radius <= 0f)
                return damaged.Count;

            var colliders = Physics2D.OverlapCircleAll(center, radius);
            for (var i = 0; i < colliders.Length; i++)
            {
                var receiver = colliders[i].GetComponentInParent<IEnemyDamageReceiver>();
                ApplyOnce(receiver, damage, damaged);
            }

            return damaged.Count;
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
