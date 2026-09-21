namespace Game.Progression.Json
{
    public sealed class SetEffectData
    {
        public SetEffectKind? Kind { get; set; }
        public CharacterStatModifierData Modifier { get; set; }
        public string Skill { get; set; }
        public string AttackTemplate { get; set; }
        public float? CooldownSeconds { get; set; }
        public int? ActivationCount { get; set; }
        public float? HealFraction { get; set; }
        public float? BuffSeconds { get; set; }
    }
}
