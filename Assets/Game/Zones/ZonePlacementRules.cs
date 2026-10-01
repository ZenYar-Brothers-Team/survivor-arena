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

        public ZonePlacementRules(ZoneLayoutDefinition layout, float sideLength, Vector2 start,
            IReadOnlyList<IReadOnlyList<Vector2>> obstacles)
        {
            _layout = layout ?? throw new ArgumentNullException(nameof(layout));
            _half = sideLength * .5f - layout.EdgeMargin;
            _start = start;
            _obstacles = obstacles ?? Array.Empty<IReadOnlyList<Vector2>>();
        }

        /// <summary>
        /// Draws up to PlacementAttempts random centers and returns the first that satisfies every clearance.
        /// <paramref name="others"/> are all other zones (their current centers count, even while invisible);
        /// <paramref name="pairFirst"/> is the first portal of the pair when placing the second one.
        /// </summary>
        public bool TryPick(ZoneEffectDefinition effect, System.Random random, IEnumerable<ZonePlacement> others,
            ZonePlacement pairFirst, out Vector2 center)
        {
            var isPortal = effect.Kind == ZoneEffectKind.Portal;
            var needed = effect.Radius + _layout.ObstacleClearance + (isPortal ? effect.PortalExitDistance + ExitBodyAllowance : 0f);
            var lo = -_half + effect.Radius;
            var hi = _half - effect.Radius;
            var occupied = new List<ZonePlacement>(others);
            center = default;
            if (hi < lo) return false;
            for (var attempt = 0; attempt < _layout.PlacementAttempts; attempt++)
            {
                var candidate = new Vector2(Range(random, lo, hi), Range(random, lo, hi));
                if ((candidate - _start).magnitude < _layout.StartClearRadius + effect.Radius) continue;
                if (occupied.Exists(other => Vector2.Distance(other.Center, candidate) <
                                             other.Effect.Radius + effect.Radius + _layout.MinGap)) continue;
                if (pairFirst != null && Vector2.Distance(pairFirst.Center, candidate) < effect.PortalMinPairDistance) continue;
                var blocked = false;
                foreach (var outline in _obstacles)
                    if (ZoneGeometry.DiscClearance(outline, candidate, needed) < 0f) { blocked = true; break; }
                if (blocked) continue;
                center = candidate;
                return true;
            }
            return false;
        }

        public static float Range(System.Random random, float min, float max) =>
            max <= min ? min : min + (float)random.NextDouble() * (max - min);
    }
}
