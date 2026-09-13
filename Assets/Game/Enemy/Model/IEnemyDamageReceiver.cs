using UnityEngine;

namespace Game.Enemy
{
    public interface IEnemyDamageReceiver
    {
        bool IsAlive { get; }
        Vector2 Position { get; }
        float ApplyDamage(EnemyDamageRequest request);
    }
}
