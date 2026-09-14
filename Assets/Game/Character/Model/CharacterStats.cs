using System;
using System.Collections.Generic;

namespace Game.Character
{
    public sealed class CharacterStats
    {
        private const float MinimumMaxHealth = 0.0001f;
        private const float MaximumIncomingDamageReduction = 0.99f;

        private readonly Dictionary<string, CharacterStatModifier> _modifiers =
            new Dictionary<string, CharacterStatModifier>(StringComparer.Ordinal);

        public CharacterBaseStats BaseStats { get; }
        public int ModifierCount => _modifiers.Count;
        public float MaxHealth { get; private set; }
        public float MovementSpeed { get; private set; }
        public float ActiveSkillDamageMultiplier { get; private set; }
        public float ActiveSkillCooldownMultiplier { get; private set; }
        public float IncomingDamageMultiplier { get; private set; }
        public float HealthRestorationMultiplier { get; private set; }
        public float HealthRegenerationPerSecond { get; private set; }
        public float DisappearingXpRecovery { get; private set; }
        public float PickedUpXpMultiplier { get; private set; }
        public float XpDropLifetimeBonusSeconds { get; private set; }

        public event Action Changed;

        public CharacterStats(CharacterBaseStats baseStats)
        {
            baseStats.Validate();
            BaseStats = baseStats;
            Recompute(notify: false);
        }

        public void SetModifier(string sourceKey, CharacterStatModifier modifier)
        {
            if (string.IsNullOrWhiteSpace(sourceKey))
                throw new ArgumentException("Modifier source key cannot be empty.", nameof(sourceKey));

            _modifiers[sourceKey] = modifier;
            Recompute(notify: true);
        }

        public bool RemoveModifier(string sourceKey)
        {
            if (string.IsNullOrWhiteSpace(sourceKey))
                return false;

            if (!_modifiers.Remove(sourceKey))
                return false;

            Recompute(notify: true);
            return true;
        }

        private void Recompute(bool notify)
        {
            var maxHealthBonus = 0f;
            var movementSpeedBonus = 0f;
            var activeSkillDamageBonus = 0f;
            var activeSkillCooldownReduction = 0f;
            var incomingDamageReduction = 0f;
            var healthRestorationBonus = 0f;
            var healthRegenerationBonus = 0f;
            var disappearingXpRecoveryBonus = 0f;
            var pickedUpXpMultiplierBonus = 0f;
            var xpDropLifetimeBonusSeconds = 0f;

            foreach (var modifier in _modifiers.Values)
            {
                maxHealthBonus += modifier.MaxHealthMultiplierBonus;
                movementSpeedBonus += modifier.MovementSpeedMultiplierBonus;
                activeSkillDamageBonus += modifier.ActiveSkillDamageMultiplierBonus;
                activeSkillCooldownReduction += modifier.ActiveSkillCooldownReductionBonus;
                incomingDamageReduction += modifier.IncomingDamageReductionBonus;
                healthRestorationBonus += modifier.HealthRestorationMultiplierBonus;
                healthRegenerationBonus += modifier.HealthRegenerationPerSecondBonus;
                disappearingXpRecoveryBonus += modifier.DisappearingXpRecoveryBonus;
                pickedUpXpMultiplierBonus += modifier.PickedUpXpMultiplierBonus;
                xpDropLifetimeBonusSeconds += modifier.XpDropLifetimeBonusSeconds;
            }

            MaxHealth = Math.Max(MinimumMaxHealth, BaseStats.MaxHealth * NonNegativeFactor(maxHealthBonus));
            MovementSpeed = BaseStats.MovementSpeed * NonNegativeFactor(movementSpeedBonus);
            ActiveSkillDamageMultiplier = BaseStats.ActiveSkillDamageMultiplier * NonNegativeFactor(activeSkillDamageBonus);
            ActiveSkillCooldownMultiplier = BaseStats.ActiveSkillCooldownMultiplier / (1f + activeSkillCooldownReduction);
            IncomingDamageMultiplier = BaseStats.IncomingDamageMultiplier *
                                       (1f - Math.Min(MaximumIncomingDamageReduction, incomingDamageReduction));
            HealthRestorationMultiplier = BaseStats.HealthRestorationMultiplier * NonNegativeFactor(healthRestorationBonus);
            HealthRegenerationPerSecond = Math.Max(0f, BaseStats.HealthRegenerationPerSecond + healthRegenerationBonus);
            DisappearingXpRecovery = Clamp01(BaseStats.DisappearingXpRecovery + disappearingXpRecoveryBonus);
            PickedUpXpMultiplier = BaseStats.PickedUpXpMultiplier * NonNegativeFactor(pickedUpXpMultiplierBonus);
            XpDropLifetimeBonusSeconds = Math.Max(0f, BaseStats.XpDropLifetimeBonusSeconds + xpDropLifetimeBonusSeconds);

            if (notify)
                Changed?.Invoke();
        }

        private static float NonNegativeFactor(float bonus)
        {
            return Math.Max(0f, 1f + bonus);
        }

        private static float Clamp01(float value)
        {
            return Math.Max(0f, Math.Min(1f, value));
        }
    }
}
