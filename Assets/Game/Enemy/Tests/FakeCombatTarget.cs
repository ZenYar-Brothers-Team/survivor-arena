using System;
using Game.Content;
using UnityEngine;

namespace Game.Enemy.Tests
{
    public sealed class FakeCombatTarget : IEnemyLifeTarget
    {
        public Guid LifeId { get; set; } = Guid.NewGuid();
        public ContentId ContentId => new ContentId("FIXTURE-TARGET");
        public EnemyCategory Category { get; set; }
        public bool IsAlive { get; set; } = true;
        public Vector2 Position { get; set; }
        public float ApplyDamage(EnemyDamageRequest request) => request.Amount;
    }
}
