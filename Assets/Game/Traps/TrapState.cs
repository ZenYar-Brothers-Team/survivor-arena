namespace Game.Traps
{
    /// <summary>States of a turret: waiting for its cooldown, winding up a volley, releasing delayed shots.</summary>
    public enum TrapState
    {
        Idle,
        Telegraph,
        Firing
    }
}
