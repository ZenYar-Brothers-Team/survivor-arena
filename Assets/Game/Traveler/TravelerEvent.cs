namespace Game.Traveler
{
    public readonly struct TravelerEvent
    {
        public TravelerSnapshot Traveler { get; }
        public string Outcome { get; }
        public TravelerEvent(TravelerSnapshot traveler, string outcome) { Traveler = traveler; Outcome = outcome; }
    }
}
