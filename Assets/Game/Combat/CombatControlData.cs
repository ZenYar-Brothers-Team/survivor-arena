namespace Game.Combat
{
    /// <summary>Optional JSON authoring layer. Absent controls mean the domain's neutral profile.</summary>
    public sealed class CombatControlData
    {
        public float? KnockbackDistance { get; set; }
        public float? KnockbackSeconds { get; set; }
        public float? SlowFraction { get; set; }
        public float? SlowSeconds { get; set; }
        public string Channel { get; set; }

        public CombatControlProfile ToProfile()
        {
            var none = CombatControlProfile.None;
            return new CombatControlProfile(
                KnockbackDistance ?? none.KnockbackDistance,
                KnockbackSeconds ?? none.KnockbackSeconds,
                SlowFraction ?? none.SlowFraction,
                SlowSeconds ?? none.SlowSeconds,
                Channel ?? none.Channel);
        }
    }
}
