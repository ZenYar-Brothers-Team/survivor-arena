using System;

namespace Game.Enemy
{
    /// <summary>A reference plus its captured life, preventing a delayed attack from tracking a pooled replacement.</summary>
    public readonly struct EnemyTargetLife : IEquatable<EnemyTargetLife>
    {
        public IEnemyDamageReceiver Target { get; }
        private readonly Guid? _lifeId;
        public bool IsAlive => Target != null && (!(Target is UnityEngine.Object obj) || obj != null) &&
            Target.IsAlive && (!(Target is IEnemyLifeTarget life) || life.LifeId == _lifeId);

        public EnemyTargetLife(IEnemyDamageReceiver target)
        {
            Target = target;
            _lifeId = (target as IEnemyLifeTarget)?.LifeId;
        }

        public bool Equals(EnemyTargetLife other) => ReferenceEquals(Target, other.Target) && _lifeId == other._lifeId;
        public override bool Equals(object obj) => obj is EnemyTargetLife other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Target, _lifeId);
    }
}
