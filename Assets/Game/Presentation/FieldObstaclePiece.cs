using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Presentation
{
    /// <summary>One obstacle of a layout pattern: kind and rectangle relative to the pattern center (DECISION-0068).</summary>
    public sealed class FieldObstaclePiece
    {
        public FieldObstacleKind Kind { get; }
        /// <summary>Optional per-piece sprite override carried into the generated obstacle.</summary>
        public ContentId VisualId { get; }
        /// <summary>
        /// Optional sprite alternatives (DECISION-0073): the generator picks one uniformly for every placed copy, so all
        /// approved props of a field keep appearing. Mutually exclusive with <see cref="VisualId"/>.
        /// </summary>
        public IReadOnlyList<ContentId> VisualVariants { get; }
        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }

        public FieldObstaclePiece(FieldObstacleKind kind, float x, float y, float width, float height,
            string visualId = null, IEnumerable<string> visualVariants = null)
        {
            if (!Enum.IsDefined(typeof(FieldObstacleKind), kind)) throw new ArgumentOutOfRangeException(nameof(kind));
            NumericValidation.ValidateFinite(x, nameof(x));
            NumericValidation.ValidateFinite(y, nameof(y));
            NumericValidation.ValidatePositive(width, nameof(width));
            NumericValidation.ValidatePositive(height, nameof(height));
            Kind = kind;
            VisualId = string.IsNullOrWhiteSpace(visualId) ? default(ContentId) : new ContentId(visualId);
            var variants = new List<ContentId>();
            foreach (var variant in visualVariants ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(variant)) throw new ArgumentException("Visual variants cannot be empty.", nameof(visualVariants));
                variants.Add(new ContentId(variant));
            }
            if (variants.Count > 0 && VisualId.IsValid)
                throw new ArgumentException("A piece uses either visualId or visualIds, not both.", nameof(visualVariants));
            VisualVariants = variants.AsReadOnly();
            X = x; Y = y; Width = width; Height = height;
        }

        /// <summary>This piece turned counter-clockwise by a multiple of 90° around the pattern center.</summary>
        public FieldObstaclePiece Rotated(int degrees)
        {
            if (degrees % 90 != 0) throw new ArgumentOutOfRangeException(nameof(degrees), "Patterns turn by multiples of 90°.");
            float x = X, y = Y, w = Width, h = Height;
            var turns = ((degrees / 90) % 4 + 4) % 4;
            for (var turn = 0; turn < turns; turn++)
            {
                var previousX = x;
                x = -y;
                y = previousX;
                (w, h) = (h, w);
            }
            var variants = new List<string>(VisualVariants.Count);
            foreach (var variant in VisualVariants) variants.Add(variant.ToString());
            return new FieldObstaclePiece(Kind, x, y, w, h, VisualId.IsValid ? VisualId.ToString() : null, variants);
        }
    }
}
