namespace Game.ScreenEvents.Json
{
    /// <summary>
    /// One screen event (DECISION-0157). Sizes ending in <c>H</c> are fractions of the visible screen height at the moment the event
    /// starts, so the same content works at any resolution. Fields that only matter to some layouts are required per layout.
    /// </summary>
    public sealed class ScreenEventData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ScreenEventLayout? Layout { get; set; }
        /// <summary>Rare events respect the profile's minimum interval between rare events.</summary>
        public bool? Rare { get; set; }
        /// <summary>Damage of each hazard that touches the player, in percent of maximum health.</summary>
        public float? DamagePercent { get; set; }
        /// <summary>How hard the event is, 0 (breather) to 1 (the hardest); it is picked when the wave of intensity is near this value.</summary>
        public float? Intensity { get; set; }
        public float? TelegraphSeconds { get; set; }
        /// <summary>Burst layouts: how long the danger holds.</summary>
        public float? StrikeSeconds { get; set; }
        /// <summary>Strips, Circles, Corners: delay between consecutive hazards.</summary>
        public float? StaggerSeconds { get; set; }

        // Strips and Fan
        public ScreenStripMotion? Motion { get; set; }
        /// <summary>Strips: travel / long-side directions to pick from (degrees, 0 = right, 90 = up, 270 = down).</summary>
        public float[] DirectionsDegrees { get; set; }
        public int? CountMin { get; set; }
        public int? CountMax { get; set; }
        /// <summary>Strips, Cross, Fan: width of one strip.</summary>
        public float? WidthH { get; set; }
        /// <summary>Strips: smallest clear space between neighbouring strips.</summary>
        public float? MinGapH { get; set; }
        /// <summary>Strips: strips light up in order across the screen instead of in random order.</summary>
        public bool? Ordered { get; set; }
        /// <summary>Strips (sweep), Fan, Ring: speed in screen heights per second.</summary>
        public float? SpeedH { get; set; }
        /// <summary>Strips (sweep), Fan: length of the travelling bar.</summary>
        public float? BodyLengthH { get; set; }

        // Cross
        /// <summary>Cross: how far each strip may move from the screen centre, as a fraction of the screen extent.</summary>
        public float? CrossJitter { get; set; }

        // Circles and Judgment
        public float? RadiusMinH { get; set; }
        public float? RadiusMaxH { get; set; }
        public ScreenCirclePlacement? Placement { get; set; }
        public float? PlayerDistanceMinH { get; set; }
        public float? PlayerDistanceMaxH { get; set; }
        /// <summary>Circles: extra clear space between a circle's edge and the player at the start.</summary>
        public float? PlayerClearanceH { get; set; }
        /// <summary>Circles: smallest clear space between two circles.</summary>
        public float? GapH { get; set; }

        // Ring
        public float? StartRadiusH { get; set; }
        public float? ThicknessH { get; set; }
        public float? GapWidthH { get; set; }

        // HalfScreen
        /// <summary>HalfScreen: how far the dividing line may move from the centre, as a fraction of the screen extent.</summary>
        public float? SplitJitter { get; set; }

        // Corners
        public float? CornerWidthFraction { get; set; }
        public float? CornerHeightFraction { get; set; }

        // Fan
        public float[] AnglesDegrees { get; set; }

        // Judgment
        public float? SafeRadiusH { get; set; }
    }
}
