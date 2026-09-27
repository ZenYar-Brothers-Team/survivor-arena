using UnityEngine;

namespace Game.Enemy
{
    public readonly struct EnemyShotCommand
    {
        public Vector2 Direction { get; }
        public bool IsExplosive { get; }
        /// <summary>Profile of this shot when it differs from the shooter's current attack (dash-end volleys); null otherwise.</summary>
        public EnemyAttackProfile Attack { get; }

        public EnemyShotCommand(Vector2 direction, bool isExplosive, EnemyAttackProfile attack = null)
        {
            Direction = direction;
            IsExplosive = isExplosive;
            Attack = attack;
        }
    }
}
