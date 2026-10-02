using System.Collections.Generic;
using System.Linq;
using Game.Content;
using Game.Presentation;
using Game.Zones;
using NUnit.Framework;
using UnityEngine;

namespace Game.Bootstrap.Tests
{
    public sealed class ProductionMonasteryObstacleTests
    {
        [Test]
        public void ObstacleLayouts_UseSixApprovedPropsAndLeaveRoomForAllAltars()
        {
            var catalog = RuntimeContentCatalog.CreateProduction();
            var field = catalog.FieldEnvironmentPresentations[new ContentId("FIELD-007-ENVIRONMENT")];
            var first = catalog.FieldEnvironmentPresentations[new ContentId("FIELD-001-ENVIRONMENT")];
            Assert.AreEqual(first.ObstacleLayout.CellSize, field.ObstacleLayout.CellSize);
            Assert.AreEqual(first.ObstacleLayout.PatternsPerCell, field.ObstacleLayout.PatternsPerCell);
            Assert.AreEqual(6, field.ObstacleLayout.Patterns.Count);
            foreach (var pattern in field.ObstacleLayout.Patterns)
                foreach (var piece in pattern.Pieces)
                    catalog.Registry.Get<SpriteDefinition>(piece.VisualId).RequireRole(SpriteRole.Prop);
            for (var seed = 0; seed < 8; seed++)
            {
                var zones = ZoneLayoutGenerator.Generate(field.ZoneLayout, field.ArenaSideLength.Value,
                    Vector2.zero, null, seed, new Vector2(24, 14));
                var exclusions = zones.Select(zone => new FieldObstacleExclusion(zone.Center,
                    field.ZoneLayout.AltarObstacleRadius.Value + field.ZoneLayout.ObstacleClearance)).ToArray();
                var obstacles = FieldObstacleLayoutGenerator.Generate(field.ObstacleLayout, field.ArenaSideLength.Value,
                    Vector2.zero, seed, "monastery", exclusions);
                var cells = Mathf.FloorToInt((field.ArenaSideLength.Value - 2f * field.ObstacleLayout.EdgeMargin) / field.ObstacleLayout.CellSize);
                var targetCount = cells * cells * field.ObstacleLayout.PatternsPerCell;
                Assert.That(obstacles.Count, Is.InRange(targetCount - 8, targetCount));
                var outlines = new List<IReadOnlyList<Vector2>>();
                foreach (var obstacle in obstacles)
                {
                    var min = new Vector2(obstacle.X - obstacle.Width * .5f, obstacle.Y - obstacle.Height * .5f);
                    var max = new Vector2(obstacle.X + obstacle.Width * .5f, obstacle.Y + obstacle.Height * .5f);
                    outlines.Add(new[] { min, new Vector2(max.x, min.y), max, new Vector2(min.x, max.y) });
                }
                Assert.AreEqual(36, zones.Count);
                foreach (var zone in zones)
                    Assert.IsTrue(outlines.All(outline => ZoneGeometry.DiscClearance(outline, zone.Center,
                        field.ZoneLayout.AltarObstacleRadius.Value + field.ZoneLayout.ObstacleClearance) >= 0f));
            }
        }
    }
}
