namespace Game.Telemetry
{
    /// <summary>Value accumulator copied when a snapshot is built. Hits are measured results, not casts.</summary>
    public struct TelemetryCombatTotals
    {
        public long Results;
        public double Attempted;
        public double Applied;
        public double Overkill;
    }
}
