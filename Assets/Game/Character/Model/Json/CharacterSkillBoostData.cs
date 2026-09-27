namespace Game.Character.Json
{
    /// <summary>
    /// DECISION-0075: bonuses of the character's starting active skill only. Omitted channels are neutral (0 = no bonus);
    /// the whole object is optional for characters without a specialization.
    /// </summary>
    public sealed class CharacterSkillBoostData
    {
        public float ActiveSkillDamageMultiplierBonus { get; set; }
        public float ActionSpeedBonus { get; set; }
        public float EffectSizeMultiplierBonus { get; set; }
        public float EffectRangeMultiplierBonus { get; set; }
    }
}
