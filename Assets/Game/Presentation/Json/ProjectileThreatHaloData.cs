namespace Game.Presentation.Json
{
    /// <summary>JSON DTO for <see cref="ProjectileThreatHaloProfile"/>; every field is required when the halo is present.</summary>
    public sealed class ProjectileThreatHaloData
    {
        public float Scale { get; set; }
        public float Red { get; set; }
        public float Green { get; set; }
        public float Blue { get; set; }
        public float Alpha { get; set; }
        public float PulseSeconds { get; set; }
        public float PulseAmplitude { get; set; }
    }
}
