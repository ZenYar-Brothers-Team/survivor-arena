using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Zones
{
    /// <summary>
    /// The clearances a zone center must satisfy, shared by the initial layout and by pulsing zones that reappear elsewhere:
    /// inside the arena margin, outside the start circle, apart from every other zone and clear of every obstacle outline
    /// (a portal also keeps room for its exit point and stays far from its pair).
    /// </summary>
    public sealed class ZonePlacementRules
    {
        // How far past the exit point the player's body may reach, so arriving never overlaps an obstacle.
        private const float ExitBodyAllowance = 1.5f;

        private readonly ZoneLayoutDefinition _layout;
        private readonly float _half;
        private readonly Vector2 _start;
        private readonly IReadOnlyList<IReadOnlyList<Vector2>> _obstacles;
        private readonly Vector2 _screenSize;

        /// <summary>Fraction of the screen size the active window extends beyond the player's screen on each side.</summary>
        public float ActiveScreenMargin => _layout.ActiveScreenMargin;
        public RandomZoneScheduleDefinition RandomSchedule => _layout.RandomSchedule;

        public ZonePlacementRules(ZoneLayoutDefinition layout, float sideLength, Vector2 start,
            IReadOnlyList<IReadOnlyList<Vector2>> obstacles, Vector2? screenSize = null)
        {
            _layout = layout ?? throw new ArgumentNullException(nameof(layout));
            _half = sideLength * .5f - layout.EdgeMargin;
            _start = start;
            _obstacles = obstacles ?? Array.Empty<IReadOnlyList<Vector2>>();
            if (layout.MaxPerScreen.HasValue)
            {
                if (!screenSize.HasValue) throw new ArgumentException("A screen-capped layout requires the current camera size.");
                NumericScreenSize(screenSize.Value);
                _screenSize = screenSize.Value + Vector2.one * (2f * layout.ScreenPadding);
            }
        }

        /// <summary>
        /// Draws up to PlacementAttempts random centers and returns the first that satisfies every clearance.
        /// <paramref name="others"/> are all other zones (their current centers count, even while invisible);
        /// <paramref name="pairFirst"/> is the first portal of the pair when placing the second one; <paramref name="within"/>,
        /// when given, confines the center to that world rectangle (the active window around the player).
        /// </summary>
        public bool TryPick(ZoneEffectDefinition effect, System.Random random, IEnumerable<ZonePlacement> others,
            ZonePlacement pairFirst, out Vector2 center, Rect? within = null, float? occurrenceRadius = null, bool allowStartOverlap = false)
        {
            var radius = occurrenceRadius ?? effect.Radius;
            var isPortal = effect.Kind == ZoneEffectKind.Portal;
            // FIELD-007: effects may cover props, while the physical altar foundation remains clear (DECISION-0146).
            var obstacleRadius = effect.IsAltar && _layout.AltarObstacleRadius.HasValue ? _layout.AltarObstacleRadius.Value : radius;
            var needed = obstacleRadius + _layout.ObstacleClearance + (isPortal ? effect.PortalExitDistance + ExitBodyAllowance : 0f);
            var loX = -_half + radius;
            var hiX = _half - radius;
            var loY = loX;
            var hiY = hiX;
            if (within.HasValue)
            {
                loX = Mathf.Max(loX, within.Value.xMin); hiX = Mathf.Min(hiX, within.Value.xMax);
                loY = Mathf.Max(loY, within.Value.yMin); hiY = Mathf.Min(hiY, within.Value.yMax);
            }
            var occupied = new List<ZonePlacement>(others);
            center = default;
            if (hiX < loX || hiY < loY) return false;
            for (var attempt = 0; attempt < _layout.PlacementAttempts; attempt++)
            {
                var candidate = new Vector2(Range(random, loX, hiX), Range(random, loY, hiY));
                if (!allowStartOverlap && (candidate - _start).magnitude < _layout.StartClearRadius + radius) continue;
                if (occupied.Exists(other => Vector2.Distance(other.Center, candidate) <
                                             other.Radius + radius + _layout.MinGap)) continue;
                if (pairFirst != null && Vector2.Distance(pairFirst.Center, candidate) < effect.PortalMinPairDistance) continue;
                var blocked = false;
                foreach (var outline in _obstacles)
                    if (ZoneGeometry.DiscClearance(outline, candidate, needed) < 0f) { blocked = true; break; }
                if (blocked) continue;
                if (!ScreenDensityFits(candidate, occupied)) continue;
                center = candidate;
                return true;
            }
            return false;
        }

        public static float Range(System.Random random, float min, float max) =>
            max <= min ? min : min + (float)random.NextDouble() * (max - min);

        private static void NumericScreenSize(Vector2 size)
        {
            Game.Content.NumericValidation.ValidatePositive(size.x, nameof(size.x));
            Game.Content.NumericValidation.ValidatePositive(size.y, nameof(size.y));
        }

        // Exact sliding-rectangle cap: any overfull rectangle can be shifted until its left/bottom
        // edges meet a point. Check all such edge pairs, including points across cell boundaries.
        private bool ScreenDensityFits(Vector2 candidate, List<ZonePlacement> occupied)
        {
            if (!_layout.MaxPerScreen.HasValue || occupied.Count < _layout.MaxPerScreen.Value) return true;
            var nearby = new List<Vector2> { candidate };
            foreach (var other in occupied)
                if (Mathf.Abs(other.Center.x - candidate.x) <= _screenSize.x &&
                    Mathf.Abs(other.Center.y - candidate.y) <= _screenSize.y) nearby.Add(other.Center);
            if (nearby.Count <= _layout.MaxPerScreen.Value) return true;
            foreach (var left in nearby)
            {
                if (candidate.x < left.x || candidate.x > left.x + _screenSize.x) continue;
                foreach (var bottom in nearby)
                {
                    if (candidate.y < bottom.y || candidate.y > bottom.y + _screenSize.y) continue;
                    var count = 0;
                    foreach (var point in nearby)
                        if (point.x >= left.x && point.x <= left.x + _screenSize.x &&
                            point.y >= bottom.y && point.y <= bottom.y + _screenSize.y &&
                            ++count > _layout.MaxPerScreen.Value) return false;
                }
            }
            return true;
        }
    }
}
