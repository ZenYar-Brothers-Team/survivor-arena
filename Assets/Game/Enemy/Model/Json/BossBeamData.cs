using Game.Combat;

namespace Game.Enemy.Json
{
    /// <summary>Boss beam (DECISION-0066); every field is required.</summary>
    public sealed class BossBeamData
    {
        public float[] AnglesDegrees { get; set; }
        public float? Length { get; set; }
        public float? Width { get; set; }
        public float? TelegraphSeconds { get; set; }
        public float? ActiveSeconds { get; set; }
        public float? SweepDegrees { get; set; }
        public float? Damage { get; set; }
        public CombatControlData Controls { get; set; }
        public float[] TelegraphColor { get; set; }
        public float[] BeamColor { get; set; }
    }
}
