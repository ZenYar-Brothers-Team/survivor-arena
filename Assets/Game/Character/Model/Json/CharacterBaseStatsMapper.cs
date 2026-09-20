using System;

namespace Game.Character.Json
{
    /// <summary>One mapping shared by base-stat and roster catalogs, with named missing-field errors.</summary>
    public static class CharacterBaseStatsMapper
    {
        public static CharacterBaseStats Map(CharacterBaseStatsData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            return new CharacterBaseStats(
                Require(data.MaxHealth, nameof(data.MaxHealth)),
                Require(data.MovementSpeed, nameof(data.MovementSpeed)),
                Require(data.ActiveSkillDamageMultiplier, nameof(data.ActiveSkillDamageMultiplier)),
                Require(data.ActiveSkillCooldownMultiplier, nameof(data.ActiveSkillCooldownMultiplier)),
                Require(data.IncomingDamageMultiplier, nameof(data.IncomingDamageMultiplier)),
                Require(data.HealthRestorationMultiplier, nameof(data.HealthRestorationMultiplier)),
                Require(data.HealthRegenerationPerSecond, nameof(data.HealthRegenerationPerSecond)),
                Require(data.DisappearingXpRecovery, nameof(data.DisappearingXpRecovery)),
                Require(data.PickedUpXpMultiplier, nameof(data.PickedUpXpMultiplier)),
                Require(data.XpDropLifetimeBonusSeconds, nameof(data.XpDropLifetimeBonusSeconds)),
                Require(data.PickupRadius, nameof(data.PickupRadius)),
                Require(data.KnockbackResistance, nameof(data.KnockbackResistance)),
                Require(data.OutgoingKnockbackBonus, nameof(data.OutgoingKnockbackBonus)),
                Require(data.EffectSizeMultiplier, nameof(data.EffectSizeMultiplier)),
                Require(data.EffectRangeMultiplier, nameof(data.EffectRangeMultiplier)),
                Require(data.PotionDropMultiplier, nameof(data.PotionDropMultiplier)),
                Require(data.LowHealthDamageMaxBonus, nameof(data.LowHealthDamageMaxBonus)));
        }

        private static float Require(float? value, string name)
        {
            return value ?? throw new InvalidOperationException($"Required character stat '{name}' is missing.");
        }
    }
}
