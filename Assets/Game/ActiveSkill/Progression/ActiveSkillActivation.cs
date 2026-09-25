using System;
using Game.Combat;
using Game.Content;
using Game.Enemy;
using UnityEngine;

namespace Game.ActiveSkill
{
    public readonly struct ActiveSkillActivation
    {
        public float RotationDegrees { get; }
        public float SizeMultiplier { get; }
        public float RangeMultiplier { get; }
        public System.Random Random { get; }
        public SkillHitLedger HitLedger { get; }
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
        /// <summary>Conditional bonuses resolved at activation, applied only against already slowed targets.</summary>
        public SlowedTargetBonus SlowedTargetBonus { get; }
        public float SlowedTargetDamageFactor { get; }

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
            float outgoingKnockbackMultiplier = 1f, float sizeMultiplier = 1f, float rangeMultiplier = 1f,
            System.Random random = null, SkillHitLedger hitLedger = null, float rotationDegrees = 0f, CombatSource? sourceOverride = null,
            SlowedTargetBonus slowedTargetBonus = default, float slowedTargetDamageFactor = 1f,
            Vector2? aimPointOverride = null)
        {
            NumericValidation.ValidatePositive(sizeMultiplier, nameof(sizeMultiplier));
            NumericValidation.ValidatePositive(rangeMultiplier, nameof(rangeMultiplier));
            NumericValidation.ValidateFinite(rotationDegrees, nameof(rotationDegrees));
            RotationDegrees = rotationDegrees;
            SizeMultiplier = sizeMultiplier;
            RangeMultiplier = rangeMultiplier;
            Random = random;
            HitLedger = hitLedger;
            SourceId = sourceId;
            Source = sourceOverride ?? new CombatSource(owner, sourceId, CombatSourceOrigin.ActiveSkill, level);
            NumericValidation.ValidateNonNegative(outgoingKnockbackMultiplier, nameof(outgoingKnockbackMultiplier));
            OutgoingKnockbackMultiplier = outgoingKnockbackMultiplier;
            TargetLife = new EnemyTargetLife(initialTarget);
            Level = level;
            Origin = origin;
            AimDirection = aimDirection.sqrMagnitude > Mathf.Epsilon ? aimDirection.normalized : Vector2.right;
            // A point attack without an on-screen enemy aims at a picked screen point (DECISION-0058).
            AimPoint = initialTarget != null ? initialTarget.Position : aimPointOverride ?? origin;
            InitialTarget = initialTarget;
            Damage = damage;
            LevelDefinition = levelDefinition;
            OwnerTransform = ownerTransform;
            NumericValidation.ValidateNonNegative(slowedTargetDamageFactor, nameof(slowedTargetDamageFactor));
            SlowedTargetBonus = slowedTargetBonus;
            SlowedTargetDamageFactor = slowedTargetDamageFactor;
        }
    }
}
