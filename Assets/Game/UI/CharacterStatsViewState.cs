using System;
using Game.Character;
using Game.Combat;

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
        public float KnockbackRemaining { get; }

        public CharacterStatsViewState(CharacterStats stats, CombatControlState controls = null)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));
            KnockbackRemaining = controls?.KnockbackRemaining ?? 0f;
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
