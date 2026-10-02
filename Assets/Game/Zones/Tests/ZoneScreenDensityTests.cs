using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Game.Zones.Tests
{
    public sealed class ZoneScreenDensityTests
    {
        [Test]
        public void Generate_CappedLayout_EverySlidingScreenHasAtMostThreeAndSeedsVary()
        {
            var data = ZoneTestData.Layout(new[] { ZoneTestData.Haste() }, ("T-HASTE", 36));
            data.MaxPerScreen = 3; data.ScreenPadding = 2; data.MinGap = 1;
            var layout = new ZoneLayoutDefinition(data);
            var screen = new Vector2(24f, 14f);
            var window = screen + Vector2.one * 4f;
            Vector2[] first = null;
            for (var seed = 0; seed < 32; seed++)
            {
                var placed = ZoneLayoutGenerator.Generate(layout, 160f, Vector2.zero, null, seed, screen);
                Assert.AreEqual(36, placed.Count);
                var points = placed.Select(p => p.Center).ToArray();
                foreach (var x in points)
                    foreach (var y in points)
                        Assert.LessOrEqual(points.Count(p => p.x >= x.x && p.x <= x.x + window.x &&
                            p.y >= y.y && p.y <= y.y + window.y), 3, "Sliding screen, seed " + seed);
                if (first == null) first = points;
                else Assert.IsFalse(first.SequenceEqual(points), "Fresh seeds must change arrangement.");
                CollectionAssert.AreEqual(points, ZoneLayoutGenerator.Generate(layout, 160f, Vector2.zero, null, seed, screen).Select(p => p.Center));
            }
        }

        [Test]
        public void Generate_CapWithoutCamera_RejectsInsteadOfSilentlyIgnoringLimit()
        {
            var data = ZoneTestData.Layout(new[] { ZoneTestData.Haste() }, ("T-HASTE", 4));
            data.MaxPerScreen = 3;
            Assert.Throws<ArgumentException>(() => ZoneLayoutGenerator.Generate(new ZoneLayoutDefinition(data), 160f, Vector2.zero, null, 1));
        }
    }
}
