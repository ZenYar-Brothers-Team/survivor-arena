using Game.Content;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>DECISION-0142: area modifiers clear on exit; a burst leaves a pause-aware, refreshing speed buff.</summary>
    public sealed class EnemyZoneInfluence
    {
        public float MovementBonus { get; private set; }
        public float ActionBonus { get; private set; }
        public float DamageReduction { get; private set; }
        /// <summary>Area bonus to the damage this enemy deals (0.5 = +50%).</summary>
        public float DamageBonus { get; private set; }
        public float DamageMultiplier => 1f + DamageBonus;
        public float BuffBonus { get; private set; }
        public float BuffRemaining { get; private set; }
        public float BuffDuration { get; private set; }
        public float MovementMultiplier => 1f + MovementBonus + (BuffRemaining > 0f ? BuffBonus : 0f);
        public float ActionMultiplier => 1f + ActionBonus;
        public float BuffRemaining01 => BuffDuration > 0f ? Mathf.Clamp01(BuffRemaining / BuffDuration) : 0f;

        public void SetArea(float movement, float action, float defense)
        {
            NumericValidation.ValidateNonNegativeFinite(movement, nameof(movement));
            NumericValidation.ValidateNonNegativeFinite(action, nameof(action));
            NumericValidation.ValidateNonNegativeFinite(defense, nameof(defense));
            MovementBonus = movement; ActionBonus = action; DamageReduction = Mathf.Clamp(defense, 0f, .95f);
        }
        public void SetDamageBonus(float bonus)
        {
            NumericValidation.ValidateNonNegativeFinite(bonus, nameof(bonus));
            DamageBonus = bonus;
        }
        public void ApplyBurst(float bonus, float seconds)
        {
            NumericValidation.ValidateNonNegativeFinite(bonus, nameof(bonus));
            NumericValidation.ValidatePositive(seconds, nameof(seconds));
            BuffBonus = bonus; BuffDuration = BuffRemaining = seconds;
        }
        public void Tick(float seconds, bool running)
        {
            NumericValidation.ValidateNonNegativeFinite(seconds, nameof(seconds));
            if (running) BuffRemaining = Mathf.Max(0f, BuffRemaining - seconds);
        }
        public void Reset() { SetArea(0f, 0f, 0f); DamageBonus = 0f; BuffBonus = BuffDuration = BuffRemaining = 0f; }
    }
}
