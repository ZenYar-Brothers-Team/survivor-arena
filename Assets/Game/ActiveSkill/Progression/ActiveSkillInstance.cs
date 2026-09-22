using System;
using System.Collections.Generic;
using Game.Diagnostics;
using Game.Character;
using Game.Combat;
using Game.Enemy;
using UnityEngine;

namespace Game.ActiveSkill
{
    public sealed class ActiveSkillInstance
    {
        private readonly ActiveSkillCooldown _cooldown = new ActiveSkillCooldown();
        private readonly List<IEnemyDamageReceiver> _targets = new List<IEnemyDamageReceiver>();
        private readonly System.Random _random;
        private Vector2 _lastDirection;
        public SkillHitLedger HitLedger { get; } = new SkillHitLedger();
        public Vector2 LastAimDirection { get; private set; }
        public Vector2 LastAimPoint { get; private set; }

        public ActiveSkillProgressionDefinition Definition { get; }
        public int Level { get; private set; } = 1;
        public int TriggerCount { get; private set; }

        public ActiveSkillInstance(ActiveSkillProgressionDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            var targeting = definition.GetLevel(1).Targeting;
            _lastDirection = targeting.InitialDirection;
            if (targeting.RandomSeed.HasValue) _random = new System.Random(targeting.RandomSeed.Value);
        }

        public void SetLevel(int level)
        {
            if (level < Level || level > ActiveSkillProgressionDefinition.MaxLevel)
                throw new ArgumentOutOfRangeException(nameof(level), "Skill level can only advance up to level 6.");
            Level = level;
        }

        public bool Tick(
            float deltaTime,
            bool isRunning,
            PlayerCharacterRuntime owner,
            IActiveSkillTargetProvider targetProvider,
            IActiveSkillEffectExecutor executor,
            Vector2 movementDirection = default, CharacterStatModifier skillModifier = default,
            CombatSource? sourceOverride = null, bool forceActivation = false)
        {
            if (owner == null || owner.Stats == null)
                throw new ArgumentNullException(nameof(owner));
            if (targetProvider == null)
                throw new ArgumentNullException(nameof(targetProvider));
            if (executor == null)
                throw new ArgumentNullException(nameof(executor));

            HitLedger.Tick(deltaTime, isRunning);
            if (isRunning && movementDirection.sqrMagnitude > Mathf.Epsilon) _lastDirection = movementDirection.normalized;
            _cooldown.Tick(deltaTime, isRunning);
            if (!isRunning || (!forceActivation && !_cooldown.IsReady))
                return false;

            var isSet = sourceOverride.HasValue && sourceOverride.Value.Origin == CombatSourceOrigin.Set;
            var rangeMultiplier = isSet ? 1f : owner.Stats.EffectRangeMultiplier + owner.Stats.BaseStats.EffectRangeMultiplier * skillModifier.EffectRangeMultiplierBonus;
            var sizeMultiplier = isSet ? 1f : owner.Stats.EffectSizeMultiplier + owner.Stats.BaseStats.EffectSizeMultiplier * skillModifier.EffectSizeMultiplierBonus;
            var damageMultiplier = owner.Stats.ActiveSkillDamageMultiplier + owner.Stats.BaseStats.ActiveSkillDamageMultiplier * skillModifier.ActiveSkillDamageMultiplierBonus;
            var knockbackMultiplier = owner.Stats.OutgoingKnockbackMultiplier + skillModifier.OutgoingKnockbackBonus;
            var levelDefinition = Definition.GetLevel(Level);
            var origin = (Vector2)owner.transform.position;
            IEnemyDamageReceiver target = null;
            var targeting = levelDefinition.Targeting;
            if (targeting.Mode == ActiveSkillTargetingMode.RandomEnemy)
            {
                if (!(targetProvider is IActiveSkillTargetSetProvider setProvider) || _random == null)
                    throw new InvalidOperationException("Random targeting requires a target-set provider and a configured seed.");
                using var guard = PerfGuard.Measure("ActiveSkillInstance.RandomTarget", 1f);
                setProvider.CopyAliveTo(_targets);
                var radius = targeting.Radius * rangeMultiplier;
                var eligible = 0;
                // Reservoir sampling: uniform over all valid world targets, no viewport dependency.
                foreach (var candidate in _targets)
                {
                    if (!new EnemyTargetLife(candidate).IsAlive || (candidate.Position - origin).sqrMagnitude > radius * radius) continue;
                    if (_random.Next(++eligible) == 0) target = candidate;
                }
                if (target == null) return false;
            }
            else if (targeting.Mode == ActiveSkillTargetingMode.NearestEnemy)
            {
                if (!targetProvider.TryGetTarget(origin, out target) || !new EnemyTargetLife(target).IsAlive) return false;
                var radius = targeting.Radius * rangeMultiplier;
                if (radius > 0f && (target.Position - origin).sqrMagnitude > radius * radius) return false;
            }
            var direction = targeting.Mode == ActiveSkillTargetingMode.MovementDirection
                ? _lastDirection : target != null ? target.Position - origin : Vector2.right;
            LastAimDirection = direction.sqrMagnitude > Mathf.Epsilon ? direction.normalized : _lastDirection;
            LastAimPoint = target != null ? target.Position : origin;
            executor.Schedule(new ActiveSkillActivation(
                Definition.Id,
                Level,
                origin,
                direction,
                target,
                levelDefinition.BaseDamage * damageMultiplier * owner.Stats.LowHealthDamageMultiplier,
                levelDefinition,
                owner.transform,
                owner.Identity,
                knockbackMultiplier, sizeMultiplier, rangeMultiplier, _random, HitLedger, TriggerCount * targeting.RotationPerActivationDegrees, sourceOverride));
            if (!forceActivation) _cooldown.Consume(levelDefinition.CooldownSeconds,
                owner.Stats.BaseStats.ActiveSkillCooldownMultiplier / (1f + owner.Stats.ActionSpeedBonus + targeting.ActionSpeedBonus + skillModifier.ActionSpeedBonus));
            TriggerCount++;
            return true;
        }
    }
}
