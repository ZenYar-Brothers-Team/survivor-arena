using System;
using System.Collections.Generic;
using Game.Content;
using Game.Enemy;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    public sealed class SkillTestTarget : IEnemyLifeTarget
    {
        public bool IsAlive { get; set; } = true;
        public Vector2 Position { get; set; }
        public Guid LifeId { get; set; } = Guid.NewGuid();
        public ContentId ContentId => "FIXTURE-IP08-TARGET";
        public EnemyCategory Category => EnemyCategory.Ordinary;
        public List<EnemyDamageRequest> Hits { get; } = new List<EnemyDamageRequest>();
        public SkillTestTarget(Vector2 position) { Position = position; }
        public float ApplyDamage(EnemyDamageRequest request) { Hits.Add(request); return request.Amount; }
    }
}
