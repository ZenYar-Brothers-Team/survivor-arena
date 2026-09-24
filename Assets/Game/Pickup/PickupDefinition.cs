using System;
using Game.Content;
using Game.Presentation;
using UnityEngine;
namespace Game.Pickup
{
    /// <summary>Synthetic world pickup: healing in HP, contact radius in world units, optional lifetime in running seconds.</summary>
    public sealed class PickupDefinition : IContentDefinition, IReferencesContent
    {
        public ContentId Id { get; }
        public PickupRewardKind Kind { get; }
        public float Healing { get; }
        public float ContactRadius { get; }
        public float? LifetimeSeconds { get; }
        public string Marker { get; }
        public Color Color { get; }
        public float MarkerSize { get; }
        public ContentRef<SpriteDefinition> Visual { get; }
        public float VisualScale { get; }
        public PickupDefinition(ContentId id, PickupRewardKind kind, float healing, float contactRadius,
            float? lifetimeSeconds, string marker, Color color, float markerSize,
            ContentRef<SpriteDefinition> visual = default, float visualScale = 1f)
        {
            if (!id.IsValid || !Enum.IsDefined(typeof(PickupRewardKind), kind)) throw new ArgumentException("Pickup requires id and reward kind.");
            NumericValidation.ValidateNonNegative(healing, nameof(healing));
            if (kind == PickupRewardKind.Potion) NumericValidation.ValidatePositive(healing, nameof(healing));
            else if (healing != 0) throw new ArgumentException("Book cannot heal.");
            NumericValidation.ValidatePositive(contactRadius, nameof(contactRadius));
            if (lifetimeSeconds.HasValue) NumericValidation.ValidatePositive(lifetimeSeconds.Value, nameof(lifetimeSeconds));
            NumericValidation.ValidatePositive(markerSize, nameof(markerSize));
            NumericValidation.ValidatePositive(visualScale, nameof(visualScale));
            if (string.IsNullOrWhiteSpace(marker)) throw new ArgumentException("Fixture marker is required.");
            for (var i = 0; i < 4; i++) NumericValidation.ValidateRange(color[i], 0, 1, nameof(color));
            Id = id; Kind = kind; Healing = healing; ContactRadius = contactRadius; LifetimeSeconds = lifetimeSeconds;
            Marker = marker; Color = color; MarkerSize = markerSize;
            Visual = visual; VisualScale = visualScale;
        }

        public System.Collections.Generic.IEnumerable<ContentReference> GetReferencedContent()
        {
            if (Visual.Id.IsValid) yield return Visual.ToReference();
        }
    }
}
