using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
using Game.Presentation.Json;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// An illustrated obstacle with fixed size and orientation. <see cref="Points"/> outline its player-only collider around the
    /// sprite pivot, so the sprite sits at the placement origin and the collider is the same polygon moved there.
    /// </summary>
    public sealed class FieldBlobLibraryItemDefinition
    {
        public string Id { get; }
        public ContentRef<SpriteDefinition> Visual { get; }
        public IReadOnlyList<Vector2> Points { get; }

        public FieldBlobLibraryItemDefinition(FieldBlobLibraryItemData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrWhiteSpace(data.Id)) throw new ArgumentException("Library item id is required.");
            if (string.IsNullOrWhiteSpace(data.VisualId)) throw new ArgumentException($"Library item '{data.Id}' requires a visual id.");
            if (data.Points == null || data.Points.Length < 3) throw new ArgumentException($"Library item '{data.Id}' needs at least three outline points.");
            Id = data.Id;
            Visual = new ContentRef<SpriteDefinition>(data.VisualId);
            var points = new List<Vector2>(data.Points.Length);
            foreach (var point in data.Points)
            {
                if (point == null || point.Length != 2) throw new ArgumentException($"Library item '{data.Id}' outline points need x and y.");
                NumericValidation.ValidateFinite(point[0], "outline x");
                NumericValidation.ValidateFinite(point[1], "outline y");
                points.Add(new Vector2(point[0], point[1]));
            }
            Points = points.AsReadOnly();
        }

        /// <summary>Distance from the sprite pivot to the farthest outline point.</summary>
        public float BoundingRadius => Points.Max(point => point.magnitude);
    }
}
