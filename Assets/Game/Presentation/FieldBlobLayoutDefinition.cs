using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
using Game.Presentation.Json;

namespace Game.Presentation
{
    /// <summary>
    /// Per-run blob layout: a fixed number of large impassable silhouettes scattered over open ground (field geometry
    /// study, docs/implementation/proposals/2026-10-01-field-geometry-variety.md). Distances are world units.
    /// </summary>
    public sealed class FieldBlobLayoutDefinition
    {
        public float EdgeMargin { get; }
        public float StartClearRadius { get; }
        /// <summary>Free radius around the player spawn that even the start-screen blob never enters.</summary>
        public float PlayerClearRadius { get; }
        public float MinGap { get; }
        public int PlacementAttempts { get; }
        public int MaxRestarts { get; }
        public int ReferenceSeed { get; }
        public float PixelsPerUnit { get; }
        public int OutlinePixels { get; }
        public FieldBlobStartDefinition StartScreen { get; }
        public IReadOnlyDictionary<FieldBlobStyle, FieldBlobStyleDefinition> Styles { get; }
        public IReadOnlyList<FieldBlobEntryDefinition> Blobs { get; }

        public FieldBlobLayoutDefinition(FieldBlobLayoutData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            EdgeMargin = Required(data.EdgeMargin, "blobLayout.edgeMargin");
            StartClearRadius = Required(data.StartClearRadius, "blobLayout.startClearRadius");
            PlayerClearRadius = Required(data.PlayerClearRadius, "blobLayout.playerClearRadius");
            MinGap = Required(data.MinGap, "blobLayout.minGap");
            PlacementAttempts = data.PlacementAttempts ?? throw new ArgumentException("blobLayout.placementAttempts is required.");
            MaxRestarts = data.MaxRestarts ?? throw new ArgumentException("blobLayout.maxRestarts is required.");
            ReferenceSeed = data.ReferenceSeed ?? throw new ArgumentException("blobLayout.referenceSeed is required.");
            PixelsPerUnit = Required(data.PixelsPerUnit, "blobLayout.pixelsPerUnit");
            OutlinePixels = data.OutlinePixels ?? throw new ArgumentException("blobLayout.outlinePixels is required.");
            NumericValidation.ValidateNonNegative(EdgeMargin, nameof(EdgeMargin));
            NumericValidation.ValidateNonNegative(StartClearRadius, nameof(StartClearRadius));
            NumericValidation.ValidateNonNegative(PlayerClearRadius, nameof(PlayerClearRadius));
            NumericValidation.ValidateNonNegative(MinGap, nameof(MinGap));
            NumericValidation.ValidateCount(PlacementAttempts, nameof(PlacementAttempts));
            NumericValidation.ValidateNonNegative(MaxRestarts, nameof(MaxRestarts));
            NumericValidation.ValidatePositive(PixelsPerUnit, nameof(PixelsPerUnit));
            NumericValidation.ValidateNonNegative(OutlinePixels, nameof(OutlinePixels));
            if (data.StartScreen != null)
                StartScreen = new FieldBlobStartDefinition(Required(data.StartScreen.HalfWidth, "blobLayout.startScreen.halfWidth"),
                    Required(data.StartScreen.HalfHeight, "blobLayout.startScreen.halfHeight"),
                    Required(data.StartScreen.Reach, "blobLayout.startScreen.reach"),
                    Required(data.StartScreen.Radius, "blobLayout.startScreen.radius"),
                    data.StartScreen.Styles ?? throw new ArgumentException("blobLayout.startScreen.styles is required."));
            var styles = new Dictionary<FieldBlobStyle, FieldBlobStyleDefinition>();
            foreach (var pair in data.Styles ?? throw new ArgumentException("blobLayout.styles is required."))
            {
                if (!Enum.TryParse<FieldBlobStyle>(pair.Key, true, out var style))
                    throw new ArgumentException("Unknown blob style '" + pair.Key + "'.");
                styles.Add(style, new FieldBlobStyleDefinition(style, pair.Value));
            }
            Styles = styles;
            var blobs = new List<FieldBlobEntryDefinition>();
            foreach (var entry in data.Blobs ?? throw new ArgumentException("blobLayout.blobs is required."))
                blobs.Add(new FieldBlobEntryDefinition(entry?.Style ?? throw new ArgumentException("Blob style is required."),
                    Required(entry.Radius, "blob radius")));
            if (blobs.Count == 0) throw new ArgumentException("A blob layout needs at least one blob.");
            Blobs = blobs.AsReadOnly();
            var used = Blobs.Select(blob => blob.Style).Concat(StartScreen == null ? Enumerable.Empty<FieldBlobStyle>() : StartScreen.Styles).Distinct();
            foreach (var style in used)
                if (!Styles.ContainsKey(style)) throw new ArgumentException("Blob style '" + style + "' has no style definition.");
        }

        /// <summary>Number of obstacles one run places (the start-screen blob counts as one when configured).</summary>
        public int TotalCount => Blobs.Count + (StartScreen == null ? 0 : 1);

        private static float Required(float? value, string name) =>
            value ?? throw new ArgumentException(name + " is required.");
    }
}
