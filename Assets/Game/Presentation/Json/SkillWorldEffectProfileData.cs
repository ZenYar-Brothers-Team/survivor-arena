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
        /// <summary>Optional ConeArc outward travel time, within FadeSeconds; 0 means no travel.</summary>
        public float ExpansionSeconds { get; set; }
        public float BandFraction { get; set; }
        public float ContourVariation { get; set; }
        public int AccentCount { get; set; }
        public float AccentLengthFraction { get; set; }
        public float AccentWidthRadians { get; set; }
        public float TailFadePower { get; set; }
        /// <summary>Optional strike light pillar; omitted or 0 means none.</summary>
        public float PillarWidth { get; set; }
        public float PillarHeight { get; set; }
        /// <summary>Optional: pillar appears this many seconds before the impact; omitted = with it.</summary>
        public float PillarLeadSeconds { get; set; }
        /// <summary>Optional beam pulse: period seconds and width breathing depth; both omitted = steady beam.</summary>
        public float PulseSeconds { get; set; }
        public float PulseDepth { get; set; }
    }
}
