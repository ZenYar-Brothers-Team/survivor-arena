using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Game.Content;
using Game.Presentation.Json;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Approved FIELD-010 art and visual tuning. Hazard geometry, damage and clocks belong to ScreenEvents.</summary>
    public sealed class ScreenEventPresentationProfile
    {
        public IReadOnlyDictionary<string, ScreenEventArtwork> Artwork { get; }
        public IReadOnlyDictionary<ContentId, string> SweepArtwork { get; }
        public float BorderWidth { get; }
        public float RepeatLength { get; }
        public float ExteriorRibbonWidth { get; }
        public float StrikeRevealSeconds { get; }
        public float CircleRotationDegreesPerSecond { get; }
        public float StripScrollSpeed { get; }
        public float RingScrollCyclesPerSecond { get; }
        public float ExteriorScrollSpeed { get; }
        public float StrikePulseHz { get; }
        public float StrikePulseDepth { get; }
        public float StrikeBoundaryAlpha { get; }
        public float RevealFeatherFraction { get; }
        public float WarningAlphaStart { get; }
        public float WarningAlphaEnd { get; }
        public float WarningFillStart { get; }
        public float WarningFillEnd { get; }
        public float StrikeAlpha { get; }
        public float StrikeFillAlpha { get; }
        public float SafeAlpha { get; }
        public float CorridorAlpha { get; }
        public Color WarningColor { get; }
        public Color StrikeColor { get; }
        public Color SafeColor { get; }
        public int SortingOrder { get; }

        public ScreenEventPresentationProfile(ScreenEventPresentationData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var artwork = new Dictionary<string, ScreenEventArtwork>(StringComparer.Ordinal);
            foreach (var key in new[] { "spear", "blade", "cloud", "warningStrip", "strikeStrip", "warningCircle", "strikeCircle", "safeRing" })
            {
                if (data.Artwork == null || !data.Artwork.TryGetValue(key, out var entry))
                    throw new ArgumentException("Screen-event artwork missing: " + key);
                var role = key == "spear" || key == "blade" ? SpriteRole.Weapon :
                    key == "cloud" || key.StartsWith("strike", StringComparison.Ordinal) ? SpriteRole.Impact : SpriteRole.Telegraph;
                artwork.Add(key, new ScreenEventArtwork(entry, role));
            }
            if (data.Artwork.Count != artwork.Count) throw new ArgumentException("Unknown screen-event artwork key.");
            Artwork = new ReadOnlyDictionary<string, ScreenEventArtwork>(artwork);
            var sweeps = new Dictionary<ContentId, string>();
            if (data.SweepArtwork == null || data.SweepArtwork.Count == 0) throw new ArgumentException("Screen-event sweepArtwork required.");
            foreach (var pair in data.SweepArtwork)
            {
                var id = new ContentId(pair.Key);
                if (!id.IsValid || (pair.Value != "spear" && pair.Value != "blade" && pair.Value != "cloud"))
                    throw new ArgumentException("Invalid screen-event sweep artwork.");
                sweeps.Add(id, pair.Value);
            }
            SweepArtwork = new ReadOnlyDictionary<ContentId, string>(sweeps);
            BorderWidth = Required(data.BorderWidth, "borderWidth");
            RepeatLength = Required(data.RepeatLength, "repeatLength");
            NumericValidation.ValidateRange(BorderWidth, .01f, .3f, nameof(BorderWidth));
            NumericValidation.ValidateRange(RepeatLength, .5f, 10f, nameof(RepeatLength));
            ExteriorRibbonWidth = Required(data.ExteriorRibbonWidth, "exteriorRibbonWidth");
            NumericValidation.ValidateRange(ExteriorRibbonWidth, .05f, RepeatLength * .5f, nameof(ExteriorRibbonWidth));
            StrikeRevealSeconds = Required(data.StrikeRevealSeconds, "strikeRevealSeconds");
            CircleRotationDegreesPerSecond = Required(data.CircleRotationDegreesPerSecond, "circleRotationDegreesPerSecond");
            StripScrollSpeed = Required(data.StripScrollSpeed, "stripScrollSpeed");
            RingScrollCyclesPerSecond = Required(data.RingScrollCyclesPerSecond, "ringScrollCyclesPerSecond");
            ExteriorScrollSpeed = Required(data.ExteriorScrollSpeed, "exteriorScrollSpeed");
            StrikePulseHz = Required(data.StrikePulseHz, "strikePulseHz");
            StrikePulseDepth = Fraction(data.StrikePulseDepth, "strikePulseDepth");
            StrikeBoundaryAlpha = Fraction(data.StrikeBoundaryAlpha, "strikeBoundaryAlpha");
            RevealFeatherFraction = Required(data.RevealFeatherFraction, "revealFeatherFraction");
            NumericValidation.ValidateRange(RevealFeatherFraction, .01f, .25f, nameof(RevealFeatherFraction));
            NumericValidation.ValidateRange(StrikeRevealSeconds, .02f, .5f, nameof(StrikeRevealSeconds));
            NumericValidation.ValidateRange(CircleRotationDegreesPerSecond, -360f, 360f, nameof(CircleRotationDegreesPerSecond));
            NumericValidation.ValidateRange(StripScrollSpeed, 0f, 20f, nameof(StripScrollSpeed));
            NumericValidation.ValidateRange(RingScrollCyclesPerSecond, 0f, 3f, nameof(RingScrollCyclesPerSecond));
            NumericValidation.ValidateRange(ExteriorScrollSpeed, 0f, 20f, nameof(ExteriorScrollSpeed));
            NumericValidation.ValidateRange(StrikePulseHz, 0f, 15f, nameof(StrikePulseHz));
            WarningAlphaStart = Fraction(data.WarningAlphaStart, "warningAlphaStart");
            WarningAlphaEnd = Fraction(data.WarningAlphaEnd, "warningAlphaEnd");
            WarningFillStart = Fraction(data.WarningFillStart, "warningFillStart");
            WarningFillEnd = Fraction(data.WarningFillEnd, "warningFillEnd");
            if (WarningAlphaEnd < WarningAlphaStart || WarningFillEnd < WarningFillStart)
                throw new ArgumentException("Screen-event warning must build up.");
            StrikeAlpha = Fraction(data.StrikeAlpha, "strikeAlpha");
            StrikeFillAlpha = Fraction(data.StrikeFillAlpha, "strikeFillAlpha");
            SafeAlpha = Fraction(data.SafeAlpha, "safeAlpha");
            CorridorAlpha = Fraction(data.CorridorAlpha, "corridorAlpha");
            WarningColor = ColorValue(data.WarningColor, "warningColor");
            StrikeColor = ColorValue(data.StrikeColor, "strikeColor");
            SafeColor = ColorValue(data.SafeColor, "safeColor");
            SortingOrder = data.SortingOrder ?? throw new ArgumentException("Screen-event sortingOrder required.");
        }

        public IEnumerable<ContentReference> GetReferencedContent()
        {
            foreach (var art in Artwork.Values) yield return art.Visual.ToReference();
        }

        private static float Required(float? value, string name) => value ?? throw new ArgumentException("Screen-event " + name + " required.");
        private static float Fraction(float? value, string name)
        {
            var result = Required(value, name);
            NumericValidation.ValidateRange(result, 0f, 1f, name);
            return result;
        }
        private static Color ColorValue(string value, string name)
        {
            if (!ColorUtility.TryParseHtmlString(value, out var color)) throw new ArgumentException("Screen-event " + name + " required as HTML color.");
            return color;
        }
    }
}
