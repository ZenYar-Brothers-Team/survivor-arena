namespace Game.Traps.Json
{
    /// <summary>
    /// A run of identical shots: shot i leaves at angleStart + i * angleStep from the volley heading, shifted
    /// lateralStart + i * lateralStep sideways, delayStep * i seconds after the volley starts. Absent offsets mean zero.
    /// </summary>
    public sealed class TrapShotGroupData
    {
        public string Projectile { get; set; }
        public int? Count { get; set; }
        public float? AngleStart { get; set; }
        public float? AngleStep { get; set; }
        public float? LateralStart { get; set; }
        public float? LateralStep { get; set; }
        public float? DelayStep { get; set; }
    }
}
