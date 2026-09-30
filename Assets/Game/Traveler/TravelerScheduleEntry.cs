using Game.Content;
namespace Game.Traveler
{
    public readonly struct TravelerScheduleEntry
    {
        public ContentId Id { get; }
        public float Time { get; }
        public int Sequence { get; }
        public float Scale { get; }
        public float HealthScale { get; }
        public TravelerScheduleEntry(ContentId id, float time, int sequence, float scale)
            : this(id, time, sequence, scale, scale) { }
        public TravelerScheduleEntry(ContentId id, float time, int sequence, float scale, float healthScale)
        { Id = id; Time = time; Sequence = sequence; Scale = scale; HealthScale = healthScale; }
    }
}
