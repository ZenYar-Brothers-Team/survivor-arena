using Game.Combat;
using Game.Content;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>One boss hazard hit on the player: damage, controls and push direction, attributed to its source step.</summary>
    public readonly struct BossHazardHit
    {
        public float Damage { get; }
        public CombatControlProfile Controls { get; }
        public Vector2 Direction { get; }
        public ContentId SourceId { get; }
        public BossSpecialKind Kind { get; }

        public BossHazardHit(float damage, CombatControlProfile controls, Vector2 direction, ContentId sourceId, BossSpecialKind kind)
        {
            Damage = damage;
            Controls = controls ?? CombatControlProfile.None;
            Direction = direction;
            SourceId = sourceId;
            Kind = kind;
        }
    }
}
