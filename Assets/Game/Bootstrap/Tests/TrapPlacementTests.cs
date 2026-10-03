using System.Collections.Generic;
using System.Linq;
using Game.Traps;
using Game.Zones;
using NUnit.Framework;
using UnityEngine;
using static Game.Bootstrap.Tests.TrapTestLayouts;

namespace Game.Bootstrap.Tests
{
    /// <summary>DECISION-0156: traps and barrels are scattered per run, clear of the start, edges, obstacles and each other.</summary>
    public sealed class TrapPlacementTests
    {
        private const float Arena = 100f;

        private static TrapLayoutDefinition SampleLayout() => Layout(new[] { Projectile("p") }, new[]
        {
            Type("big", "fixed", new[] { Shots("p") }, body: 1f, count: 5),
            Type("small", "aim", new[] { Shots("p") }, body: .5f, count: 8)
        }, barrels: Barrels(12, .25f));

        [Test]
        public void Generate_PlacesEveryTrapAndBarrel_ClearOfStartEdgeAndEachOther()
        {
            var layout = SampleLayout();
            var set = TrapPlacementGenerator.Generate(layout, Arena, Vector2.zero, null, 7);
            Assert.AreEqual(0, set.SkippedTraps);
            Assert.AreEqual(0, set.SkippedBarrels);
            Assert.AreEqual(13, set.Traps.Count);
            Assert.AreEqual(12, set.Barrels.Count);
            var bodies = set.Traps.Select(t => (c: t.Center, r: t.Type.BodyRadius))
                .Concat(set.Barrels.Select(b => (c: b.Center, r: layout.Barrels.BodyRadius))).ToList();
            foreach (var (center, radius) in bodies)
            {
                Assert.GreaterOrEqual(center.magnitude, layout.StartClearRadius + radius);
                Assert.LessOrEqual(Mathf.Abs(center.x), Arena * .5f - layout.EdgeMargin - radius + 1e-4f);
                Assert.LessOrEqual(Mathf.Abs(center.y), Arena * .5f - layout.EdgeMargin - radius + 1e-4f);
            }
            for (var i = 0; i < bodies.Count; i++)
                for (var j = i + 1; j < bodies.Count; j++)
                    Assert.GreaterOrEqual(Vector2.Distance(bodies[i].c, bodies[j].c), bodies[i].r + bodies[j].r + layout.MinGap - 1e-4f);
        }

        [Test]
        public void Generate_IsDeterministicPerSeed_AndDiffersBetweenSeeds()
        {
            var layout = SampleLayout();
            var a = TrapPlacementGenerator.Generate(layout, Arena, Vector2.zero, null, 11);
            var b = TrapPlacementGenerator.Generate(layout, Arena, Vector2.zero, null, 11);
            var c = TrapPlacementGenerator.Generate(layout, Arena, Vector2.zero, null, 12);
            CollectionAssert.AreEqual(a.Traps.Select(t => t.Center).ToArray(), b.Traps.Select(t => t.Center).ToArray());
            CollectionAssert.AreEqual(a.Barrels.Select(t => (t.Center, t.Explosive)).ToArray(), b.Barrels.Select(t => (t.Center, t.Explosive)).ToArray());
            CollectionAssert.AreNotEqual(a.Traps.Select(t => t.Center).ToArray(), c.Traps.Select(t => t.Center).ToArray());
        }

        [Test]
        public void Generate_MakesExactlyTheExplosiveShareOfBarrelsExplosive()
        {
            var layout = SampleLayout();
            var set = TrapPlacementGenerator.Generate(layout, Arena, Vector2.zero, null, 3);
            Assert.AreEqual(layout.Barrels.ExplosiveCount, set.Barrels.Count(b => b.Explosive));
            Assert.AreEqual(3, layout.Barrels.ExplosiveCount);
        }

        [Test]
        public void Generate_KeepsClearOfObstacleOutlines()
        {
            var layout = SampleLayout();
            var outlines = Box(-30f, -30f, 30f, 30f);
            var set = TrapPlacementGenerator.Generate(layout, Arena, new Vector2(45f, 45f), outlines, 5);
            foreach (var trap in set.Traps)
                Assert.GreaterOrEqual(ZoneGeometry.DiscClearance(outlines[0], trap.Center, trap.Type.BodyRadius + layout.ObstacleClearance), 0f);
            foreach (var barrel in set.Barrels)
                Assert.GreaterOrEqual(ZoneGeometry.DiscClearance(outlines[0], barrel.Center, layout.Barrels.BodyRadius + layout.ObstacleClearance), 0f);
        }

