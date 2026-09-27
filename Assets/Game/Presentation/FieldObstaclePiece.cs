using System;
using Game.Content;

namespace Game.Presentation
{
    /// <summary>One obstacle of a layout pattern: kind and rectangle relative to the pattern center (DECISION-0068).</summary>
    public sealed class FieldObstaclePiece
    {
        public FieldObstacleKind Kind { get; }
        /// <summary>Optional per-piece sprite override carried into the generated obstacle.</summary>
        public ContentId VisualId { get; }
        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }

        public FieldObstaclePiece(FieldObstacleKind kind, float x, float y, float width, float height,
            string visualId = null)
        {
            if (!Enum.IsDefined(typeof(FieldObstacleKind), kind)) throw new ArgumentOutOfRangeException(nameof(kind));
            NumericValidation.ValidateFinite(x, nameof(x));
            NumericValidation.ValidateFinite(y, nameof(y));
            NumericValidation.ValidatePositive(width, nameof(width));
            NumericValidation.ValidatePositive(height, nameof(height));
            Kind = kind;
            VisualId = string.IsNullOrWhiteSpace(visualId) ? default(ContentId) : new ContentId(visualId);
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
            return new FieldObstaclePiece(Kind, x, y, w, h, VisualId.ToString());
        }
    }
}
