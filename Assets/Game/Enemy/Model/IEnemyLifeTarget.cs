using System;
using Game.Content;

namespace Game.Enemy
{
    /// <summary>Target identity changes on reuse, independently of the Unity object reference.</summary>
    public interface IEnemyLifeTarget : IEnemyDamageReceiver
    {
        Guid LifeId { get; }
        ContentId ContentId { get; }
        EnemyCategory Category { get; }
    }
}
