using UnityEngine;

namespace Game.Enemy
{
    public readonly struct EnemyShotCommand
    {
        public Vector2 Direction { get; }
        public bool IsExplosive { get; }

        public EnemyShotCommand(Vector2 direction, bool isExplosive)
        {
            Direction = direction;
            IsExplosive = isExplosive;
        }
    }
}
