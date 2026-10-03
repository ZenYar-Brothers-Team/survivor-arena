using System;
using System.Collections.Generic;
using Game.Diagnostics;
using Game.Zones;
using UnityEngine;

namespace Game.Traps
{
    /// <summary>
    /// Scatters a field's turrets and barrels for one run (DECISION-0156). A body keeps clear of the start circle, the arena
    /// edge, every obstacle outline and every other trap body; a fixed-heading turret also needs open space in front of it.
    /// The result depends only on the layout, the arena, the start, the obstacles and the seed.
    /// </summary>
    public static class TrapPlacementGenerator
    {
        private const float ForwardProbeStep = 0.5f;
        private const float ForwardProbeRadius = 0.3f;
        private const float InitialCooldownMinFraction = 0.3f;

        private struct Body
        {
            public Vector2 Center;
            public float Radius;
        }

        public static TrapPlacementSet Generate(TrapLayoutDefinition layout, float arenaSideLength, Vector2 start,
            IReadOnlyList<IReadOnlyList<Vector2>> outlines, int seed)
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));
            Game.Content.NumericValidation.ValidatePositive(arenaSideLength, nameof(arenaSideLength));
            using var guard = PerfGuard.Measure("Traps.Placement", 50f);
            outlines ??= Array.Empty<IReadOnlyList<Vector2>>();
            var random = new System.Random(unchecked(seed * 7919 + 4001));
            var half = arenaSideLength * .5f - layout.EdgeMargin;
            var bodies = new List<Body>();

            // Largest bodies first pack better; the stable order keeps the result reproducible.
            var order = new List<TrapTypeDefinition>(layout.Types);
            var indexed = new List<(TrapTypeDefinition type, int index)>();
            for (var i = 0; i < order.Count; i++) indexed.Add((order[i], i));
            indexed.Sort((a, b) =>
            {
                var byRadius = b.type.BodyRadius.CompareTo(a.type.BodyRadius);
                return byRadius != 0 ? byRadius : a.index.CompareTo(b.index);
            });

            var traps = new List<TrapPlacement>();
            var skippedTraps = 0;
            var remaining = new Dictionary<TrapTypeDefinition, int>();
            foreach (var type in layout.Types) remaining[type] = type.Count;

            // Start-screen traps are placed first, near their authored offset, and count towards their type's total.
            foreach (var startTrap in layout.StartTraps)
            {
                var type = startTrap.Type;
                remaining[type]--;
                var rotation = type.Heading == TrapHeadingMode.Fixed ? type.RotationsDegrees[0] : 0f;
                var forward = type.Heading == TrapHeadingMode.Fixed && type.SpinDegreesPerSecond == 0f
                    ? (float?)(rotation + type.Shots[0].AngleDegrees) : null;
                if (!TryPickNear(layout, type.BodyRadius, half, start, startTrap.Offset, outlines, bodies, forward, out var center))
                {
                    skippedTraps++;
                    continue;
                }
                bodies.Add(new Body { Center = center, Radius = type.BodyRadius });
                traps.Add(new TrapPlacement(type, center, rotation, InitialCooldown(type, random), startTrap.ModelKey ?? type.ModelKey));
            }

            if (layout.Density != null)
                skippedTraps += ScatterByScreen(layout, half, start, outlines, bodies, random, traps);
            else
            foreach (var (type, _) in indexed)
            {
                for (var n = 0; n < remaining[type]; n++)
                {
                    var rotation = type.Heading == TrapHeadingMode.Fixed
                        ? type.RotationsDegrees[random.Next(type.RotationsDegrees.Count)] : 0f;
                    var forward = type.Heading == TrapHeadingMode.Fixed && type.SpinDegreesPerSecond == 0f
                        ? (float?)(rotation + type.Shots[0].AngleDegrees) : null;
                    if (!TryPick(layout, type.BodyRadius, half, start, outlines, bodies, random, forward, out var center))
                    {
                        skippedTraps++;
                        continue;
                    }
                    bodies.Add(new Body { Center = center, Radius = type.BodyRadius });
                    traps.Add(new TrapPlacement(type, center, rotation, InitialCooldown(type, random), type.ModelKey));
                }
            }

            var barrels = new List<TrapBarrelPlacement>();
            var skippedBarrels = 0;
            var spec = layout.Barrels;
            if (layout.Density != null && spec.PerScreen > 0)
            {
                // One guaranteed barrel per screen cell (after every trap); each explodes with the authored chance.
                skippedBarrels += ScatterBarrelsByScreen(layout, half, start, outlines, bodies, random, barrels);
                return new TrapPlacementSet(traps, barrels, skippedTraps, skippedBarrels);
            }
            var authoredExplosive = 0;
            foreach (var startBarrel in layout.StartBarrels)
            {
                if (!TryPickNear(layout, spec.BodyRadius, half, start, startBarrel.Offset, outlines, bodies, null, out var center))
                {
                    skippedBarrels++;
                    continue;
                }
                bodies.Add(new Body { Center = center, Radius = spec.BodyRadius });
                barrels.Add(new TrapBarrelPlacement(center, startBarrel.Explosive));
                if (startBarrel.Explosive) authoredExplosive++;
            }
            // The rest scatter at random; the explosive total still follows the share, minus the authored explosive ones.
            var randomCount = spec.Count - layout.StartBarrels.Count;
            var explosive = new bool[randomCount];
            var indices = new int[randomCount];
            for (var i = 0; i < indices.Length; i++) indices[i] = i;
            for (var i = indices.Length - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (indices[i], indices[j]) = (indices[j], indices[i]);
            }
            for (var i = 0; i < Math.Min(Math.Max(0, spec.ExplosiveCount - authoredExplosive), indices.Length); i++) explosive[indices[i]] = true;
            for (var n = 0; n < randomCount; n++)
            {
                if (!TryPick(layout, spec.BodyRadius, half, start, outlines, bodies, random, null, out var center))
                {
                    skippedBarrels++;
                    continue;
                }
                bodies.Add(new Body { Center = center, Radius = spec.BodyRadius });
                barrels.Add(new TrapBarrelPlacement(center, explosive[n]));
            }
            return new TrapPlacementSet(traps, barrels, skippedTraps, skippedBarrels);
        }

        /// <summary>
        /// Screen-based scatter: the arena is cut into cells of one screen; each cell gets a random count from the density weights
        /// and that many traps of weighted random type are placed inside it (types are weighted by their count). Returns how many
        /// traps found no spot (the cells around the start never get any).
        /// </summary>
        private static int ScatterByScreen(TrapLayoutDefinition layout, float half, Vector2 start,
            IReadOnlyList<IReadOnlyList<Vector2>> outlines, List<Body> bodies, System.Random random, List<TrapPlacement> traps)
        {
            var density = layout.Density;
            var skipped = 0;
            // The cell count is rounded and the cells stretched to fill the arena evenly, so no sliver is left at the edge.
            var columns = Mathf.Max(1, Mathf.RoundToInt(2f * half / density.CellWidth));
            var rows = Mathf.Max(1, Mathf.RoundToInt(2f * half / density.CellHeight));
            var cellWidth = 2f * half / columns;
            var cellHeight = 2f * half / rows;
            var totalWeight = 0;
            foreach (var type in layout.Types) totalWeight += type.Count;
            for (var row = 0; row < rows; row++)
            for (var column = 0; column < columns; column++)
            {
                var cell = Rect.MinMaxRect(-half + column * cellWidth, -half + row * cellHeight,
                    -half + (column + 1) * cellWidth, -half + (row + 1) * cellHeight);
                var count = density.PickCount(random.NextDouble());
                for (var n = 0; n < count; n++)
                {
                    var type = PickType(layout, totalWeight, random);
                    var rotation = type.Heading == TrapHeadingMode.Fixed
                        ? type.RotationsDegrees[random.Next(type.RotationsDegrees.Count)] : 0f;
                    var forward = type.Heading == TrapHeadingMode.Fixed && type.SpinDegreesPerSecond == 0f
                        ? (float?)(rotation + type.Shots[0].AngleDegrees) : null;
                    if (!TryPick(layout, type.BodyRadius, half, start, outlines, bodies, random, forward, out var center, cell))
                    {
                        skipped++;
                        continue;
                    }
                    bodies.Add(new Body { Center = center, Radius = type.BodyRadius });
                    traps.Add(new TrapPlacement(type, center, rotation, InitialCooldown(type, random), type.ModelKey));
                }
            }
            return skipped;
        }

        private static int ScatterBarrelsByScreen(TrapLayoutDefinition layout, float half, Vector2 start,
            IReadOnlyList<IReadOnlyList<Vector2>> outlines, List<Body> bodies, System.Random random, List<TrapBarrelPlacement> barrels)
        {
            var density = layout.Density;
            var spec = layout.Barrels;
            var skipped = 0;
            var columns = Mathf.Max(1, Mathf.RoundToInt(2f * half / density.CellWidth));
            var rows = Mathf.Max(1, Mathf.RoundToInt(2f * half / density.CellHeight));
            var cellWidth = 2f * half / columns;
            var cellHeight = 2f * half / rows;
            for (var row = 0; row < rows; row++)
            for (var column = 0; column < columns; column++)
            {
                var cell = Rect.MinMaxRect(-half + column * cellWidth, -half + row * cellHeight,
                    -half + (column + 1) * cellWidth, -half + (row + 1) * cellHeight);
                for (var n = 0; n < spec.PerScreen; n++)
                {
                    if (!TryPick(layout, spec.BodyRadius, half, start, outlines, bodies, random, null, out var center, cell))
                    {
                        skipped++;
                        continue;
                    }
                    bodies.Add(new Body { Center = center, Radius = spec.BodyRadius });
                    barrels.Add(new TrapBarrelPlacement(center, random.NextDouble() < spec.ExplosiveShare));
                }
            }
            return skipped;
        }

        private static TrapTypeDefinition PickType(TrapLayoutDefinition layout, int totalWeight, System.Random random)
        {
            var roll = random.Next(totalWeight);
            foreach (var type in layout.Types)
            {
                if (roll < type.Count) return type;
                roll -= type.Count;
            }
            return layout.Types[layout.Types.Count - 1];
        }

        private static float InitialCooldown(TrapTypeDefinition type, System.Random random) =>
            type.CooldownSeconds * (InitialCooldownMinFraction + (float)random.NextDouble() * (1f - InitialCooldownMinFraction));

        /// <summary>
        /// Looks for a free spot at the authored offset from the start. When it is taken the spot is nudged first a little sideways
        /// and inwards (so a trap meant for the first screen stays on it), then the same distance is swept around the start in
        /// 30-degree steps, alternating sides.
        /// </summary>
        private static bool TryPickNear(TrapLayoutDefinition layout, float radius, float half, Vector2 start, Vector2 offset,
            IReadOnlyList<IReadOnlyList<Vector2>> outlines, List<Body> bodies, float? forwardDegrees, out Vector2 center)
        {
            center = default;
            var limit = half - radius;
            bool Fits(Vector2 candidate)
            {
                if (Mathf.Abs(candidate.x) > limit || Mathf.Abs(candidate.y) > limit) return false;
                if (Overlaps(candidate, radius, layout.MinGap, bodies)) return false;
                if (TouchesObstacle(candidate, radius + layout.ObstacleClearance, outlines)) return false;
                return !forwardDegrees.HasValue || ForwardIsOpen(candidate, radius, forwardDegrees.Value, layout.ForwardClearance, outlines);
            }
            foreach (var scale in new[] { 1f, .85f, .7f })
                foreach (var angle in new[] { 0f, 8f, -8f, 16f, -16f, 24f, -24f })
                {
                    var rotated = Quaternion.Euler(0f, 0f, angle) * (offset * scale);
                    var candidate = start + new Vector2(rotated.x, rotated.y);
                    if (!Fits(candidate)) continue;
                    center = candidate;
                    return true;
                }
            for (var step = 0; step < 12; step++)
            {
                var angle = ((step + 1) / 2) * 30f * (step % 2 == 0 ? 1f : -1f);
                var rotated = Quaternion.Euler(0f, 0f, angle) * offset;
                var candidate = start + new Vector2(rotated.x, rotated.y);
                if (!Fits(candidate)) continue;
                center = candidate;
                return true;
            }
            return false;
        }

        private static bool TryPick(TrapLayoutDefinition layout, float radius, float half, Vector2 start,
            IReadOnlyList<IReadOnlyList<Vector2>> outlines, List<Body> bodies, System.Random random, float? forwardDegrees,
            out Vector2 center, Rect? within = null)
        {
            center = default;
            var limit = half - radius;
            if (limit <= 0f) return false;
            var minX = -limit;
            var maxX = limit;
            var minY = -limit;
            var maxY = limit;
            if (within.HasValue)
            {
                minX = Mathf.Max(minX, within.Value.xMin); maxX = Mathf.Min(maxX, within.Value.xMax);
                minY = Mathf.Max(minY, within.Value.yMin); maxY = Mathf.Min(maxY, within.Value.yMax);
                if (maxX < minX || maxY < minY) return false;
            }
            for (var attempt = 0; attempt < layout.PlacementAttempts; attempt++)
            {
                var candidate = new Vector2(minX + (float)random.NextDouble() * (maxX - minX),
                    minY + (float)random.NextDouble() * (maxY - minY));
                if ((candidate - start).magnitude < layout.StartClearRadius + radius) continue;
                if (Overlaps(candidate, radius, layout.MinGap, bodies)) continue;
                if (TouchesObstacle(candidate, radius + layout.ObstacleClearance, outlines)) continue;
                if (forwardDegrees.HasValue && !ForwardIsOpen(candidate, radius, forwardDegrees.Value, layout.ForwardClearance, outlines))
                    continue;
                center = candidate;
                return true;
            }
            return false;
        }

        private static bool Overlaps(Vector2 candidate, float radius, float gap, List<Body> bodies)
        {
            foreach (var body in bodies)
                if (Vector2.Distance(body.Center, candidate) < body.Radius + radius + gap) return true;
            return false;
        }

        private static bool TouchesObstacle(Vector2 candidate, float needed, IReadOnlyList<IReadOnlyList<Vector2>> outlines)
        {
            foreach (var outline in outlines)
                if (ZoneGeometry.DiscClearance(outline, candidate, needed) < 0f) return true;
            return false;
        }

        private static bool ForwardIsOpen(Vector2 origin, float bodyRadius, float degrees, float clearance,
            IReadOnlyList<IReadOnlyList<Vector2>> outlines)
        {
            var radians = degrees * Mathf.Deg2Rad;
            var direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            for (var distance = bodyRadius + ForwardProbeStep; distance <= clearance + bodyRadius; distance += ForwardProbeStep)
                if (TouchesObstacle(origin + direction * distance, ForwardProbeRadius, outlines)) return false;
            return true;
        }
    }
}
