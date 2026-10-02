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

        // DECISION-0148: mechanic channels of the starting skill (see SkillMechanicBonus); omitted = none.
        public int ExtraProjectiles { get; set; }
        public int ExtraPierce { get; set; }
        public int ExtraChainTargets { get; set; }
        public int ExtraRicochets { get; set; }
        public int ExtraMines { get; set; }
        public int ExtraStrikes { get; set; }
        public float ExtraStrikeDelaySeconds { get; set; }
        public float ExtraProjectileSpreadDegrees { get; set; }
    }
}
