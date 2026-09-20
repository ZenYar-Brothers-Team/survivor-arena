namespace Game.Combat
{
    /// <summary>Measured in Health before callbacks: max-HP rescale never creates a damage/heal result.</summary>
    public readonly struct HealthChange
    {
        public float Requested { get; }
        public float AfterMitigation { get; }
        public float Actual { get; }
        public bool IsHealing { get; }
        public float Overkill => IsHealing ? 0f : System.Math.Max(0f, AfterMitigation - Actual);
        public HealthChange(float requested, float afterMitigation, float actual, bool isHealing)
        {
            Requested = requested;
            AfterMitigation = afterMitigation;
            Actual = actual;
            IsHealing = isHealing;
        }
    }
}
