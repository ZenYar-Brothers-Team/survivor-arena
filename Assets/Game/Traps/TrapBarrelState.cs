namespace Game.Traps
{
    /// <summary>A barrel is intact, burning towards an explosion (explosive ones only) or already blown up.</summary>
    public enum TrapBarrelState
    {
        Intact,
        Fusing,
        Exploded
    }
}
