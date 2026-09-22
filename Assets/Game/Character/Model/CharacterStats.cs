using System;
using System.Collections.Generic;
using Game.Combat;
using Game.Content;

namespace Game.Character
{
    public sealed class CharacterStats : IHealthProfile
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
        public float ActionSpeedBonus { get; private set; }
        public float PickupRadius { get; private set; }
        public float KnockbackResistance { get; private set; }
        public float OutgoingKnockbackMultiplier { get; private set; }
        public float EffectSizeMultiplier { get; private set; }
        public float EffectRangeMultiplier { get; private set; }
        public float PotionDropMultiplier { get; private set; }
        public float LowHealthDamageMultiplier { get; private set; }
        private float _healthRatio = 1f;

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

            var hadPrevious = _modifiers.TryGetValue(sourceKey, out var previous);
            _modifiers[sourceKey] = modifier;
            try { Recompute(notify: false); }
            catch
            {
                if (hadPrevious) _modifiers[sourceKey] = previous;
                else _modifiers.Remove(sourceKey);
                throw;
            }
            Changed?.Invoke();
        }

        public bool RemoveModifier(string sourceKey)
        {
            if (string.IsNullOrWhiteSpace(sourceKey))
                return false;

            if (!_modifiers.TryGetValue(sourceKey, out var previous))
                return false;
            _modifiers.Remove(sourceKey);
            try { Recompute(notify: false); }
            catch { _modifiers.Add(sourceKey, previous); throw; }
            Changed?.Invoke();
            return true;
        }

        private void Recompute(bool notify)
        {
            double maxHealthBonus = 0, movementSpeedBonus = 0, activeSkillDamageBonus = 0;
            double actionSpeedBonus = 0, incomingDamageReduction = 0, healthRestorationBonus = 0;
            double healthRegenerationBonus = 0, disappearingXpRecoveryBonus = 0;
            double pickedUpXpMultiplierBonus = 0, xpDropLifetimeBonusSeconds = 0;
            double pickupRadiusBonus = 0, resistanceBonus = 0, outgoingKnockbackBonus = 0;
            double sizeBonus = 0, rangeBonus = 0, potionBonus = 0, lowHealthMaxBonus = 0;

            foreach (var modifier in _modifiers.Values)
            {
                maxHealthBonus += modifier.MaxHealthMultiplierBonus;
                movementSpeedBonus += modifier.MovementSpeedMultiplierBonus;
                activeSkillDamageBonus += modifier.ActiveSkillDamageMultiplierBonus;
                actionSpeedBonus += modifier.ActionSpeedBonus;
                incomingDamageReduction += modifier.IncomingDamageReductionBonus;
                healthRestorationBonus += modifier.HealthRestorationMultiplierBonus;
                healthRegenerationBonus += modifier.HealthRegenerationPerSecondBonus;
                disappearingXpRecoveryBonus += modifier.DisappearingXpRecoveryBonus;
                pickedUpXpMultiplierBonus += modifier.PickedUpXpMultiplierBonus;
                xpDropLifetimeBonusSeconds += modifier.XpDropLifetimeBonusSeconds;
                pickupRadiusBonus += modifier.PickupRadiusMultiplierBonus;
                resistanceBonus += modifier.KnockbackResistanceBonus;
                outgoingKnockbackBonus += modifier.OutgoingKnockbackBonus;
                sizeBonus += modifier.EffectSizeMultiplierBonus;
                rangeBonus += modifier.EffectRangeMultiplierBonus;
                potionBonus += modifier.PotionDropMultiplierBonus;
                lowHealthMaxBonus += modifier.LowHealthDamageMaxBonus;
            }

            // Validate the complete candidate before publishing any fields; overflow cannot poison live stats.
            var candidate = new CharacterBaseStats(
                (float)Math.Max(MinimumMaxHealth, BaseStats.MaxHealth * NonNegativeFactor(maxHealthBonus)),
                (float)(BaseStats.MovementSpeed * NonNegativeFactor(movementSpeedBonus)),
                (float)(BaseStats.ActiveSkillDamageMultiplier * NonNegativeFactor(activeSkillDamageBonus)),
                (float)(BaseStats.ActiveSkillCooldownMultiplier / (1d + actionSpeedBonus)),
                (float)(BaseStats.IncomingDamageMultiplier * (1d - Math.Min(MaximumIncomingDamageReduction, incomingDamageReduction))),
                (float)(BaseStats.HealthRestorationMultiplier * NonNegativeFactor(healthRestorationBonus)),
                (float)Math.Max(0d, BaseStats.HealthRegenerationPerSecond + healthRegenerationBonus),
                (float)Clamp01(BaseStats.DisappearingXpRecovery + disappearingXpRecoveryBonus),
                (float)(BaseStats.PickedUpXpMultiplier * NonNegativeFactor(pickedUpXpMultiplierBonus)),
                (float)Math.Max(0d, BaseStats.XpDropLifetimeBonusSeconds + xpDropLifetimeBonusSeconds),
                (float)(BaseStats.PickupRadius * (1d + pickupRadiusBonus)),
                (float)Clamp01(BaseStats.KnockbackResistance + resistanceBonus),
                (float)(BaseStats.OutgoingKnockbackBonus + outgoingKnockbackBonus),
                (float)(BaseStats.EffectSizeMultiplier * (1d + sizeBonus)),
                (float)(BaseStats.EffectRangeMultiplier * (1d + rangeBonus)),
                (float)(BaseStats.PotionDropMultiplier * (1d + potionBonus)),
                (float)(BaseStats.LowHealthDamageMaxBonus + lowHealthMaxBonus));
            var lowHealthDamage = (float)(1d + candidate.LowHealthDamageMaxBonus * Math.Min(1d, (1d - _healthRatio) / 0.9d));
            NumericValidation.ValidateFinite((float)actionSpeedBonus, nameof(ActionSpeedBonus));
            NumericValidation.ValidateFinite(lowHealthDamage, nameof(LowHealthDamageMultiplier));

            MaxHealth = candidate.MaxHealth;
            MovementSpeed = candidate.MovementSpeed;
            ActiveSkillDamageMultiplier = candidate.ActiveSkillDamageMultiplier;
            ActiveSkillCooldownMultiplier = candidate.ActiveSkillCooldownMultiplier;
            IncomingDamageMultiplier = candidate.IncomingDamageMultiplier;
            HealthRestorationMultiplier = candidate.HealthRestorationMultiplier;
            HealthRegenerationPerSecond = candidate.HealthRegenerationPerSecond;
            DisappearingXpRecovery = candidate.DisappearingXpRecovery;
            PickedUpXpMultiplier = candidate.PickedUpXpMultiplier;
            XpDropLifetimeBonusSeconds = candidate.XpDropLifetimeBonusSeconds;
            PickupRadius = candidate.PickupRadius;
            KnockbackResistance = candidate.KnockbackResistance;
            OutgoingKnockbackMultiplier = 1f + candidate.OutgoingKnockbackBonus;
            EffectSizeMultiplier = candidate.EffectSizeMultiplier;
            EffectRangeMultiplier = candidate.EffectRangeMultiplier;
            PotionDropMultiplier = candidate.PotionDropMultiplier;
            LowHealthDamageMultiplier = lowHealthDamage;
            ActionSpeedBonus = (float)actionSpeedBonus;

            if (notify)
                Changed?.Invoke();
        }

        /// <summary>Health owns HP; this input only updates the PASSIVE-014 curve, not hit-time policy.</summary>
        public void UpdateHealthRatio(float ratio)
        {
            NumericValidation.ValidateRange(ratio, 0f, 1f, nameof(ratio));
            if (_healthRatio == ratio) return;
            var previous = _healthRatio;
            var previousMultiplier = LowHealthDamageMultiplier;
            _healthRatio = ratio;
            try { Recompute(notify: false); }
            catch { _healthRatio = previous; throw; }
            if (LowHealthDamageMultiplier != previousMultiplier) Changed?.Invoke();
        }

        private static double NonNegativeFactor(double bonus)
        {
            return Math.Max(0f, 1f + bonus);
        }

        private static double Clamp01(double value)
        {
            return Math.Max(0f, Math.Min(1f, value));
        }
    }
}
