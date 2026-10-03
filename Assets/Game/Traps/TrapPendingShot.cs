namespace Game.Traps
{
    /// <summary>A shot of a started volley that has not left yet.</summary>
    public struct TrapPendingShot
    {
        public TrapShot Shot;
        public float HeadingDegrees;
        public float DelayRemaining;
    }
}
