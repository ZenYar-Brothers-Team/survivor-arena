using System.Collections.Generic;
using Game.Enemy;
using UnityEngine;

namespace Game.ActiveSkill
{
    public static class EnemyDamageArea
    {
        private static readonly List<Collider2D> ColliderBuffer = new List<Collider2D>();
        private static readonly HashSet<IEnemyDamageReceiver> DamagedBuffer = new HashSet<IEnemyDamageReceiver>();

        public static int Apply(
            Vector2 center,
            float radius,
            EnemyDamageRequest damage,
            IEnemyDamageReceiver directTarget = null)
        {
            ColliderBuffer.Clear();
            DamagedBuffer.Clear();
            ApplyOnce(directTarget, damage, DamagedBuffer);

            if (radius <= 0f)
                return DamagedBuffer.Count;

            Physics2D.OverlapCircle(center, radius, new ContactFilter2D().NoFilter(), ColliderBuffer);
            for (var i = 0; i < ColliderBuffer.Count; i++)
            {
                var receiver = ColliderBuffer[i].GetComponentInParent<IEnemyDamageReceiver>();
                ApplyOnce(receiver, damage, DamagedBuffer);
            }

            return DamagedBuffer.Count;
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
