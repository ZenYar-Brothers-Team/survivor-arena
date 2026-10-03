namespace Game.Audio.Json
{
    /// <summary>One sound family for a screen event's strike starts; playback limits belong to the referenced cue.</summary>
    public sealed class AudioScreenEventCueData
    {
        public string EventId;
        public string Cue;
    }
}
