using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.ScreenEvents
{
    /// <summary>
    /// Turns a <see cref="ScreenEventDefinition"/> into concrete hazards for the screen the player sees right now (DECISION-0157).
    /// Everything is placed in world space and stays there while the event runs. The result depends only on the definition, the visible
    /// rectangle, the player position and the random source. Layout rules keep a way through: strips keep a minimum clear gap, quadrants
    /// of a cross, the safe circle of a judgment and the gap of a ring are never covered, and hazards never start under the player.
    /// </summary>
    public static class ScreenEventSpawner
    {
        private const int PlacementAttempts = 40;
        // How far a hazard that must reach the screen edge overshoots it, in screen heights.
        private const float EdgeOvershootH = 1f;
        private const float StripOvershootH = .1f;

        public static ScreenEventInstance Create(ScreenEventDefinition definition, Rect view, Vector2 player, float playerRadius,
            System.Random random)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (random == null) throw new ArgumentNullException(nameof(random));
            Game.Content.NumericValidation.ValidatePositive(view.width, nameof(view));
            Game.Content.NumericValidation.ValidatePositive(view.height, nameof(view));
            var hazards = new List<ScreenHazard>();
            switch (definition.Layout)
            {
                case ScreenEventLayout.Strips: Strips(definition, view, random, hazards); break;
                case ScreenEventLayout.Cross: Cross(definition, view, random, hazards); break;
                case ScreenEventLayout.Circles: Circles(definition, view, player, playerRadius, random, hazards); break;
                case ScreenEventLayout.Ring: Ring(definition, view, random, hazards); break;
                case ScreenEventLayout.HalfScreen: HalfScreen(definition, view, random, hazards); break;
                case ScreenEventLayout.Corners: Corners(definition, view, random, hazards); break;
                case ScreenEventLayout.Fan: Fan(definition, view, random, hazards); break;
                case ScreenEventLayout.Judgment: Judgment(definition, view, player, random, hazards); break;
                default: throw new ArgumentException($"Unknown layout '{definition.Layout}'.");
            }
            return new ScreenEventInstance(definition, hazards);
        }

        private static void Strips(ScreenEventDefinition def, Rect view, System.Random random, List<ScreenHazard> hazards)
        {
            var u = view.height;
            var angle = def.DirectionsDegrees[random.Next(def.DirectionsDegrees.Count)] * Mathf.Deg2Rad;
            var along = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            var across = new Vector2(-along.y, along.x);
            var extentAlong = Mathf.Abs(view.width * along.x) + Mathf.Abs(view.height * along.y);
            var extentAcross = Mathf.Abs(view.width * across.x) + Mathf.Abs(view.height * across.y);
            var length = extentAlong + 2f * StripOvershootH * u;
            var width = def.WidthH * u;
            var count = random.Next(def.CountMin, def.CountMax + 1);
            // The screen is cut into one equal cell per strip and each strip is placed at random inside its cell, kept clear of the
            // cell walls by half the minimum gap: strips are spread across the screen and never closer than the clear gap. A count the
            // screen cannot hold with that gap is reduced.
            var gap = def.MinGapH * u;
            count = Mathf.Max(1, Mathf.Min(count, Mathf.FloorToInt(extentAcross / (width + gap))));
            var cell = extentAcross / count;
            var offsets = new List<float>();
            for (var i = 0; i < count; i++)
            {
                var low = -extentAcross * .5f + i * cell + width * .5f + gap * .5f;
                var high = -extentAcross * .5f + (i + 1) * cell - width * .5f - gap * .5f;
                offsets.Add(high > low ? Mathf.Lerp(low, high, (float)random.NextDouble()) : (low + high) * .5f);
            }
            if (def.Ordered) offsets.Sort(); else Shuffle(offsets, random);
            var center = view.center;
            for (var i = 0; i < offsets.Count; i++)
            {
                var delay = i * def.StaggerSeconds;
                if (def.Motion == ScreenStripMotion.Sweep)
                    hazards.Add(ScreenHazard.Sweep(delay, def.TelegraphSeconds, def.DamageFraction,
                        center - along * (length * .5f) + across * offsets[i], along, length, width, def.BodyLengthH * u, def.SpeedH * u));
                else
                    hazards.Add(ScreenHazard.Rect(delay, def.TelegraphSeconds, def.StrikeSeconds, def.DamageFraction,
                        center + across * offsets[i], along, length, width));
            }
        }

        private static void Cross(ScreenEventDefinition def, Rect view, System.Random random, List<ScreenHazard> hazards)
        {
            var u = view.height;
            var width = def.WidthH * u;
            var x = view.center.x + Signed(random) * def.CrossJitter * view.width;
            var y = view.center.y + Signed(random) * def.CrossJitter * view.height;
            hazards.Add(ScreenHazard.Rect(0f, def.TelegraphSeconds, def.StrikeSeconds, def.DamageFraction,
                new Vector2(x, view.center.y), Vector2.up, view.height + 2f * EdgeOvershootH * u, width));
            hazards.Add(ScreenHazard.Rect(0f, def.TelegraphSeconds, def.StrikeSeconds, def.DamageFraction,
                new Vector2(view.center.x, y), Vector2.right, view.width + 2f * EdgeOvershootH * u, width));
        }

        private static void Circles(ScreenEventDefinition def, Rect view, Vector2 player, float playerRadius, System.Random random,
            List<ScreenHazard> hazards)
        {
            var u = view.height;
            var count = random.Next(def.CountMin, def.CountMax + 1);
            var placed = new List<(Vector2 center, float radius)>();
            for (var i = 0; i < count; i++)
            {
                for (var attempt = 0; attempt < PlacementAttempts; attempt++)
                {
                    var radius = Mathf.Lerp(def.RadiusMinH, def.RadiusMaxH, (float)random.NextDouble()) * u;
                    if (radius * 2f >= Mathf.Min(view.width, view.height)) continue;
                    Vector2 center;
                    if (def.Placement == ScreenCirclePlacement.AroundPlayer)
                    {
                        var distance = Mathf.Lerp(def.PlayerDistanceMinH, def.PlayerDistanceMaxH, (float)random.NextDouble()) * u;
                        var theta = (float)random.NextDouble() * Mathf.PI * 2f;
                        center = player + new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * distance;
                    }
                    else
                    {
                        center = new Vector2(view.xMin + (float)random.NextDouble() * view.width, view.yMin + (float)random.NextDouble() * view.height);
                    }
                    center = Inset(view, center, radius);
                    if ((center - player).magnitude < radius + playerRadius + def.PlayerClearanceH * u) continue;
                    if (!placed.TrueForAll(other => (other.center - center).magnitude >= other.radius + radius + def.GapH * u)) continue;
                    placed.Add((center, radius));
                    hazards.Add(ScreenHazard.Circle(placed.Count - 1 == 0 ? 0f : (placed.Count - 1) * def.StaggerSeconds,
                        def.TelegraphSeconds, def.StrikeSeconds, def.DamageFraction, center, radius));
                    break;
                }
            }
            if (hazards.Count > 0) return;
            // The screen was too small for the rules: one circle, as far from the player as the screen allows.
            var fallback = Mathf.Lerp(def.RadiusMinH, def.RadiusMaxH, .5f) * u;
            var farthest = new Vector2(view.center.x + (player.x < view.center.x ? 1f : -1f) * view.width * .25f, view.center.y);
            hazards.Add(ScreenHazard.Circle(0f, def.TelegraphSeconds, def.StrikeSeconds, def.DamageFraction, Inset(view, farthest, fallback), fallback));
        }

        private static void Ring(ScreenEventDefinition def, Rect view, System.Random random, List<ScreenHazard> hazards)
        {
            var u = view.height;
            var center = view.center;
            var thickness = def.ThicknessH * u;
            var reach = new Vector2(view.width, view.height).magnitude * .5f + thickness;
            var theta = (float)random.NextDouble() * Mathf.PI * 2f;
            hazards.Add(ScreenHazard.Ring(0f, def.TelegraphSeconds, def.DamageFraction, center, def.StartRadiusH * u, reach,
                def.SpeedH * u, thickness, new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)), def.GapWidthH * u));
        }

        private static void HalfScreen(ScreenEventDefinition def, Rect view, System.Random random, List<ScreenHazard> hazards)
        {
            var u = view.height;
            var side = random.Next(4);
            var margin = EdgeOvershootH * u;
            if (side < 2)
            {
                // Left or right half: the dividing line is vertical.
                var line = view.center.x + Signed(random) * def.SplitJitter * view.width;
                var far = side == 0 ? view.xMin - margin : view.xMax + margin;
                hazards.Add(ScreenHazard.Rect(0f, def.TelegraphSeconds, def.StrikeSeconds, def.DamageFraction,
                    new Vector2((line + far) * .5f, view.center.y), Vector2.right, Mathf.Abs(far - line), view.height + 2f * margin));
            }
            else
            {
                var line = view.center.y + Signed(random) * def.SplitJitter * view.height;
                var far = side == 2 ? view.yMin - margin : view.yMax + margin;
                hazards.Add(ScreenHazard.Rect(0f, def.TelegraphSeconds, def.StrikeSeconds, def.DamageFraction,
                    new Vector2(view.center.x, (line + far) * .5f), Vector2.up, Mathf.Abs(far - line), view.width + 2f * margin));
            }
        }

        private static void Corners(ScreenEventDefinition def, Rect view, System.Random random, List<ScreenHazard> hazards)
        {
            var u = view.height;
            var margin = EdgeOvershootH * u;
            var width = def.CornerWidthFraction * view.width;
            var height = def.CornerHeightFraction * view.height;
            // Clockwise from the top left: the player runs round the middle instead of picking a side.
            var corners = new[]
            {
                new Vector2(view.xMin + width * .5f - margin * .5f, view.yMax - height * .5f + margin * .5f),
                new Vector2(view.xMax - width * .5f + margin * .5f, view.yMax - height * .5f + margin * .5f),
                new Vector2(view.xMax - width * .5f + margin * .5f, view.yMin + height * .5f - margin * .5f),
                new Vector2(view.xMin + width * .5f - margin * .5f, view.yMin + height * .5f - margin * .5f)
            };
            var start = random.Next(4);
            var step = random.Next(2) == 0 ? 1 : 3;
            for (var i = 0; i < 4; i++)
                hazards.Add(ScreenHazard.Rect(i * def.StaggerSeconds, def.TelegraphSeconds, def.StrikeSeconds, def.DamageFraction,
                    corners[(start + i * step) % 4], Vector2.right, width + margin, height + margin));
        }

        private static void Fan(ScreenEventDefinition def, Rect view, System.Random random, List<ScreenHazard> hazards)
        {
            var u = view.height;
            var edge = random.Next(4);
            var spread = Signed(random) * .2f;
            Vector2 apex;
            switch (edge)
            {
                case 0: apex = new Vector2(view.xMin, view.center.y + spread * view.height); break;
                case 1: apex = new Vector2(view.xMax, view.center.y + spread * view.height); break;
                case 2: apex = new Vector2(view.center.x + spread * view.width, view.yMin); break;
                default: apex = new Vector2(view.center.x + spread * view.width, view.yMax); break;
            }
            var inward = (view.center - apex).normalized;
            var length = new Vector2(view.width, view.height).magnitude + StripOvershootH * u;
            foreach (var degrees in def.AnglesDegrees)
            {
                var radians = degrees * Mathf.Deg2Rad;
                var direction = new Vector2(inward.x * Mathf.Cos(radians) - inward.y * Mathf.Sin(radians),
                    inward.x * Mathf.Sin(radians) + inward.y * Mathf.Cos(radians));
                hazards.Add(ScreenHazard.Sweep(0f, def.TelegraphSeconds, def.DamageFraction, apex, direction, length,
                    def.WidthH * u, def.BodyLengthH * u, def.SpeedH * u));
            }
        }

        private static void Judgment(ScreenEventDefinition def, Rect view, Vector2 player, System.Random random, List<ScreenHazard> hazards)
        {
            var u = view.height;
            var radius = def.SafeRadiusH * u;
            var distance = Mathf.Lerp(def.PlayerDistanceMinH, def.PlayerDistanceMaxH, (float)random.NextDouble()) * u;
            var theta = (float)random.NextDouble() * Mathf.PI * 2f;
            var center = Inset(view, player + new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * distance, radius);
            hazards.Add(ScreenHazard.Circle(0f, def.TelegraphSeconds, def.StrikeSeconds, def.DamageFraction, center, radius, outside: true));
        }

        /// <summary>Pulls a circle centre inside the visible rectangle so the whole circle is on screen.</summary>
        public static Vector2 Inset(Rect view, Vector2 center, float radius)
        {
            var halfWidth = Mathf.Max(0f, view.width * .5f - radius);
            var halfHeight = Mathf.Max(0f, view.height * .5f - radius);
            return new Vector2(Mathf.Clamp(center.x, view.center.x - halfWidth, view.center.x + halfWidth),
                Mathf.Clamp(center.y, view.center.y - halfHeight, view.center.y + halfHeight));
        }

        private static float Signed(System.Random random) => (float)random.NextDouble() * 2f - 1f;

        private static void Shuffle(List<float> values, System.Random random)
        {
            for (var i = values.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (values[i], values[j]) = (values[j], values[i]);
            }
        }
    }
}
