namespace Game.Presentation
{
    /// <summary>
    /// Candidate looks for a slowed enemy (DECISION-0108 preview, GI-09). Selected only from the development
    /// panel until the user picks one; <see cref="Off"/> is the current production look.
    /// </summary>
    public enum SlowStatusStyle
    {
        Off,
        Bar,
        Tint,
        Ice,
        Outline,
        All
    }
}
