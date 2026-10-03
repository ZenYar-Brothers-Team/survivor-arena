namespace Game.ScreenEvents
{
    /// <summary>Area of a <see cref="ScreenHazardKind.Burst"/> hazard.</summary>
    public enum ScreenBurstShape
    {
        /// <summary>Rotated rectangle around the hazard origin.</summary>
        Rect,
        Circle,
        /// <summary>Everything outside a safe circle.</summary>
        OutsideCircle
    }
}
