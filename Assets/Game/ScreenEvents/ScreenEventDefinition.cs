using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
using Game.ScreenEvents.Json;

namespace Game.ScreenEvents
{
    /// <summary>
    /// A validated screen event (DECISION-0157): a layout that decides how the hazards are placed on the visible screen when the
    /// event starts, plus the numbers of that layout. Sizes ending in H are fractions of the screen height; fields a layout does not
    /// use stay at zero. Only ever damages the player.
    /// </summary>
    public sealed class ScreenEventDefinition
    {
        private const float MaxDamagePercent = 100f;
        private const int MaxCount = 24;

        public ContentId Id { get; }
        public string Name { get; }
        public ScreenEventLayout Layout { get; }
        public bool Rare { get; }
        public float DamagePercent { get; }
        /// <summary>0 (breather) to 1 (hardest); the schedule favours events near the current intensity.</summary>
        public float Intensity { get; }
        public float DamageFraction => DamagePercent / 100f;
        public float TelegraphSeconds { get; }
        public float StrikeSeconds { get; }
        public float StaggerSeconds { get; }
        public ScreenStripMotion Motion { get; }
        public IReadOnlyList<float> DirectionsDegrees { get; }
        public int CountMin { get; }
        public int CountMax { get; }
        public float WidthH { get; }
        public float MinGapH { get; }
        public bool Ordered { get; }
        public float SpeedH { get; }
        public float BodyLengthH { get; }
        public float CrossJitter { get; }
        public float RadiusMinH { get; }
        public float RadiusMaxH { get; }
        public ScreenCirclePlacement Placement { get; }
        public float PlayerDistanceMinH { get; }
        public float PlayerDistanceMaxH { get; }
        public float PlayerClearanceH { get; }
        public float GapH { get; }
        public float StartRadiusH { get; }
        public float ThicknessH { get; }
        public float GapWidthH { get; }
        public float SplitJitter { get; }
        public float CornerWidthFraction { get; }
        public float CornerHeightFraction { get; }
        public IReadOnlyList<float> AnglesDegrees { get; }
        public float SafeRadiusH { get; }

        public ScreenEventDefinition(ScreenEventData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrWhiteSpace(data.Id)) throw new ArgumentException("Screen event id is required.");
            var id = data.Id;
            Id = id;
            Name = string.IsNullOrWhiteSpace(data.Name) ? throw new ArgumentException($"{id}: name is required.") : data.Name;
            Layout = data.Layout ?? throw new ArgumentException($"{id}: layout is required.");
            Rare = data.Rare ?? throw new ArgumentException($"{id}: rare is required.");
            DamagePercent = data.DamagePercent ?? throw new ArgumentException($"{id}: damagePercent is required.");
            TelegraphSeconds = data.TelegraphSeconds ?? throw new ArgumentException($"{id}: telegraphSeconds is required.");
            Intensity = data.Intensity ?? throw new ArgumentException($"{id}: intensity is required.");
            NumericValidation.ValidateRange(DamagePercent, 0f, MaxDamagePercent, nameof(DamagePercent));
            NumericValidation.ValidateRange(Intensity, 0f, 1f, nameof(Intensity));
            NumericValidation.ValidatePositive(TelegraphSeconds, nameof(TelegraphSeconds));

