using System;
using Game.Character;
using Game.Enemy;
using UnityEngine;

namespace Game.ActiveSkill
{
    public sealed class ActiveSkillInstance
    {
        private readonly ActiveSkillCooldown _cooldown = new ActiveSkillCooldown();

        public ActiveSkillProgressionDefinition Definition { get; }
        public int Level { get; private set; } = 1;
        public int TriggerCount { get; private set; }

        public ActiveSkillInstance(ActiveSkillProgressionDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
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
            IActiveSkillEffectExecutor executor)
        {
            if (owner == null || owner.Stats == null)
                throw new ArgumentNullException(nameof(owner));
            if (targetProvider == null)
                throw new ArgumentNullException(nameof(targetProvider));
            if (executor == null)
                throw new ArgumentNullException(nameof(executor));

            _cooldown.Tick(deltaTime, isRunning);
            if (!isRunning || !_cooldown.IsReady)
                return false;

            var levelDefinition = Definition.GetLevel(Level);
            IEnemyDamageReceiver target = null;
            if (levelDefinition.TargetingMode == ActiveSkillTargetingMode.NearestEnemy &&
                !targetProvider.TryGetTarget(owner.transform.position, out target))
            {
                return false;
            }

            var origin = (Vector2)owner.transform.position;
            var direction = target != null ? target.Position - origin : Vector2.right;
            executor.Schedule(new ActiveSkillActivation(
                Definition.Id,
                Level,
                origin,
                direction,
                target,
                levelDefinition.BaseDamage * owner.Stats.ActiveSkillDamageMultiplier,
                levelDefinition,
                owner.transform));
            _cooldown.Consume(levelDefinition.CooldownSeconds, owner.Stats.ActiveSkillCooldownMultiplier);
            TriggerCount++;
            return true;
        }
    }
}
