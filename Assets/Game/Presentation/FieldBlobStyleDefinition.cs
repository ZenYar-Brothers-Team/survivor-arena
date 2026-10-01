using System;
using Game.Content;
using Game.Presentation.Json;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Validated generation and paint parameters of one blob style (units: world units and ratios of the blob radius).</summary>
    public sealed class FieldBlobStyleDefinition
    {
        public FieldBlobStyle Style { get; }
        public Color Fill { get; }
        public Color Outline { get; }
        public float StretchMin { get; }
        public float StretchMax { get; }
        public int OutlineVertexCount { get; }
        public float[] HarmonicAmplitudes { get; }
        public float HarmonicScaleMin { get; }
        public float HarmonicScaleMax { get; }
        public int MinVertices { get; }
        public int MaxVertices { get; }
        public float RadiusJitterMin { get; }
        public float RadiusJitterMax { get; }
        public float MinAspect { get; }
        public float LengthMin { get; }
        public float LengthMax { get; }
        public float WidthMin { get; }
        public float WidthMax { get; }
        public float BendMax { get; }
        public float Taper { get; }
        public int SpineSamples { get; }

        public FieldBlobStyleDefinition(FieldBlobStyle style, FieldBlobStyleData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            Style = style;
            Fill = ParseColor(data.FillColor, "fillColor");
            Outline = ParseColor(data.OutlineColor, "outlineColor");
            if (style != FieldBlobStyle.Linear)
            {
                StretchMin = Required(data.StretchMin, "stretchMin");
                StretchMax = Required(data.StretchMax, "stretchMax");
                NumericValidation.ValidatePositive(StretchMin, nameof(StretchMin));
                if (StretchMax < StretchMin) throw new ArgumentException("Blob stretch range is reversed.");
            }
            switch (style)
            {
                case FieldBlobStyle.Round:
                    OutlineVertexCount = data.OutlineVertexCount ?? throw new ArgumentException("outlineVertexCount is required.");
                    if (OutlineVertexCount < 12) throw new ArgumentException("A round blob needs at least 12 outline vertices.");
                    HarmonicAmplitudes = data.HarmonicAmplitudes ?? throw new ArgumentException("harmonicAmplitudes is required.");
                    if (HarmonicAmplitudes.Length != 3) throw new ArgumentException("harmonicAmplitudes needs exactly three values.");
                    var sum = 0f;
                    foreach (var amplitude in HarmonicAmplitudes)
                    {
                        NumericValidation.ValidateNonNegative(amplitude, "harmonic amplitude");
                        sum += amplitude;
                    }
                    HarmonicScaleMin = Required(data.HarmonicScaleMin, "harmonicScaleMin");
                    HarmonicScaleMax = Required(data.HarmonicScaleMax, "harmonicScaleMax");
                    NumericValidation.ValidatePositive(HarmonicScaleMin, nameof(HarmonicScaleMin));
                    if (HarmonicScaleMax < HarmonicScaleMin) throw new ArgumentException("Harmonic scale range is reversed.");
                    if (sum * HarmonicScaleMax >= 1f) throw new ArgumentException("Harmonics must keep the outline radius positive.");
                    break;
                case FieldBlobStyle.Angular:
                    MinVertices = data.MinVertices ?? throw new ArgumentException("minVertices is required.");
                    MaxVertices = data.MaxVertices ?? throw new ArgumentException("maxVertices is required.");
                    if (MinVertices < 3 || MaxVertices < MinVertices) throw new ArgumentException("Angular vertex range is invalid.");
                    RadiusJitterMin = Required(data.RadiusJitterMin, "radiusJitterMin");
                    RadiusJitterMax = Required(data.RadiusJitterMax, "radiusJitterMax");
                    NumericValidation.ValidatePositive(RadiusJitterMin, nameof(RadiusJitterMin));
                    if (RadiusJitterMax < RadiusJitterMin) throw new ArgumentException("Radius jitter range is reversed.");
                    MinAspect = Required(data.MinAspect, "minAspect");
                    NumericValidation.ValidateRange(MinAspect, 0f, 1f, nameof(MinAspect));
                    break;
                default:
                    LengthMin = Required(data.LengthMin, "lengthMin");
                    LengthMax = Required(data.LengthMax, "lengthMax");
                    WidthMin = Required(data.WidthMin, "widthMin");
                    WidthMax = Required(data.WidthMax, "widthMax");
                    BendMax = Required(data.BendMax, "bendMax");
                    Taper = Required(data.Taper, "taper");
                    SpineSamples = data.SpineSamples ?? throw new ArgumentException("spineSamples is required.");
                    NumericValidation.ValidatePositive(LengthMin, nameof(LengthMin));
                    NumericValidation.ValidatePositive(WidthMin, nameof(WidthMin));
                    if (LengthMax < LengthMin || WidthMax < WidthMin) throw new ArgumentException("Linear size range is reversed.");
                    NumericValidation.ValidateNonNegative(BendMax, nameof(BendMax));
                    NumericValidation.ValidateRange(Taper, 0f, 0.9f, nameof(Taper));
                    if (SpineSamples < 4) throw new ArgumentException("A linear blob needs at least 4 spine samples.");
                    break;
            }
        }

        private static float Required(float? value, string name) => value ?? throw new ArgumentException(name + " is required.");

        private static Color ParseColor(string html, string name)
        {
            if (string.IsNullOrWhiteSpace(html) || !ColorUtility.TryParseHtmlString(html, out var color))
                throw new ArgumentException(name + " must be an HTML color.");
            return color;
        }
    }
}
