using System;
using System.Collections.Generic;
using Game.Character;
using Game.Content;
using Game.Presentation;

namespace Game.Progression
{
    public sealed class PassiveProgressionDefinition : BuildEntryDefinition, IReferencesContent
    {
        private readonly CharacterStatModifier[] _levels;

        public IReadOnlyList<CharacterStatModifier> Levels => _levels;
        public ContentRef<SpriteDefinition> Icon { get; }

        public PassiveProgressionDefinition(
            ContentId id,
            string displayName,
            params CharacterStatModifier[] levels)
            : this(id, displayName, default, levels)
        {
        }

        public PassiveProgressionDefinition(
            ContentId id,
            string displayName,
            ContentRef<SpriteDefinition> icon,
            params CharacterStatModifier[] levels)
            : base(id, BuildEntryKind.PassiveItem, displayName)
        {
            if (levels == null || levels.Length != MaxLevel)
                throw new ArgumentException($"Passive progression requires exactly {MaxLevel} levels.", nameof(levels));

            _levels = (CharacterStatModifier[])levels.Clone();
            Icon = icon;
        }

        public override DraftOptionPreview CreateDraftPreview(int currentLevel, int nextLevel)
        {
            var current = currentLevel == 0 ? default : GetLevel(currentLevel);
            var next = GetLevel(nextLevel);
            var values = new List<DraftValueChange>();
            if (current.MaxHealthMultiplierBonus != 0f || next.MaxHealthMultiplierBonus != 0f)
                values.Add(new DraftValueChange("Max HP", current.MaxHealthMultiplierBonus * 100f, next.MaxHealthMultiplierBonus * 100f, "%"));
            if (current.MovementSpeedMultiplierBonus != 0f || next.MovementSpeedMultiplierBonus != 0f)
                values.Add(new DraftValueChange("Move speed", current.MovementSpeedMultiplierBonus * 100f, next.MovementSpeedMultiplierBonus * 100f, "%"));
            if (current.ActiveSkillDamageMultiplierBonus != 0f || next.ActiveSkillDamageMultiplierBonus != 0f)
                values.Add(new DraftValueChange("Damage", current.ActiveSkillDamageMultiplierBonus * 100f, next.ActiveSkillDamageMultiplierBonus * 100f, "%"));
            if (current.ActionSpeedBonus != 0f || next.ActionSpeedBonus != 0f)
                values.Add(new DraftValueChange("Action speed", current.ActionSpeedBonus * 100f, next.ActionSpeedBonus * 100f, "%"));
            if (current.IncomingDamageReductionBonus != 0f || next.IncomingDamageReductionBonus != 0f)
                values.Add(new DraftValueChange("Damage reduction", current.IncomingDamageReductionBonus * 100f, next.IncomingDamageReductionBonus * 100f, "%"));
            if (current.HealthRestorationMultiplierBonus != 0f || next.HealthRestorationMultiplierBonus != 0f)
                values.Add(new DraftValueChange("Healing", current.HealthRestorationMultiplierBonus * 100f, next.HealthRestorationMultiplierBonus * 100f, "%"));
            if (current.HealthRegenerationPerSecondBonus != 0f || next.HealthRegenerationPerSecondBonus != 0f)
                values.Add(new DraftValueChange("Regeneration", current.HealthRegenerationPerSecondBonus, next.HealthRegenerationPerSecondBonus, " HP/s"));
            if (current.DisappearingXpRecoveryBonus != 0f || next.DisappearingXpRecoveryBonus != 0f)
                values.Add(new DraftValueChange("Expired XP recovery", current.DisappearingXpRecoveryBonus * 100f, next.DisappearingXpRecoveryBonus * 100f, "%"));
            if (current.PickedUpXpMultiplierBonus != 0f || next.PickedUpXpMultiplierBonus != 0f)
                values.Add(new DraftValueChange("Picked-up XP", current.PickedUpXpMultiplierBonus * 100f, next.PickedUpXpMultiplierBonus * 100f, "%"));
            if (current.XpDropLifetimeBonusSeconds != 0f || next.XpDropLifetimeBonusSeconds != 0f)
                values.Add(new DraftValueChange("XP lifetime", current.XpDropLifetimeBonusSeconds, next.XpDropLifetimeBonusSeconds, " s"));
            if (current.KnockbackResistanceBonus != 0f || next.KnockbackResistanceBonus != 0f)
                values.Add(new DraftValueChange("Knockback resistance", current.KnockbackResistanceBonus * 100f, next.KnockbackResistanceBonus * 100f, "%"));
            if (current.OutgoingKnockbackBonus != 0f || next.OutgoingKnockbackBonus != 0f)
                values.Add(new DraftValueChange("Outgoing knockback", current.OutgoingKnockbackBonus * 100f, next.OutgoingKnockbackBonus * 100f, "%"));
            if (current.PickupRadiusMultiplierBonus != 0f || next.PickupRadiusMultiplierBonus != 0f)
                values.Add(new DraftValueChange("Pickup radius", current.PickupRadiusMultiplierBonus * 100f, next.PickupRadiusMultiplierBonus * 100f, "%"));
            if (current.EffectSizeMultiplierBonus != 0f || next.EffectSizeMultiplierBonus != 0f)
                values.Add(new DraftValueChange("Effect size", current.EffectSizeMultiplierBonus * 100f, next.EffectSizeMultiplierBonus * 100f, "%"));
            if (current.EffectRangeMultiplierBonus != 0f || next.EffectRangeMultiplierBonus != 0f)
                values.Add(new DraftValueChange("Effect range", current.EffectRangeMultiplierBonus * 100f, next.EffectRangeMultiplierBonus * 100f, "%"));
            if (current.PotionDropMultiplierBonus != 0f || next.PotionDropMultiplierBonus != 0f)
                values.Add(new DraftValueChange("Potion drops", current.PotionDropMultiplierBonus * 100f, next.PotionDropMultiplierBonus * 100f, "%"));
            if (current.LowHealthDamageMaxBonus != 0f || next.LowHealthDamageMaxBonus != 0f)
                values.Add(new DraftValueChange("Max low-HP damage", current.LowHealthDamageMaxBonus * 100f, next.LowHealthDamageMaxBonus * 100f, "%"));
            return new DraftOptionPreview(currentLevel, nextLevel, values);
        }

        public CharacterStatModifier GetLevel(int level)
        {
            if (level < 1 || level > MaxLevel)
                throw new ArgumentOutOfRangeException(nameof(level));
            return _levels[level - 1];
        }

        public IEnumerable<ContentReference> GetReferencedContent()
        {
            if (Icon.Id.IsValid)
                yield return Icon.ToReference();
        }
    }
}