        [Test]
        public void Generate_FixedTrapsTakeOnlyTheirAuthoredRotations_AndAimedOnesNone()
        {
            var layout = SampleLayout();
            var set = TrapPlacementGenerator.Generate(layout, Arena, Vector2.zero, null, 9);
            Assert.IsTrue(set.Traps.Where(t => t.Type.Heading == TrapHeadingMode.Fixed).All(t => t.RotationDegrees == 0f));
            Assert.IsTrue(set.Traps.Where(t => t.Type.Heading == TrapHeadingMode.Aim).All(t => t.RotationDegrees == 0f));
        }

        [Test]
        public void Generate_PutsTheStartTrapOnTheFirstScreen_CountedInItsType_WithItsModel()
        {
            var layout = Layout(new[] { Projectile("p") }, new[] { Type("spin", "fixed", new[] { Shots("p") }, count: 3, spin: 40f) },
                models: new[] { Model() }, starts: new[] { Start("spin", 5.5f, 2f, "cross") });
            var set = TrapPlacementGenerator.Generate(layout, Arena, Vector2.zero, null, 4);
            Assert.AreEqual(3, set.Traps.Count, "The start trap is one of the type's three.");
            var start = set.Traps.Single(t => t.ModelKey == "cross");
            Assert.AreEqual(new Vector2(5.5f, 2f), start.Center, "Free space: exactly at the authored offset.");
            Assert.AreEqual(2, set.Traps.Count(t => t.ModelKey == null));
        }

        [Test]
        public void Generate_MovesTheStartTrapAroundTheStartWhenItsSpotIsTaken()
        {
            var layout = Layout(new[] { Projectile("p") }, new[] { Type("spin", "fixed", new[] { Shots("p") }, spin: 40f) },
                models: new[] { Model() }, starts: new[] { Start("spin", 5.5f, 0f, "cross") });
            var blocker = Box(4f, -3f, 8f, 3f);
            var set = TrapPlacementGenerator.Generate(layout, Arena, Vector2.zero, blocker, 4);
            Assert.AreEqual(0, set.SkippedTraps);
            var start = set.Traps[0];
            Assert.AreEqual(5.5f, start.Center.magnitude, .01f, "The same distance from the start, rotated to a free spot.");
            Assert.GreaterOrEqual(ZoneGeometry.DiscClearance(blocker[0], start.Center, start.Type.BodyRadius + layout.ObstacleClearance), 0f);
        }

        [Test]
        public void Generate_GivesEveryPlacementItsTypeModel_AndStartBarrelsKeepTheirKind()
        {
            var layout = Layout(new[] { Projectile("p") }, new[] { Type("t", "aim", new[] { Shots("p") }, count: 3, model: "cross") },
                barrels: Barrels(5, .4f), models: new[] { Model() },
                startBarrels: new[] { StartBarrel(3f, 0f, true), StartBarrel(-3f, 0f, false) });
            var set = TrapPlacementGenerator.Generate(layout, Arena, Vector2.zero, null, 6);
            Assert.IsTrue(set.Traps.All(t => t.ModelKey == "cross"));
            Assert.AreEqual(5, set.Barrels.Count);
            Assert.AreEqual(new Vector2(3f, 0f), set.Barrels[0].Center);
            Assert.IsTrue(set.Barrels[0].Explosive);
            Assert.AreEqual(new Vector2(-3f, 0f), set.Barrels[1].Center);
            Assert.IsFalse(set.Barrels[1].Explosive);
            Assert.AreEqual(layout.Barrels.ExplosiveCount, set.Barrels.Count(b => b.Explosive), "The total still follows the share.");
        }

        [Test]
        public void Density_PickCount_FollowsTheWeights()
        {
            var density = new TrapDensityDefinition(Density(2, 7, 1));
            Assert.AreEqual(1, density.PickCount(0.0));
            Assert.AreEqual(1, density.PickCount(0.19));
            Assert.AreEqual(2, density.PickCount(0.2));
            Assert.AreEqual(2, density.PickCount(0.89));
            Assert.AreEqual(3, density.PickCount(0.9));
            Assert.AreEqual(3, density.PickCount(0.999));
        }

