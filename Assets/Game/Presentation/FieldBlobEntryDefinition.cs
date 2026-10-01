using Game.Content;

namespace Game.Presentation
{
    /// <summary>One blob to place: its silhouette family and base radius in world units.</summary>
    public sealed class FieldBlobEntryDefinition
    {
        public FieldBlobStyle Style { get; }
        public float Radius { get; }

        public FieldBlobEntryDefinition(FieldBlobStyle style, float radius)
        {
            NumericValidation.ValidatePositive(radius, nameof(radius));
            Style = style;
            Radius = radius;
        }
    }
}
