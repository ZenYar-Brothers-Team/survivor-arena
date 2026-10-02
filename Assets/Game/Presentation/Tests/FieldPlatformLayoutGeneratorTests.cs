using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Game.Presentation.Tests
{
    /// <summary>FIELD-009: the per-run circular-platform network (docs/prototypes/field009-platforms).</summary>
    public sealed class FieldPlatformLayoutGeneratorTests
    {
        private static FieldPlatformLayoutDefinition Profile() =>
            FixtureFieldEnvironmentPresentationCatalog.Load("Content/Presentation/ProductionFieldEnvironmentPresentation")
                ["FIELD-009-ENVIRONMENT"].PlatformLayout;

        [Test]
        public void SameSeed_GivesTheSameLayout()
        {
            var a = FieldPlatformLayoutGenerator.Generate(Profile(), 5);
            var b = FieldPlatformLayoutGenerator.Generate(Profile(), 5);
            CollectionAssert.AreEqual(a.Platforms.Select(p => (p.Center, p.Radius)), b.Platforms.Select(p => (p.Center, p.Radius)));
            CollectionAssert.AreEqual(a.Bridges.Select(x => (x.From, x.To)), b.Bridges.Select(x => (x.From, x.To)));
        }

        [Test]
        public void ManySeeds_AreConnectedInsideTheArenaWithinRadiusLimits()
        {
            var profile = Profile();
            var half = profile.ArenaSideLength * .5f;
            for (var seed = 1; seed <= 40; seed++)
            {
                var layout = FieldPlatformLayoutGenerator.Generate(profile, seed);
                Assert.GreaterOrEqual(layout.Platforms.Count, 2, $"seed {seed}");
                Assert.AreEqual(layout.Platforms.Count, Reach(layout), $"seed {seed}: every platform is reachable from the start");
                foreach (var disc in layout.Platforms)
                {
                    Assert.That(disc.Radius, Is.InRange(profile.MinimumRadius, profile.MaximumRadius), $"seed {seed}");
                    Assert.LessOrEqual(Mathf.Abs(disc.Center.x) + disc.Radius, half, $"seed {seed}");
                    Assert.LessOrEqual(Mathf.Abs(disc.Center.y) + disc.Radius, half, $"seed {seed}");
                }
                for (var i = 0; i < layout.Platforms.Count; i++)
                for (var j = i + 1; j < layout.Platforms.Count; j++)
                    Assert.GreaterOrEqual(Vector2.Distance(layout.Platforms[i].Center, layout.Platforms[j].Center) -
                                          layout.Platforms[i].Radius - layout.Platforms[j].Radius, profile.MinimumPlatformGap - .001f, $"seed {seed}");
            }
        }

        [Test]
        public void OnlyBigPlatformsAreDeadEnds()
        {
            var profile = Profile();
            for (var seed = 1; seed <= 40; seed++)
            {
                var layout = FieldPlatformLayoutGenerator.Generate(profile, seed);
                var degrees = layout.Degrees();
                for (var i = 0; i < degrees.Length; i++)
                    if (i != layout.StartIndex && degrees[i] <= 1)
                        Assert.GreaterOrEqual(layout.Platforms[i].Radius, profile.DeadEndMinimumRadius, $"seed {seed}: small dead end");
            }
        }

        [Test]
        public void NoTwoTooSmallPlatformsAreJoined()
        {
            var profile = Profile();
            for (var seed = 1; seed <= 40; seed++)
            {
                var layout = FieldPlatformLayoutGenerator.Generate(profile, seed);
                foreach (var bridge in layout.Bridges)
                    Assert.IsFalse(layout.Platforms[bridge.From].Radius < profile.SmallRadiusMax &&
                                   layout.Platforms[bridge.To].Radius < profile.SmallRadiusMax, $"seed {seed}: two small platforms joined");
            }
        }

        [Test]
        public void Books_AreAboutHalfOfThePlatforms_NeverOnTheStart()
        {
            var profile = Profile();
            int books = 0, platforms = 0;
            for (var seed = 1; seed <= 40; seed++)
            {
                var layout = FieldPlatformLayoutGenerator.Generate(profile, seed);
                Assert.IsFalse(layout.Platforms[layout.StartIndex].HasBook, $"seed {seed}");
                books += layout.Platforms.Count(p => p.HasBook);
                platforms += layout.Platforms.Count - 1;
            }
            Assert.That(books / (float)platforms, Is.InRange(profile.BookChance - .08f, profile.BookChance + .08f));
        }

        [Test]
        public void Walkable_IsPlatformsAndBridgesOnly()
        {
            var layout = FieldPlatformLayoutGenerator.Generate(Profile(), 7);
            Assert.IsTrue(layout.IsWalkable(layout.SpawnPosition));
            foreach (var disc in layout.Platforms) Assert.IsTrue(layout.IsWalkable(disc.Center));
            var bridge = layout.Bridges[0];
            var from = layout.Platforms[bridge.From];
            var to = layout.Platforms[bridge.To];
            var direction = (to.Center - from.Center).normalized;
            var middle = from.Center + direction * (from.Radius + bridge.Gap * .5f);
            Assert.IsTrue(layout.IsWalkable(middle), "the span of a bridge is walkable");
            Assert.IsFalse(layout.IsWalkable(middle + Vector2.Perpendicular(direction) * (layout.Profile.BridgeWidth * .5f + .5f)),
                "beside a bridge is the void");
        }

        private static int Reach(FieldPlatformLayout layout)
        {
            var seen = new System.Collections.Generic.HashSet<int> { layout.StartIndex };
            var changed = true;
            while (changed)
            {
                changed = false;
                foreach (var bridge in layout.Bridges)
                {
                    if (seen.Contains(bridge.From) && seen.Add(bridge.To)) changed = true;
                    if (seen.Contains(bridge.To) && seen.Add(bridge.From)) changed = true;
                }
            }
            return seen.Count;
        }
    }
}
