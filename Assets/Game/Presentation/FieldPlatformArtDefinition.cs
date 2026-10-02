using System;
using Game.Content;
using Game.Presentation.Json;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>DECISION-0147: approved concept material regions, without changing the safe footprint or damage.</summary>
    public sealed class FieldPlatformArtDefinition
    {
        public ContentRef<SpriteDefinition> Visual { get; }
        public Rect GroundUvBounds { get; }
        public Rect SurfaceUvBounds { get; }
        public float GroundRepeat { get; }
        public float SurfaceRepeat { get; }
        public float ContourStep { get; }
        public float RimWidth { get; }
        public float InlayWidth { get; }
        public float FaceHeight { get; }
        public Color RimColor { get; }
        public Color InlayColor { get; }
        public Color FaceColor { get; }

        public FieldPlatformArtDefinition(FieldPlatformArtData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            Visual = new ContentRef<SpriteDefinition>(data.VisualId);
            if (!Visual.Id.IsValid) throw new ArgumentException("Platform art visualId is required.");
            GroundUvBounds = Bounds(data.GroundUvBounds);
            SurfaceUvBounds = Bounds(data.SurfaceUvBounds);
            NumericValidation.ValidatePositive(data.GroundRepeat, nameof(data.GroundRepeat));
            NumericValidation.ValidatePositive(data.SurfaceRepeat, nameof(data.SurfaceRepeat));
            NumericValidation.ValidateRange(data.ContourStep, .05f, .5f, nameof(data.ContourStep));
            NumericValidation.ValidatePositive(data.RimWidth, nameof(data.RimWidth));
            NumericValidation.ValidatePositive(data.InlayWidth, nameof(data.InlayWidth));
            NumericValidation.ValidatePositive(data.FaceHeight, nameof(data.FaceHeight));
            if (data.InlayWidth >= data.RimWidth) throw new ArgumentException("Inlay must fit inside the rim.");
            GroundRepeat = data.GroundRepeat; SurfaceRepeat = data.SurfaceRepeat;
            ContourStep = data.ContourStep; RimWidth = data.RimWidth;
            InlayWidth = data.InlayWidth; FaceHeight = data.FaceHeight;
            RimColor = ParseColor(data.RimColor); InlayColor = ParseColor(data.InlayColor);
            FaceColor = ParseColor(data.FaceColor);
        }

        private static Rect Bounds(float[] b)
        {
            if (b == null || b.Length != 4) throw new ArgumentException("Material bounds need four UV coordinates.");
            foreach (var v in b) NumericValidation.ValidateRange(v, 0f, 1f, "UV coordinate");
            if (b[0] >= b[2] || b[1] >= b[3]) throw new ArgumentException("Material UV bounds must have positive area.");
            return Rect.MinMaxRect(b[0], b[1], b[2], b[3]);
        }

        private static Color ParseColor(string value) => ColorUtility.TryParseHtmlString(value, out var color)
            ? color : throw new ArgumentException("Platform art colors must be HTML colors.");
    }
}
