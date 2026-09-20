using System;
using Game.Character;

namespace Game.UI
{
    /// <summary>A copy of current stat channels for observation, never an owner of gameplay state.</summary>
    public sealed class CharacterStatsViewState
    {
        public float ActionSpeedBonus { get; }
        public float ActiveSkillCooldownMultiplier { get; }
        public float PickupRadius { get; }
        public float KnockbackResistance { get; }
        public float OutgoingKnockbackMultiplier { get; }
        public float EffectSizeMultiplier { get; }
        public float EffectRangeMultiplier { get; }
        public float PotionDropMultiplier { get; }
        public float LowHealthDamageMultiplier { get; }

        public CharacterStatsViewState(CharacterStats stats)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));
            ActionSpeedBonus = stats.ActionSpeedBonus;
            ActiveSkillCooldownMultiplier = stats.ActiveSkillCooldownMultiplier;
            PickupRadius = stats.PickupRadius;
            KnockbackResistance = stats.KnockbackResistance;
            OutgoingKnockbackMultiplier = stats.OutgoingKnockbackMultiplier;
            EffectSizeMultiplier = stats.EffectSizeMultiplier;
            EffectRangeMultiplier = stats.EffectRangeMultiplier;
            PotionDropMultiplier = stats.PotionDropMultiplier;
            LowHealthDamageMultiplier = stats.LowHealthDamageMultiplier;
        }
    }
}
