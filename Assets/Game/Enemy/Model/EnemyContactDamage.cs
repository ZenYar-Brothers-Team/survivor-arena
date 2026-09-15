using System;
using Game.Combat;
using Game.Run;

namespace Game.Enemy
{
    public static class EnemyContactDamage
    {
        public static float Apply(float amount, Health target, RunState runState)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (float.IsNaN(amount) || float.IsInfinity(amount) || amount < 0f)
                throw new ArgumentOutOfRangeException(nameof(amount), "Damage must be finite and non-negative.");

            if (runState != RunState.Running)
                return 0f;

            return target.TakeDamage(amount);
        }
    }
}
