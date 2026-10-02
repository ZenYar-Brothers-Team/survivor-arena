namespace Game.Zones
{
    /// <summary>
    /// One <see cref="ZoneTriggerKind"/> moment; <see cref="Polarity"/> is set for altars and null for other zones.
    /// <see cref="Position"/> is where it happened, or null when it concerns the player wherever they are.
    /// </summary>
    public readonly struct ZoneTrigger
    {
        public ZoneTriggerKind Kind { get; }
        public ZoneAltarPolarity? Polarity { get; }
        public UnityEngine.Vector2? Position { get; }

        public ZoneTrigger(ZoneTriggerKind kind, ZoneAltarPolarity? polarity, UnityEngine.Vector2? position)
        {
            Kind = kind;
            Polarity = polarity;
            Position = position;
        }
    }
}
