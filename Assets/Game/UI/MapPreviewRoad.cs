using System;
using System.Collections.Generic;
using Game.Content;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// One walkable road piece for the development map: a world-space centerline stroked at <see cref="Width"/> with round
    /// ends and joins. A single point is a filled disc whose diameter is <see cref="Width"/> (a dead end's round end).
    /// </summary>
    public sealed class MapPreviewRoad
    {
        public IReadOnlyList<Vector2> Points { get; }
        public float Width { get; }
        public Color Color { get; }

        public MapPreviewRoad(IReadOnlyList<Vector2> points, float width, Color color)
        {
            if (points == null || points.Count == 0) throw new ArgumentException("A map road needs at least one point.", nameof(points));
            NumericValidation.ValidatePositive(width, nameof(width));
            Points = points; Width = width; Color = color;
        }
    }
}
