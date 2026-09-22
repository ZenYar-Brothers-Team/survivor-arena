using System;
using Game.Content;
using UnityEngine;

namespace Game.Presentation
{
    public sealed class PresentationFixtureDefinition
    {
        public ContentId Id { get; }
        public SpriteRole Role { get; }
        public Vector2 Position { get; }
        public Vector2 Size { get; }
        public Color Color { get; }
        public bool BodyMotion { get; }
        public PresentationFixtureDefinition(ContentId id, SpriteRole role, Vector2 position, Vector2 size,
            Color color, bool bodyMotion)
        {
            if (!id.IsValid || !id.ToString().StartsWith("FIXTURE-", StringComparison.Ordinal))
                throw new ArgumentException("Diagnostic fixtures require a FIXTURE id.");
            if (!Enum.IsDefined(typeof(SpriteRole), role) || role == SpriteRole.Unspecified)
                throw new ArgumentOutOfRangeException(nameof(role));
            NumericValidation.ValidateFinite(position.x, nameof(position));
            NumericValidation.ValidateFinite(position.y, nameof(position));
            NumericValidation.ValidatePositive(size.x, nameof(size));
            NumericValidation.ValidatePositive(size.y, nameof(size));
            for (var i = 0; i < 4; i++) NumericValidation.ValidateRange(color[i], 0, 1, nameof(color));
            Id = id; Role = role; Position = position; Size = size; Color = color; BodyMotion = bodyMotion;
        }
    }
}
