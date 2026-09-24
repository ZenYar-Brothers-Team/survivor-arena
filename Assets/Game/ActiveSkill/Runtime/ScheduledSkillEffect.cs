using System;
using System.Collections.Generic;
using Game.Content;
using Game.Combat;
using Game.Diagnostics;
using Game.Enemy;
using Game.Movement;
using Game.Pooling;
using Game.Run;
using UnityEngine;

namespace Game.ActiveSkill
{
    internal sealed class ScheduledSkillEffect
    {
        public ActiveSkillActivation Activation { get; }
        public ActiveSkillActivationWave Wave { get; }
        public IActiveSkillEffect Effect { get; }
        public float KnockbackMultiplier { get; }
        public int TickIndex { get; }
        public Vector2? CenterOverride { get; }
        public float RemainingDelay { get; set; }
        /// <summary>Shared distinct-target set for deferred RandomEnemy strike waves; null for the first wave.</summary>
        public StrikeTargetSet StrikeTargets { get; set; }

        public ScheduledSkillEffect(
            ActiveSkillActivation activation,
            ActiveSkillActivationWave wave,
            IActiveSkillEffect effect,
            float remainingDelay,
            int tickIndex,
            Vector2? centerOverride = null, float knockbackMultiplier = 1f)
        {
            Activation = activation;
            Wave = wave;
            Effect = effect;
            RemainingDelay = remainingDelay;
            TickIndex = tickIndex;
            CenterOverride = centerOverride;
            KnockbackMultiplier = knockbackMultiplier;
        }
    }
}
