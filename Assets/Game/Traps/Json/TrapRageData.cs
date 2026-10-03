namespace Game.Traps.Json
{
    /// <summary>Authoring values of the rage ramp: how fast a trap fires at most and how long that takes (DECISION-0156).</summary>
    public sealed class TrapRageData
    {
        public float? MaxMultiplier { get; set; }
        public float? SecondsToMax { get; set; }
    }
}
