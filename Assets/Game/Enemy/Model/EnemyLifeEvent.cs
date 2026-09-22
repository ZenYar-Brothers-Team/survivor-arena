using System;
using Game.Content;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>Immutable per-life data, safe to retain after the pooled object has another life.</summary>
    public sealed class EnemyLifeEvent
    {
        public Guid LifeId { get; }
        public Guid? RunId { get; }
        public ContentId ContentId { get; }
        public EnemyCategory Category { get; }
        public EnemyLifeEventKind Kind { get; }
        public EnemyLifeReason Reason { get; }
        public Vector2 Position { get; }
        public ContentId? DamageSourceId { get; }
        public float ExperienceReward { get; }

        internal EnemyLifeEvent(Guid lifeId, Guid? runId, EnemyDefinition definition, EnemyCategory category,
            EnemyLifeEventKind kind, EnemyLifeReason reason, Vector2 position, ContentId? damageSourceId)
        {
            LifeId = lifeId;
            RunId = runId;
            ContentId = definition.Id;
            Category = category;
            Kind = kind;
            Reason = reason;
            Position = position;
            DamageSourceId = damageSourceId;
            ExperienceReward = definition.ExperienceReward;
        }
    }
}
