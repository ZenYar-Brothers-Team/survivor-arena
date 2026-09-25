namespace Game.Presentation.Json
{
    public sealed class SkillWorldEffectProfileData
    {
        public string SkillId { get; set; }
        public SkillWorldEffectKind Kind { get; set; }
        public float[] Color { get; set; }
        public float[] ImpactColor { get; set; }
        public float Thickness { get; set; }
        public float FadeSeconds { get; set; }
        /// <summary>Optional strike light pillar; omitted or 0 means none.</summary>
        public float PillarWidth { get; set; }
        public float PillarHeight { get; set; }
    }
}
