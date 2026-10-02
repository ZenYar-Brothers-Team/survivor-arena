using System;
using Game.Content;
using Game.Presentation.Json;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Approved FIELD-003 road art; cannot change walking geometry.</summary>
    public sealed class FieldRoadArtDefinition
    {
        public ContentRef<SpriteDefinition> Main { get; }
        public ContentRef<SpriteDefinition> Branch { get; }
        public ContentRef<SpriteDefinition> Curb { get; }
        public float SurfaceRepeat { get; }
        public float CurbWidth { get; }
        public float CurbRepeat { get; }
        public Rect CurbUvBounds { get; }
        /// <summary>Projected height, in world units, toward the bottom of the fixed 3/4 camera view.</summary>
        public float CurbFaceHeight { get; }
        public Color CurbFaceTint { get; }
        /// <summary>Exterior dirt feather width in world units; does not change the walking boundary.</summary>
        public float EdgeWidth { get; }
        public float EdgeOpacity { get; }
        public float EdgeNoiseScale { get; }
        public float EdgeNoiseStrength { get; }
        public Color EdgeSoilColor { get; }

        public FieldRoadArtDefinition(FieldRoadArtData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            Main = new ContentRef<SpriteDefinition>(data.MainVisualId);
            Branch = new ContentRef<SpriteDefinition>(data.BranchVisualId);
            Curb = new ContentRef<SpriteDefinition>(data.CurbVisualId);
            if (!Main.Id.IsValid || !Branch.Id.IsValid || !Curb.Id.IsValid)
                throw new ArgumentException("Road art requires all three visual references.");
            SurfaceRepeat = data.SurfaceRepeat ?? throw new ArgumentException("surfaceRepeat is required.");
            CurbWidth = data.CurbWidth ?? throw new ArgumentException("curbWidth is required.");
            CurbRepeat = data.CurbRepeat ?? throw new ArgumentException("curbRepeat is required.");
            NumericValidation.ValidatePositive(SurfaceRepeat, nameof(SurfaceRepeat));
            NumericValidation.ValidatePositive(CurbWidth, nameof(CurbWidth));
            NumericValidation.ValidatePositive(CurbRepeat, nameof(CurbRepeat));
            var bounds = data.CurbUvBounds;
            if (bounds == null || bounds.Length != 4) throw new ArgumentException("curbUvBounds requires four coordinates.");
            foreach (var value in bounds) NumericValidation.ValidateNonNegative(value, "curbUvBounds");
            if (bounds[2] > 1 || bounds[3] > 1 || bounds[2] <= bounds[0] || bounds[3] <= bounds[1])
                throw new ArgumentException("curbUvBounds must be an ordered rectangle within [0,1].");
            CurbUvBounds = Rect.MinMaxRect(bounds[0], bounds[1], bounds[2], bounds[3]);
            CurbFaceHeight = data.CurbFaceHeight ?? throw new ArgumentException("curbFaceHeight is required.");
            EdgeWidth = data.EdgeWidth ?? throw new ArgumentException("edgeWidth is required.");
            EdgeOpacity = data.EdgeOpacity ?? throw new ArgumentException("edgeOpacity is required.");
            EdgeNoiseScale = data.EdgeNoiseScale ?? throw new ArgumentException("edgeNoiseScale is required.");
            EdgeNoiseStrength = data.EdgeNoiseStrength ?? throw new ArgumentException("edgeNoiseStrength is required.");
            NumericValidation.ValidatePositive(CurbFaceHeight, nameof(CurbFaceHeight));
            NumericValidation.ValidatePositive(EdgeWidth, nameof(EdgeWidth));
            NumericValidation.ValidatePositive(EdgeNoiseScale, nameof(EdgeNoiseScale));
            NumericValidation.ValidateRange(EdgeOpacity, 0f, 1f, nameof(EdgeOpacity));
            NumericValidation.ValidateRange(EdgeNoiseStrength, 0f, 1f, nameof(EdgeNoiseStrength));
            if (string.IsNullOrWhiteSpace(data.CurbFaceTint) || !ColorUtility.TryParseHtmlString(data.CurbFaceTint,out var face))
                throw new ArgumentException("curbFaceTint must be an HTML color.");
            if (string.IsNullOrWhiteSpace(data.EdgeSoilColor) || !ColorUtility.TryParseHtmlString(data.EdgeSoilColor,out var soil))
                throw new ArgumentException("edgeSoilColor must be an HTML color.");
            CurbFaceTint = face; EdgeSoilColor = soil;
        }
    }
}
