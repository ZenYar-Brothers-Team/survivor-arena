namespace Game.Presentation.Json
{
    /// <summary>JSON DTO for the shared particle-only explosion presentation.</summary>
    public sealed class ExplosionPresentationProfileData
    {
        public float DurationSeconds { get; set; }
        public float FlashSizeMultiplier { get; set; }
        public float FlashRed { get; set; }
        public float FlashGreen { get; set; }
        public float FlashBlue { get; set; }
        public float FlashAlpha { get; set; }
        public int ParticleCount { get; set; }
        public float ParticleSizeMultiplier { get; set; }
        public float ParticleSpeedMultiplier { get; set; }
        public float ParticleRed { get; set; }
        public float ParticleGreen { get; set; }
        public float ParticleBlue { get; set; }
        public float ParticleAlpha { get; set; }
    }
}
