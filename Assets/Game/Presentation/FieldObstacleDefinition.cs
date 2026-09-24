using System;
using Game.Content;

namespace Game.Presentation
{
    /// <summary>One authored, axis-aligned field obstacle; the rectangle is its player-only collision box.</summary>
    public sealed class FieldObstacleDefinition
    {
        public string Id { get; }
        public FieldObstacleKind Kind { get; }
        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }

        public FieldObstacleDefinition(string id, FieldObstacleKind kind, float x, float y, float width, float height)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Obstacle id is required.", nameof(id));
            if (!Enum.IsDefined(typeof(FieldObstacleKind), kind)) throw new ArgumentOutOfRangeException(nameof(kind));
            NumericValidation.ValidateFinite(x, nameof(x));
            NumericValidation.ValidateFinite(y, nameof(y));
            NumericValidation.ValidatePositive(width, nameof(width));
            NumericValidation.ValidatePositive(height, nameof(height));
            Id = id; Kind = kind; X = x; Y = y; Width = width; Height = height;
        }
    }
}
