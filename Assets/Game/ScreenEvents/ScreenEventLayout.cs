namespace Game.ScreenEvents
{
    /// <summary>How an event places its hazards on the screen when it starts (DECISION-0157).</summary>
    public enum ScreenEventLayout
    {
        /// <summary>Parallel strips across the screen: sweeps (spear, blade, wave, rain) or bursts (a row of pillars).</summary>
        Strips,
        /// <summary>One vertical and one horizontal burst strip; the four quadrants stay safe.</summary>
        Cross,
        /// <summary>Burst circles (circles of judgment, falling feathers).</summary>
        Circles,
        /// <summary>An expanding ring from the screen centre with a crossable gap.</summary>
        Ring,
        /// <summary>One half of the screen strikes.</summary>
        HalfScreen,
        /// <summary>The four screen corners strike in turn.</summary>
        Corners,
        /// <summary>Several sweeps fan out from one point on a screen edge.</summary>
        Fan,
        /// <summary>The whole screen strikes except one safe circle.</summary>
        Judgment
    }
}
