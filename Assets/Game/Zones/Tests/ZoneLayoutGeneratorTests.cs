using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Game.Zones.Tests
{
    public sealed class ZoneLayoutGeneratorTests
    {
        private const float Side = 120f;

        private static ZoneLayoutDefinition Layout() => new ZoneLayoutDefinition(ZoneTestData.Layout(
            new[]
            {
                ZoneTestData.Slow(), ZoneTestData.Haste(mode: ZoneLifetimeMode.Pulsing), ZoneTestData.Regeneration(),
                ZoneTestData.Arcane(), ZoneTestData.Rift(), ZoneTestData.Portal()
            },
            ("T-SLOW", 2), ("T-HASTE", 2), ("T-REGEN", 1), ("T-ARCANE", 2), ("T-RIFT", 2), ("T-PORTAL", 2)));

        private static IReadOnlyList<IReadOnlyList<Vector2>> Obstacles() => new IReadOnlyList<Vector2>[]
        {
            new[] { new Vector2(20, 20), new Vector2(34, 20), new Vector2(34, 30), new Vector2(20, 30) },
            new[] { new Vector2(-40, -10), new Vector2(-25, -10), new Vector2(-30, 12) }
        };

        [Test]
        public void Generate_PlacesEveryZone_WithAllClearances()
        {
            var layout = Layout();
            var half = Side * .5f - layout.EdgeMargin;
            for (var seed = 0; seed < 50; seed++)
            {
                var zones = ZoneLayoutGenerator.Generate(layout, Side, Vector2.zero, Obstacles(), seed);
                Assert.AreEqual(layout.Zones.Count, zones.Count, $"seed {seed}");
                foreach (var zone in zones)
                {
                    var r = zone.Effect.Radius;
                    Assert.LessOrEqual(Mathf.Abs(zone.Center.x), half - r + 1e-3f, $"seed {seed} zone {zone.Index}");
                    Assert.LessOrEqual(Mathf.Abs(zone.Center.y), half - r + 1e-3f, $"seed {seed} zone {zone.Index}");
                    Assert.GreaterOrEqual(zone.Center.magnitude, layout.StartClearRadius + r - 1e-3f, $"seed {seed} start circle clear");
                    var extra = zone.Effect.Kind == ZoneEffectKind.Portal ? zone.Effect.PortalExitDistance + 1.5f : 0f;
                    foreach (var outline in Obstacles())
                        Assert.GreaterOrEqual(ZoneGeometry.DiscClearance(outline, zone.Center, r + layout.ObstacleClearance + extra), -1e-3f,
                            $"seed {seed} zone {zone.Index} clear of obstacles (portals also keep their exit clear)");
                }
                for (var i = 0; i < zones.Count; i++)
                    for (var j = i + 1; j < zones.Count; j++)
                        Assert.GreaterOrEqual(Vector2.Distance(zones[i].Center, zones[j].Center),
                            zones[i].Effect.Radius + zones[j].Effect.Radius + layout.MinGap - 1e-3f, $"seed {seed} {i}/{j}");
            }
        }

        [Test]
        public void Generate_PairsPortalsAndKeepsThemApart()
        {
            var layout = Layout();
            for (var seed = 0; seed < 30; seed++)
            {
                var zones = ZoneLayoutGenerator.Generate(layout, Side, Vector2.zero, Obstacles(), seed);
                var portals = zones.Where(z => z.Effect.Kind == ZoneEffectKind.Portal).ToList();
                Assert.AreEqual(2, portals.Count);
                Assert.AreEqual(portals[1].Index, portals[0].PartnerIndex);
                Assert.AreEqual(portals[0].Index, portals[1].PartnerIndex);
                Assert.GreaterOrEqual(Vector2.Distance(portals[0].Center, portals[1].Center), portals[0].Effect.PortalMinPairDistance - 1e-3f);
                Assert.IsTrue(zones.Where(z => z.Effect.Kind != ZoneEffectKind.Portal).All(z => z.PartnerIndex == -1));
            }
        }

        [Test]
        public void Generate_PulsingZonesGetAPhaseInsideTheirCycle_PermanentZonesNone()
        {
            var layout = Layout();
            var zones = ZoneLayoutGenerator.Generate(layout, Side, Vector2.zero, Obstacles(), 3);
            foreach (var zone in zones)
            {
                if (zone.Effect.Lifetime == ZoneLifetimeMode.Permanent) Assert.AreEqual(0f, zone.PhaseSeconds);
                else Assert.That(zone.PhaseSeconds, Is.InRange(0f, zone.Effect.PulsePeriodSeconds));
            }
            Assert.AreEqual(2, zones.Count(z => z.Effect.Lifetime == ZoneLifetimeMode.Pulsing));
        }

        [Test]
        public void Generate_SameSeedSameLayout_DifferentSeedsDiffer()
        {
            var layout = Layout();
            var first = ZoneLayoutGenerator.Generate(layout, Side, Vector2.zero, Obstacles(), 11).Select(z => z.Center).ToList();
            CollectionAssert.AreEqual(first, ZoneLayoutGenerator.Generate(layout, Side, Vector2.zero, Obstacles(), 11).Select(z => z.Center).ToList());
            CollectionAssert.AreNotEqual(first, ZoneLayoutGenerator.Generate(layout, Side, Vector2.zero, Obstacles(), 12).Select(z => z.Center).ToList());
        }

        [Test]
        public void Generate_ImpossibleLayout_Throws()
        {
            var data = ZoneTestData.Layout(new[] { ZoneTestData.Slow() }, ("T-SLOW", 30));
            data.MaxRestarts = 2;
            Assert.Throws<InvalidOperationException>(() => ZoneLayoutGenerator.Generate(new ZoneLayoutDefinition(data), 60f, Vector2.zero, null, 1));
        }

        [Test]
        public void Layout_RejectsUnknownDuplicateOddPortalsAndEmpty()
        {
            var effects = new[] { ZoneTestData.Slow(), ZoneTestData.Portal() };
            Assert.Catch<ArgumentException>(() => new ZoneLayoutDefinition(ZoneTestData.Layout(effects, ("MISSING", 1))));
            Assert.Catch<ArgumentException>(() => new ZoneLayoutDefinition(ZoneTestData.Layout(effects, ("T-SLOW", 1), ("T-SLOW", 1))));
            Assert.Catch<ArgumentException>(() => new ZoneLayoutDefinition(ZoneTestData.Layout(effects, ("T-PORTAL", 3))), "Portals come in pairs.");
            Assert.Catch<ArgumentException>(() => new ZoneLayoutDefinition(ZoneTestData.Layout(effects)));
            Assert.Catch<ArgumentException>(() => new ZoneLayoutDefinition(ZoneTestData.Layout(new[] { ZoneTestData.Slow(), ZoneTestData.Slow() },
                ("T-SLOW", 1))), "Duplicate effect ids.");
        }
    }
}
