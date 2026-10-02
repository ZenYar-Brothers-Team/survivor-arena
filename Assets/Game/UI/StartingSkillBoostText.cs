using System.Collections.Generic;
using Game.Character;
using Game.Progression;

namespace Game.UI
{
    /// <summary>DECISION-0075: short text of a character's starting-skill specialization, e.g. " (+60% damage, +25% action speed)".</summary>
    public static class StartingSkillBoostText
    {
        public static string Describe(CharacterStatModifier boost) => Describe(boost, default);

        /// <summary>DECISION-0148: also lists the mechanic the starting skill has from level 1, e.g. "+1 ricochet".</summary>
        public static string Describe(CharacterStatModifier boost, SkillMechanicBonus mechanic)
        {
            var parts = new List<string>();
            if (boost.ActiveSkillDamageMultiplierBonus > 0f) parts.Add($"+{boost.ActiveSkillDamageMultiplierBonus * 100f:0}% damage");
            if (boost.ActionSpeedBonus > 0f) parts.Add($"+{boost.ActionSpeedBonus * 100f:0}% action speed");
            if (boost.EffectSizeMultiplierBonus > 0f) parts.Add($"+{boost.EffectSizeMultiplierBonus * 100f:0}% size");
            if (boost.EffectRangeMultiplierBonus > 0f) parts.Add($"+{boost.EffectRangeMultiplierBonus * 100f:0}% range");
            if (mechanic.ExtraRicochets > 0) parts.Add($"+{mechanic.ExtraRicochets} ricochet");
            if (mechanic.ExtraPierce > 0) parts.Add($"+{mechanic.ExtraPierce} pierce");
            if (mechanic.ExtraChainTargets > 0) parts.Add($"+{mechanic.ExtraChainTargets} chain target");
            if (mechanic.ExtraStrikes > 0) parts.Add($"+{mechanic.ExtraStrikes} strike");
            if (mechanic.ExtraMines > 0) parts.Add($"+{mechanic.ExtraMines} mines");
            if (mechanic.ExtraProjectiles > 0) parts.Add($"+{mechanic.ExtraProjectiles} projectile");
            return parts.Count == 0 ? string.Empty : " (" + string.Join(", ", parts) + ")";
        }
    }
}
