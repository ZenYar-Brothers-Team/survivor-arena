using System;
using Game.Content;
using Game.Presentation.Json;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Validated generation and surface profile of the circular-platform field study (FIELD-009): round platforms
    /// joined by straight bridges over a damaging void. World units, centered arena.
    /// </summary>
    public sealed class FieldPlatformLayoutDefinition
    {
        public float ArenaSideLength { get; }
        public float MinimumRadius { get; }
        public float MaximumRadius { get; }
        /// <summary>Radii from here up to <see cref="MaximumRadius"/> are rare "giant" platforms.</summary>
        public float GiantRadiusMin { get; }
        public float GiantChance { get; }
        /// <summary>Ordinary radii are minimum + (giant threshold - minimum) * u^skew: above 1 favors small platforms.</summary>
        public float RadiusSkew { get; }
        /// <summary>Only a platform this large (or larger) may have a single bridge.</summary>
        public float DeadEndMinimumRadius { get; }
        /// <summary>Platforms below this radius are "too small": two of them are never joined by a bridge.</summary>
        public float SmallRadiusMax { get; }
        /// <summary>Chance that a platform (other than the start one) holds a Book at its center.</summary>
        public float BookChance { get; }
        public float EdgeMargin { get; }
        public float MinimumPlatformGap { get; }
        public float MaximumBridgeLength { get; }
        public float BridgeWidth { get; }
        public float BridgeClearance { get; }
        public float TreeMinimumAngleDegrees { get; }
        public float ExtraMinimumAngleDegrees { get; }
        public float EdgeWeightJitterMin { get; }
        public float EdgeWeightJitterMax { get; }
        public float VoidDamagePerSecond { get; }
        /// <summary>A layout is accepted when at least this fraction of the placed platforms stays connected.</summary>
        public float KeptPlatformFraction { get; }
        public int PlatformCount { get; }
        public int ExtraBridgeCount { get; }
        public int PlacementAttempts { get; }
        public int LayoutAttempts { get; }
        public int CircleSegments { get; }
        public int ReferenceSeed { get; }
        public int BookUpgradeCount { get; }
        public Color VoidColor { get; }
        public Color PlatformColor { get; }
        public Color BridgeColor { get; }
        public Color StartPlatformColor { get; }
        public FieldPlatformArtDefinition Art { get; }

        public FieldPlatformLayoutDefinition(FieldPlatformLayoutData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            ArenaSideLength = Positive(data.ArenaSideLength, "arenaSideLength");
            MinimumRadius = Positive(data.MinimumRadius, "minimumRadius");
            MaximumRadius = Positive(data.MaximumRadius, "maximumRadius");
            GiantRadiusMin = Positive(data.GiantRadiusMin, "giantRadiusMin");
            GiantChance = data.GiantChance ?? throw new ArgumentException("giantChance is required.");
            NumericValidation.ValidateRange(GiantChance, 0f, 1f, nameof(GiantChance));
            RadiusSkew = Positive(data.RadiusSkew, "radiusSkew");
            DeadEndMinimumRadius = Positive(data.DeadEndMinimumRadius, "deadEndMinimumRadius");
            SmallRadiusMax = Positive(data.SmallRadiusMax, "smallRadiusMax");
            BookChance = data.BookChance ?? throw new ArgumentException("bookChance is required.");
            NumericValidation.ValidateRange(BookChance, 0f, 1f, nameof(BookChance));
            EdgeMargin = Positive(data.EdgeMargin, "edgeMargin");
            MinimumPlatformGap = Positive(data.MinimumPlatformGap, "minimumPlatformGap");
            MaximumBridgeLength = Positive(data.MaximumBridgeLength, "maximumBridgeLength");
            BridgeWidth = Positive(data.BridgeWidth, "bridgeWidth");
            BridgeClearance = Positive(data.BridgeClearance, "bridgeClearance");
            TreeMinimumAngleDegrees = Positive(data.TreeMinimumAngleDegrees, "treeMinimumAngleDegrees");
            ExtraMinimumAngleDegrees = Positive(data.ExtraMinimumAngleDegrees, "extraMinimumAngleDegrees");
            EdgeWeightJitterMin = Positive(data.EdgeWeightJitterMin, "edgeWeightJitterMin");
            EdgeWeightJitterMax = Positive(data.EdgeWeightJitterMax, "edgeWeightJitterMax");
            VoidDamagePerSecond = Positive(data.VoidDamagePerSecond, "voidDamagePerSecond");
            KeptPlatformFraction = data.KeptPlatformFraction ?? throw new ArgumentException("keptPlatformFraction is required.");
            NumericValidation.ValidateRange(KeptPlatformFraction, .01f, 1f, nameof(KeptPlatformFraction));
            PlatformCount = Count(data.PlatformCount, "platformCount");
            ExtraBridgeCount = data.ExtraBridgeCount ?? throw new ArgumentException("extraBridgeCount is required.");
            NumericValidation.ValidateNonNegative(ExtraBridgeCount, nameof(ExtraBridgeCount));
            PlacementAttempts = Count(data.PlacementAttempts, "placementAttempts");
            LayoutAttempts = Count(data.LayoutAttempts, "layoutAttempts");
            CircleSegments = Count(data.CircleSegments, "circleSegments");
            ReferenceSeed = data.ReferenceSeed ?? throw new ArgumentException("referenceSeed is required.");
            BookUpgradeCount = Count(data.BookUpgradeCount, "bookUpgradeCount");
            if (MinimumRadius >= MaximumRadius || GiantRadiusMin <= MinimumRadius || GiantRadiusMin > MaximumRadius ||
                DeadEndMinimumRadius < MinimumRadius || SmallRadiusMax <= MinimumRadius || SmallRadiusMax >= GiantRadiusMin || EdgeWeightJitterMin > EdgeWeightJitterMax || CircleSegments < 8 ||
                MaximumRadius + EdgeMargin >= ArenaSideLength * .5f)
                throw new ArgumentException("Platform radii, jitter or circle segments are inconsistent.");
            VoidColor = Color(data.VoidColor, "voidColor");
            PlatformColor = Color(data.PlatformColor, "platformColor");
            BridgeColor = Color(data.BridgeColor, "bridgeColor");
            StartPlatformColor = Color(data.StartPlatformColor, "startPlatformColor");
            Art = data.Art == null ? null : new FieldPlatformArtDefinition(data.Art);
        }

        private static float Positive(float? value, string name)
        {
            var result = value ?? throw new ArgumentException($"{name} is required.");
            NumericValidation.ValidatePositive(result, name);
            return result;
        }

        private static int Count(int? value, string name)
        {
            var result = value ?? throw new ArgumentException($"{name} is required.");
            NumericValidation.ValidateCount(result, name);
            return result;
        }

        private static Color Color(string value, string name) =>
            ColorUtility.TryParseHtmlString(value, out var color) ? color : throw new ArgumentException($"{name} is not a color.");
    }
}
