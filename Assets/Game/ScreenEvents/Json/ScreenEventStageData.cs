namespace Game.ScreenEvents.Json
{
    public sealed class ScreenEventStageData
    {
        /// <summary>Run time (seconds of Running) from which this stage applies.</summary>
        public float? FromSeconds { get; set; }
        /// <summary>Intensity at the bottom of the wave at the start of this stage (the next stage is blended in).</summary>
        public float? ValleyIntensity { get; set; }
        /// <summary>Intensity at the top of the wave at the start of this stage.</summary>
        public float? PeakIntensity { get; set; }
        public float? PauseMinSeconds { get; set; }
        public float? PauseMaxSeconds { get; set; }
        public ScreenEventPoolEntryData[] Pool { get; set; }
    }
}
