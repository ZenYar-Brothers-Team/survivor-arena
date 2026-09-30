namespace Game.Presentation
{
    /// <summary>
    /// Slow-status looks (DECISION-0108, GI-09). Ice includes the selected progress bar;
    /// the other values remain available in the development panel for comparison.
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
