using System;
using Game.Content;
using UnityEngine;

namespace Game.ActiveSkill
{
    /// <summary>World-space selection configuration. Zero radius means unrestricted for nearest targeting only.</summary>
    public sealed class ActiveSkillTargetingProfile
    {
        public ActiveSkillTargetingMode Mode { get; }
        public float Radius { get; }
        public int? RandomSeed { get; }
        public Vector2 InitialDirection { get; }
        public float RotationPerActivationDegrees { get; }
        public float ActionSpeedBonus { get; }
        public ActiveSkillTargetingProfile(ActiveSkillTargetingMode mode, float radius = 0f,
            int? randomSeed = null, float initialDirectionDegrees = 0f, float actionSpeedBonus = 0f, float rotationPerActivationDegrees = 0f)
        {
            if (!Enum.IsDefined(typeof(ActiveSkillTargetingMode), mode)) throw new ArgumentOutOfRangeException(nameof(mode));
            NumericValidation.ValidateNonNegative(radius, nameof(radius));
            NumericValidation.ValidateFinite(initialDirectionDegrees, nameof(initialDirectionDegrees));
            NumericValidation.ValidateNonNegative(actionSpeedBonus, nameof(actionSpeedBonus));
            if (mode == ActiveSkillTargetingMode.RandomEnemy)
            {
                NumericValidation.ValidatePositive(radius, nameof(radius));
                if (!randomSeed.HasValue) throw new ArgumentException("Random targeting requires an explicit seed.");
            }
            NumericValidation.ValidateFinite(rotationPerActivationDegrees, nameof(rotationPerActivationDegrees));
            RotationPerActivationDegrees = rotationPerActivationDegrees;
            Mode = mode;
            Radius = radius;
            RandomSeed = randomSeed;
            var radians = initialDirectionDegrees * Mathf.Deg2Rad;
            InitialDirection = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            ActionSpeedBonus = actionSpeedBonus;
        }
    }
}
