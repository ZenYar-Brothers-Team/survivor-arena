using System;
using System.Collections.Generic;
using Game.Character;
using Game.Content;
namespace Game.Progression
{
    /// <summary>Fixture framework values: seconds, fraction of max HP, and additive stat bonuses.</summary>
    public sealed class SetEffectDefinition
    {
        public SetEffectKind Kind { get; }
        public CharacterStatModifier Modifier { get; }
        public ContentId? Skill { get; }
        public ContentId? AttackTemplate { get; }
        public float CooldownSeconds { get; }
        public int ActivationCount { get; }
        public float HealFraction { get; }
        public float BuffSeconds { get; }
        public float SlowFraction { get; }
        public float SlowSeconds { get; }
        public float RefreshSeconds { get; }
        /// <summary>IndependentAttack only: generic size/range stats also scale this set attack (SET-017).</summary>
        public bool ScalesWithSizeAndRange { get; }
        public SetEffectDefinition(SetEffectKind kind, CharacterStatModifier modifier = default,
            ContentId? skill = null, ContentId? attackTemplate = null, float cooldownSeconds = 0,
            int activationCount = 0, float healFraction = 0, float buffSeconds = 0,
            float slowFraction = 0, float slowSeconds = 0, float refreshSeconds = 0, bool scalesWithSizeAndRange = false)
        {
            if (!Enum.IsDefined(typeof(SetEffectKind), kind)) throw new ArgumentOutOfRangeException(nameof(kind));
            NumericValidation.ValidateNonNegative(cooldownSeconds, nameof(cooldownSeconds));
            NumericValidation.ValidateNonNegative(buffSeconds, nameof(buffSeconds));
            NumericValidation.ValidateRange(healFraction, 0f, 1f, nameof(healFraction));
            if (kind == SetEffectKind.SkillTransform && (!skill.HasValue || !skill.Value.IsValid))
                throw new ArgumentException("Skill transform requires a target skill.");
            if (kind == SetEffectKind.IndependentAttack || kind == SetEffectKind.RewardProc || kind == SetEffectKind.ActivationProc)
            {
                if (!attackTemplate.HasValue || !attackTemplate.Value.IsValid) throw new ArgumentException("Attack effect requires a template.");
                NumericValidation.ValidatePositive(cooldownSeconds, nameof(cooldownSeconds));
            }
            if (kind == SetEffectKind.ActivationProc) NumericValidation.ValidateCount(activationCount, nameof(activationCount));
            if (kind == SetEffectKind.LevelHeal) NumericValidation.ValidatePositive(healFraction, nameof(healFraction));
            if (kind == SetEffectKind.SlowedTargetBonus)
            {
                var supported = new CharacterStatModifier(activeSkillDamageMultiplierBonus: modifier.ActiveSkillDamageMultiplierBonus,
                    outgoingKnockbackBonus: modifier.OutgoingKnockbackBonus);
                if (!modifier.Equals(supported)) throw new ArgumentException("Slowed-target bonus uses only damage and knockback channels.", nameof(modifier));
                NumericValidation.ValidateNonNegative(modifier.ActiveSkillDamageMultiplierBonus, nameof(modifier));
                NumericValidation.ValidateNonNegative(modifier.OutgoingKnockbackBonus, nameof(modifier));
                if (modifier.ActiveSkillDamageMultiplierBonus > 0 && (!skill.HasValue || !skill.Value.IsValid))
                    throw new ArgumentException("A slowed-target damage bonus must name its skill.");
            }
            if (kind == SetEffectKind.OrbitSlowAura)
            {
                if (!skill.HasValue || !skill.Value.IsValid) throw new ArgumentException("Orbit slow aura requires its orbit skill.");
                NumericValidation.ValidateRange(slowFraction, 0f, 1f, nameof(slowFraction));
                NumericValidation.ValidatePositive(slowFraction, nameof(slowFraction));
                NumericValidation.ValidatePositive(slowSeconds, nameof(slowSeconds));
                NumericValidation.ValidatePositive(refreshSeconds, nameof(refreshSeconds));
            }
            if (scalesWithSizeAndRange && kind != SetEffectKind.IndependentAttack)
                throw new ArgumentException("Only independent set attacks can opt into size/range scaling.");
            if (kind == SetEffectKind.SkillTransform)
            {
                var supported = new CharacterStatModifier(activeSkillDamageMultiplierBonus: modifier.ActiveSkillDamageMultiplierBonus,
                    actionSpeedBonus: modifier.ActionSpeedBonus, outgoingKnockbackBonus: modifier.OutgoingKnockbackBonus,
                    effectSizeMultiplierBonus: modifier.EffectSizeMultiplierBonus, effectRangeMultiplierBonus: modifier.EffectRangeMultiplierBonus);
                if (!modifier.Equals(supported)) throw new ArgumentException("Unsupported skill-specific modifier channel.", nameof(modifier));
                NumericValidation.ValidateNonNegative(modifier.ActiveSkillDamageMultiplierBonus, nameof(modifier));
            }
            Kind = kind; Modifier = modifier; Skill = skill; AttackTemplate = attackTemplate;
            CooldownSeconds = cooldownSeconds; ActivationCount = activationCount; HealFraction = healFraction; BuffSeconds = buffSeconds;
            SlowFraction = slowFraction; SlowSeconds = slowSeconds; RefreshSeconds = refreshSeconds; ScalesWithSizeAndRange = scalesWithSizeAndRange;
        }
    }
}
