namespace Game.Enemy
{
    /// <summary>
    /// Life of one boss zone: Waiting (trail circles not yet placed) → Filling → Settling (impact flash and optional burning
    /// ground) → Done. Only <see cref="BossHazardField"/> changes the stage.
    /// </summary>
    internal enum BossZoneStage
    {
        Waiting,
        Filling,
        Settling,
        Done
    }
}
