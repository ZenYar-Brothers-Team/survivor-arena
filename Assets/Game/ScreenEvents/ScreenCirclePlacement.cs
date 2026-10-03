namespace Game.ScreenEvents
{
    /// <summary>Where a <see cref="ScreenEventLayout.Circles"/> event puts its circles.</summary>
    public enum ScreenCirclePlacement
    {
        /// <summary>Anywhere on the visible screen.</summary>
        Random,
        /// <summary>Around the player at a given distance, never right under them.</summary>
        AroundPlayer
    }
}
