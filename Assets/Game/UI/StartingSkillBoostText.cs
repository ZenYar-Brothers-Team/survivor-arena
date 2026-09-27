using System.Collections.Generic;
using Game.Character;

namespace Game.UI
{
    /// <summary>DECISION-0075: short text of a character's starting-skill specialization, e.g. " (+60% damage, +25% action speed)".</summary>
    public static class StartingSkillBoostText
    {
        public static string Describe(CharacterStatModifier boost)
        {
            var parts = new List<string>();
            if (boost.ActiveSkillDamageMultiplierBonus > 0f) parts.Add($"+{boost.ActiveSkillDamageMultiplierBonus * 100f:0}% damage");
            if (boost.ActionSpeedBonus > 0f) parts.Add($"+{boost.ActionSpeedBonus * 100f:0}% action speed");
            if (boost.EffectSizeMultiplierBonus > 0f) parts.Add($"+{boost.EffectSizeMultiplierBonus * 100f:0}% size");
            if (boost.EffectRangeMultiplierBonus > 0f) parts.Add($"+{boost.EffectRangeMultiplierBonus * 100f:0}% range");
            return parts.Count == 0 ? string.Empty : " (" + string.Join(", ", parts) + ")";
        }
    }
}
