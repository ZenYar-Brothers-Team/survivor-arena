namespace Game.Traps.Json
{
    public sealed class TrapTypeData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int? Tier { get; set; }
        public int? Count { get; set; }
        /// <summary>Optional key of the 3D model every placement of this type is drawn with; absent = placeholder shape.</summary>
        public string Model { get; set; }
        public float? BodyRadius { get; set; }
        /// <summary>"fixed" (the placement's rotation) or "aim" (towards the player when the telegraph starts).</summary>
        public string Heading { get; set; }
        /// <summary>Fixed heading only: the rotations a placement may take.</summary>
        public float[] RotationsDegrees { get; set; }
        /// <summary>Optional heading change per volley; absent = none.</summary>
        public float? HeadingStepDegrees { get; set; }
        /// <summary>Optional: the step restarts every N volleys (0 or absent = it keeps accumulating).</summary>
        public int? HeadingCycleVolleys { get; set; }
        /// <summary>Optional fixed-heading only: the head turns this fast while the trap rests; it stops for a volley and fires along its pose.</summary>
        public float? SpinDegreesPerSecond { get; set; }
        /// <summary>Optional aimed traps: seconds after a volley the head keeps its pose before it starts tracking the player again.</summary>
        public float? AimResumeDelaySeconds { get; set; }
        public float? TelegraphSeconds { get; set; }
        /// <summary>Optional: length of the aim line drawn along the heading during the telegraph; absent = no line.</summary>
        public float? TelegraphLineLength { get; set; }
        public float? CooldownSeconds { get; set; }
        public TrapShotGroupData[] Shots { get; set; }
    }
}
