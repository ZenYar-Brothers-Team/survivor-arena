namespace Game.Pickup
{
    public readonly struct PickupSnapshot
    {
        public int Spawned { get; }
        public int Collected { get; }
        public int Expired { get; }
        public int Cancelled { get; }
        public int Rejected { get; }
        public int Active { get; }
        public string Feedback { get; }
        public PickupSnapshot(int spawned, int collected, int expired, int cancelled, int rejected, int active, string feedback)
        { Spawned = spawned; Collected = collected; Expired = expired; Cancelled = cancelled; Rejected = rejected; Active = active; Feedback = feedback; }
    }
}
