using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>One generated impassable silhouette; the polygon in world space is its player-only collision shape.</summary>
    public sealed class FieldObstacleShapeDefinition
    {
        public string Id { get; }
        public FieldBlobStyle Style { get; }
        public IReadOnlyList<Vector2> Points { get; }
        /// <summary>Authored sprite drawn at <see cref="Origin"/>; invalid for a procedural shape (painted from the points).</summary>
        public ContentId VisualId { get; }
        /// <summary>World position of the sprite pivot (the shape center for a procedural shape).</summary>
        public Vector2 Origin { get; }
        public Vector2 Center { get; }
        /// <summary>Distance from <see cref="Center"/> to the farthest point.</summary>
        public float BoundingRadius { get; }

        public FieldObstacleShapeDefinition(string id, FieldBlobStyle style, IEnumerable<Vector2> points,
            ContentId visualId = default, Vector2? origin = null)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Shape id is required.", nameof(id));
            var list = (points ?? throw new ArgumentNullException(nameof(points))).ToList();
            if (list.Count < 3) throw new ArgumentException("A shape needs at least three points.", nameof(points));
            foreach (var point in list)
            {
                NumericValidation.ValidateFinite(point.x, "point x");
                NumericValidation.ValidateFinite(point.y, "point y");
            }
            Id = id; Style = style; Points = list.AsReadOnly();
            var sum = Vector2.zero;
            foreach (var point in list) sum += point;
            Center = sum / list.Count;
            VisualId = visualId;
            Origin = origin ?? Center;
            BoundingRadius = list.Max(point => (point - Center).magnitude);
        }
    }
}
