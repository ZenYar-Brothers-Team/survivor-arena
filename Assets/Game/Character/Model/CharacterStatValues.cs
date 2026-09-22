using System;

namespace Game.Character
{
    public static class CharacterStatValues
    {
        public static float Get(CharacterBaseStats stats, CharacterStatField field)
        {
            switch (field)
            {
                case CharacterStatField.MaxHealth: return stats.MaxHealth;
                case CharacterStatField.MovementSpeed: return stats.MovementSpeed;
                case CharacterStatField.ActiveSkillDamageMultiplier: return stats.ActiveSkillDamageMultiplier;
                case CharacterStatField.ActiveSkillCooldownMultiplier: return stats.ActiveSkillCooldownMultiplier;
                case CharacterStatField.IncomingDamageMultiplier: return stats.IncomingDamageMultiplier;
                case CharacterStatField.HealthRestorationMultiplier: return stats.HealthRestorationMultiplier;
                case CharacterStatField.HealthRegenerationPerSecond: return stats.HealthRegenerationPerSecond;
                case CharacterStatField.DisappearingXpRecovery: return stats.DisappearingXpRecovery;
                case CharacterStatField.PickedUpXpMultiplier: return stats.PickedUpXpMultiplier;
                case CharacterStatField.XpDropLifetimeBonusSeconds: return stats.XpDropLifetimeBonusSeconds;
                case CharacterStatField.PickupRadius: return stats.PickupRadius;
                case CharacterStatField.KnockbackResistance: return stats.KnockbackResistance;
                case CharacterStatField.OutgoingKnockbackBonus: return stats.OutgoingKnockbackBonus;
                case CharacterStatField.EffectSizeMultiplier: return stats.EffectSizeMultiplier;
                case CharacterStatField.EffectRangeMultiplier: return stats.EffectRangeMultiplier;
                case CharacterStatField.PotionDropMultiplier: return stats.PotionDropMultiplier;
                case CharacterStatField.LowHealthDamageMaxBonus: return stats.LowHealthDamageMaxBonus;
                default: throw new ArgumentOutOfRangeException(nameof(field));
            }
        }
    }
}