        [Test]
        public void Density_GivesEveryScreenCellMostlyTwoTraps_SometimesOne_RarelyThree()
        {
            var layout = Layout(new[] { Projectile("p") }, new[]
            {
                Type("a", "aim", new[] { Shots("p") }, body: .6f, count: 3), Type("b", "aim", new[] { Shots("p") }, body: .8f, count: 1)
            }, density: Density(2, 7, 1));
            const float arena = 200f;
            var set = TrapPlacementGenerator.Generate(layout, arena, Vector2.zero, null, 21);
            var half = arena * .5f - layout.EdgeMargin;
            var columns = Mathf.RoundToInt(2f * half / 17.8f);
            var rows = Mathf.RoundToInt(2f * half / 10f);
            var cellWidth = 2f * half / columns;
            var cellHeight = 2f * half / rows;
            var perCell = new int[columns, rows];
            foreach (var trap in set.Traps)
            {
                var column = Mathf.Min(columns - 1, (int)((trap.Center.x + half) / cellWidth));
                var row = Mathf.Min(rows - 1, (int)((trap.Center.y + half) / cellHeight));
                perCell[column, row]++;
            }
            var counted = 0;
            var empties = "";
            var histogram = new int[5];
            for (var c = 0; c < columns; c++)
                for (var r = 0; r < rows; r++)
                {
                    var center = new Vector2(-half + (c + .5f) * cellWidth, -half + (r + .5f) * cellHeight);
                    if (center.magnitude < layout.StartClearRadius + 12f) continue; // around the start nothing is scattered
                    counted++;
                    histogram[Mathf.Min(4, perCell[c, r])]++;
                    if (perCell[c, r] == 0) empties += $"({c},{r}) ";
                }
            Assert.Greater(counted, 150, "Most of the arena is checked.");
            Assert.AreEqual(0, histogram[0], "Every cell away from the start has a trap. Empty: " + empties + $" of {columns}x{rows}");
            Assert.AreEqual(0, histogram[4], "Never more than three on a screen cell.");
            Assert.Greater(histogram[2], counted * .55f, "Mostly two.");
            Assert.Greater(histogram[1], 0, "Sometimes one.");
            Assert.Less(histogram[1], counted * .35f);
            Assert.Greater(histogram[3], 0, "Three exist but are rare.");
            Assert.Less(histogram[3], counted * .2f);
            Assert.IsTrue(set.Traps.Any(t => t.Type.Id.ToString() == "a") && set.Traps.Any(t => t.Type.Id.ToString() == "b"));
        }

        [Test]
        public void Density_IsDeterministicPerSeed_AndKeepsTheStartScreenTrapsAlongsideTheScatter()
        {
            var layout = Layout(new[] { Projectile("p") }, new[] { Type("a", "aim", new[] { Shots("p") }, count: 1) },
                models: new[] { Model() }, starts: new[] { Start("a", 5f, 2f) }, density: Density());
            var a = TrapPlacementGenerator.Generate(layout, 200f, Vector2.zero, null, 4);
            var b = TrapPlacementGenerator.Generate(layout, 200f, Vector2.zero, null, 4);
            CollectionAssert.AreEqual(a.Traps.Select(t => t.Center).ToArray(), b.Traps.Select(t => t.Center).ToArray());
            Assert.AreEqual(new Vector2(5f, 2f), a.Traps[0].Center, "The start trap comes first, at its authored spot.");
            Assert.Greater(a.Traps.Count, 100);
        }

        [Test]
        public void Generate_SkipsWhatCannotFit_InsteadOfLooping()
        {
            var layout = Layout(new[] { Projectile("p") }, new[] { Type("t", "aim", new[] { Shots("p") }, count: 500) });
            var set = TrapPlacementGenerator.Generate(layout, 30f, Vector2.zero, null, 1);
            Assert.Greater(set.SkippedTraps, 0);
            Assert.AreEqual(500, set.Traps.Count + set.SkippedTraps);
        }

        [Test]
        public void Generate_FixedTrapNeedsOpenSpaceInFront()
        {
            // A wall everywhere except a narrow open corridor: a fixed east-facing trap must not stand right before the wall.
            var layout = Layout(new[] { Projectile("p") }, new[] { Type("t", "fixed", new[] { Shots("p") }, count: 6) });
            var wall = new List<IReadOnlyList<Vector2>> { new[] { new Vector2(10f, -50f), new Vector2(12f, -50f), new Vector2(12f, 50f), new Vector2(10f, 50f) } };
            var set = TrapPlacementGenerator.Generate(layout, Arena, new Vector2(-45f, 0f), wall, 2);
            foreach (var trap in set.Traps)
            {
                var gap = 10f - trap.Center.x;
                if (trap.Center.x < 10f) Assert.GreaterOrEqual(gap, trap.Type.BodyRadius + layout.ForwardClearance, "Open space ahead of an east-facing trap.");
            }
        }
    }
}
