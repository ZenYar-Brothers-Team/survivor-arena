using System;
using Game.Content;

namespace Game.Presentation
{
    /// <summary>One blob to place: a procedural silhouette (style and radius) or an authored library item.</summary>
    public sealed class FieldBlobEntryDefinition
    {
        public FieldBlobStyle Style { get; }
        public float Radius { get; }
        /// <summary>Library item id; null for a procedural silhouette.</summary>
        public string LibraryId { get; }

        public FieldBlobEntryDefinition(FieldBlobStyle style, float radius)
        {
            NumericValidation.ValidatePositive(radius, nameof(radius));
            if (style == FieldBlobStyle.Library) throw new ArgumentException("A library entry names its item.", nameof(style));
            Style = style;
            Radius = radius;
        }

        public FieldBlobEntryDefinition(string libraryId)
        {
            if (string.IsNullOrWhiteSpace(libraryId)) throw new ArgumentException("Library id is required.", nameof(libraryId));
            Style = FieldBlobStyle.Library;
            LibraryId = libraryId;
        }
    }
}