            // Which numbers a layout needs. Anything required but missing throws by name; anything unused stays zero.
            switch (Layout)
            {
                case ScreenEventLayout.Strips:
                    Motion = data.Motion ?? throw Missing(id, "motion");
                    DirectionsDegrees = Directions(id, data.DirectionsDegrees);
                    CountMin = data.CountMin ?? throw Missing(id, "countMin");
                    CountMax = data.CountMax ?? throw Missing(id, "countMax");
                    WidthH = Positive(id, data.WidthH, "widthH");
                    MinGapH = NonNegative(id, data.MinGapH, "minGapH");
                    Ordered = data.Ordered ?? throw Missing(id, "ordered");
                    StaggerSeconds = NonNegative(id, data.StaggerSeconds, "staggerSeconds");
                    if (Motion == ScreenStripMotion.Sweep)
                    {
                        SpeedH = Positive(id, data.SpeedH, "speedH");
                        BodyLengthH = Positive(id, data.BodyLengthH, "bodyLengthH");
                    }
                    else StrikeSeconds = Positive(id, data.StrikeSeconds, "strikeSeconds");
                    Counts(id);
                    break;
                case ScreenEventLayout.Cross:
                    WidthH = Positive(id, data.WidthH, "widthH");
                    CrossJitter = Fraction(id, data.CrossJitter, "crossJitter", .45f);
                    StrikeSeconds = Positive(id, data.StrikeSeconds, "strikeSeconds");
                    break;
                case ScreenEventLayout.Circles:
                    CountMin = data.CountMin ?? throw Missing(id, "countMin");
                    CountMax = data.CountMax ?? throw Missing(id, "countMax");
                    RadiusMinH = Positive(id, data.RadiusMinH, "radiusMinH");
                    RadiusMaxH = Positive(id, data.RadiusMaxH, "radiusMaxH");
                    Placement = data.Placement ?? throw Missing(id, "placement");
                    PlayerClearanceH = NonNegative(id, data.PlayerClearanceH, "playerClearanceH");
                    GapH = NonNegative(id, data.GapH, "gapH");
                    StaggerSeconds = NonNegative(id, data.StaggerSeconds, "staggerSeconds");
                    StrikeSeconds = Positive(id, data.StrikeSeconds, "strikeSeconds");
                    if (Placement == ScreenCirclePlacement.AroundPlayer)
                    {
                        PlayerDistanceMinH = NonNegative(id, data.PlayerDistanceMinH, "playerDistanceMinH");
                        PlayerDistanceMaxH = Positive(id, data.PlayerDistanceMaxH, "playerDistanceMaxH");
                        if (PlayerDistanceMaxH < PlayerDistanceMinH) throw new ArgumentException($"{id}: playerDistanceMaxH is below playerDistanceMinH.");
                    }
                    if (RadiusMaxH < RadiusMinH) throw new ArgumentException($"{id}: radiusMaxH is below radiusMinH.");
                    Counts(id);
                    break;
                case ScreenEventLayout.Ring:
                    StartRadiusH = Positive(id, data.StartRadiusH, "startRadiusH");
                    ThicknessH = Positive(id, data.ThicknessH, "thicknessH");
                    GapWidthH = Positive(id, data.GapWidthH, "gapWidthH");
                    SpeedH = Positive(id, data.SpeedH, "speedH");
                    break;
                case ScreenEventLayout.HalfScreen:
                    SplitJitter = Fraction(id, data.SplitJitter, "splitJitter", .45f);
                    StrikeSeconds = Positive(id, data.StrikeSeconds, "strikeSeconds");
                    break;
                case ScreenEventLayout.Corners:
                    CornerWidthFraction = Fraction(id, data.CornerWidthFraction, "cornerWidthFraction", .5f);
                    CornerHeightFraction = Fraction(id, data.CornerHeightFraction, "cornerHeightFraction", .5f);
                    if (CornerWidthFraction <= 0f || CornerHeightFraction <= 0f) throw new ArgumentException($"{id}: corner fractions must be positive.");
                    StaggerSeconds = NonNegative(id, data.StaggerSeconds, "staggerSeconds");
                    StrikeSeconds = Positive(id, data.StrikeSeconds, "strikeSeconds");
                    break;
                case ScreenEventLayout.Fan:
                    AnglesDegrees = (data.AnglesDegrees ?? throw Missing(id, "anglesDegrees")).ToArray();
                    if (AnglesDegrees.Count == 0) throw new ArgumentException($"{id}: a fan needs at least one angle.");
                    WidthH = Positive(id, data.WidthH, "widthH");
                    SpeedH = Positive(id, data.SpeedH, "speedH");
                    BodyLengthH = Positive(id, data.BodyLengthH, "bodyLengthH");
                    break;
                case ScreenEventLayout.Judgment:
                    SafeRadiusH = Positive(id, data.SafeRadiusH, "safeRadiusH");
                    PlayerDistanceMinH = NonNegative(id, data.PlayerDistanceMinH, "playerDistanceMinH");
                    PlayerDistanceMaxH = Positive(id, data.PlayerDistanceMaxH, "playerDistanceMaxH");
                    if (PlayerDistanceMaxH < PlayerDistanceMinH) throw new ArgumentException($"{id}: playerDistanceMaxH is below playerDistanceMinH.");
                    StrikeSeconds = Positive(id, data.StrikeSeconds, "strikeSeconds");
                    break;
                default:
                    throw new ArgumentException($"{id}: unknown layout '{Layout}'.");
            }
        }

        private void Counts(string id)
        {
            NumericValidation.ValidateCount(CountMin, nameof(CountMin));
            NumericValidation.ValidateCount(CountMax, nameof(CountMax));
            if (CountMax < CountMin) throw new ArgumentException($"{id}: countMax is below countMin.");
            if (CountMax > MaxCount) throw new ArgumentException($"{id}: countMax exceeds {MaxCount}.");
        }

        private static IReadOnlyList<float> Directions(string id, float[] values)
        {
            if (values == null || values.Length == 0) throw Missing(id, "directionsDegrees");
            foreach (var value in values) NumericValidation.ValidateFinite(value, "directionsDegrees");
            return values.ToArray();
        }

        private static ArgumentException Missing(string id, string field) => new ArgumentException($"{id}: {field} is required.");

        private static float Positive(string id, float? value, string field)
        {
            var result = value ?? throw Missing(id, field);
            NumericValidation.ValidatePositive(result, field);
            return result;
        }

        private static float NonNegative(string id, float? value, string field)
        {
            var result = value ?? throw Missing(id, field);
            NumericValidation.ValidateNonNegative(result, field);
            return result;
        }

        private static float Fraction(string id, float? value, string field, float maximum)
        {
            var result = value ?? throw Missing(id, field);
            NumericValidation.ValidateRange(result, 0f, maximum, field);
            return result;
        }
    }
}
