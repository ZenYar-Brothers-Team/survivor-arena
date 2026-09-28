using System;
using System.Collections.Generic;
using Game.Content;
using UnityEngine;

namespace Game.Pickup
{
    /// <summary>
    /// Cheap axis-aligned arena query used by Traveler spawn and movement. Player-only obstacles reject spawn
    /// points but do not constrain movement; no path or connected-component search is performed.
    /// </summary>
    public sealed class BoxPickupPlacement
    {
        private readonly Rect _bounds;
        private readonly Rect[] _obstacles;

        public BoxPickupPlacement(Rect bounds, IEnumerable<Rect> obstacles, Vector2 actorHalfSize, Vector2 anchor, float skin)
        {
            if (obstacles == null) throw new ArgumentNullException(nameof(obstacles));
            NumericValidation.ValidatePositive(skin, nameof(skin));
            NumericValidation.ValidatePositive(bounds.width, nameof(bounds));
            NumericValidation.ValidatePositive(bounds.height, nameof(bounds));
            NumericValidation.ValidateNonNegative(actorHalfSize.x, nameof(actorHalfSize));
            NumericValidation.ValidateNonNegative(actorHalfSize.y, nameof(actorHalfSize));
            var margin = actorHalfSize + Vector2.one * skin;
            _bounds = Rect.MinMaxRect(bounds.xMin + margin.x, bounds.yMin + margin.y,
                bounds.xMax - margin.x, bounds.yMax - margin.y);
            if (_bounds.width <= 0 || _bounds.height <= 0)
                throw new ArgumentException("Arena has no usable space after actor clearance.");
            var inflated = new List<Rect>();
            foreach (var rect in obstacles)
                inflated.Add(Rect.MinMaxRect(rect.xMin - margin.x, rect.yMin - margin.y,
                    rect.xMax + margin.x, rect.yMax + margin.y));
            _obstacles = inflated.ToArray();
            if (!Contains(anchor)) throw new ArgumentException("Arena requires a free spawn anchor.");
        }

        /// <summary>True when a spawn point is inside the arena and outside every player-only obstacle.</summary>
        public bool Contains(Vector2 point)
        {
            NumericValidation.ValidateFinite(point.x, nameof(point));
            NumericValidation.ValidateFinite(point.y, nameof(point));
            if (point.x < _bounds.xMin || point.x > _bounds.xMax ||
                point.y < _bounds.yMin || point.y > _bounds.yMax) return false;
            foreach (var obstacle in _obstacles)
                if (point.x > obstacle.xMin && point.x < obstacle.xMax &&
                    point.y > obstacle.yMin && point.y < obstacle.yMax) return false;
            return true;
        }

        /// <summary>Constrains movement to the arena while deliberately ignoring player-only obstacles.</summary>
        public Vector2 ClampToBounds(Vector2 point)
        {
            NumericValidation.ValidateFinite(point.x, nameof(point));
            NumericValidation.ValidateFinite(point.y, nameof(point));
            return new Vector2(Mathf.Clamp(point.x, _bounds.xMin, _bounds.xMax),
                Mathf.Clamp(point.y, _bounds.yMin, _bounds.yMax));
        }
    }
}
