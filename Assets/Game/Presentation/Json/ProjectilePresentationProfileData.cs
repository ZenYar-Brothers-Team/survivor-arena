namespace Game.Presentation.Json
{
    /// <summary>JSON DTO for a projectile sprite's visual motion and impact settings.</summary>
    public sealed class ProjectilePresentationProfileData
    {
        public float VisualScale { get; set; }
        public float SpinDegreesPerSecond { get; set; }
        public float ImpactDurationSeconds { get; set; }
        public float FlashSize { get; set; }
        public float FlashRed { get; set; }
        public float FlashGreen { get; set; }
        public float FlashBlue { get; set; }
        public float FlashAlpha { get; set; }
        public int ParticleCount { get; set; }
        public float ParticleSize { get; set; }
        public float ParticleSpeed { get; set; }
        public float ParticleRed { get; set; }
        public float ParticleGreen { get; set; }
        public float ParticleBlue { get; set; }
        public float ParticleAlpha { get; set; }
        public ExplosionPresentationProfileData Explosion { get; set; }
        public ProjectileThreatHaloData ThreatHalo { get; set; }
    }
}
