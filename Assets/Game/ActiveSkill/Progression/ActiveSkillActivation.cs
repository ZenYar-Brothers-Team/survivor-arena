using System;
using Game.Combat;
using Game.Content;
using Game.Enemy;
using UnityEngine;

namespace Game.ActiveSkill
{
    public readonly struct ActiveSkillActivation
    {
        public ContentId SourceId { get; }
        public CombatSource Source { get; }
        public float OutgoingKnockbackMultiplier { get; }
        public EnemyTargetLife TargetLife { get; }
        public int Level { get; }
        public Vector2 Origin { get; }
        public Vector2 AimDirection { get; }
        public Vector2 AimPoint { get; }
        public IEnemyDamageReceiver InitialTarget { get; }
        public float Damage { get; }
        public ActiveSkillLevelDefinition LevelDefinition { get; }
        public Transform OwnerTransform { get; }

        public ActiveSkillActivation(
            ContentId sourceId,
            int level,
            Vector2 origin,
            Vector2 aimDirection,
            IEnemyDamageReceiver initialTarget,
            float damage,
            ActiveSkillLevelDefinition levelDefinition,
            Transform ownerTransform,
            CombatIdentity owner = default,
            float outgoingKnockbackMultiplier = 1f)
        {
            SourceId = sourceId;
            Source = new CombatSource(owner, sourceId, CombatSourceOrigin.ActiveSkill, level);
            NumericValidation.ValidateNonNegative(outgoingKnockbackMultiplier, nameof(outgoingKnockbackMultiplier));
            OutgoingKnockbackMultiplier = outgoingKnockbackMultiplier;
            TargetLife = new EnemyTargetLife(initialTarget);
            Level = level;
            Origin = origin;
            AimDirection = aimDirection.sqrMagnitude > Mathf.Epsilon ? aimDirection.normalized : Vector2.right;
            AimPoint = initialTarget != null ? initialTarget.Position : origin;
            InitialTarget = initialTarget;
            Damage = damage;
            LevelDefinition = levelDefinition;
            OwnerTransform = ownerTransform;
        }
    }
}
