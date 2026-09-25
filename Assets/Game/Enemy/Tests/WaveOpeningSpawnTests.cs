using System;
using NUnit.Framework;

namespace Game.Enemy.Tests
{
    /// <summary>DECISION-0057 (playtest 2026-09-25_5233a664 OBS-01): opening screen-edge spawn.</summary>
    public sealed class WaveOpeningSpawnTests
    {
        private const float HalfWidth = 8.9f;
        private const float HalfHeight = 5f;
        private const float Margin = 1f;

        [TestCase(0d, HalfWidth + Margin, 0f)]
        [TestCase(Math.PI, -(HalfWidth + Margin), 0f)]
        [TestCase(Math.PI / 2d, 0f, HalfHeight + Margin)]
        [TestCase(Math.PI * 1.5d, 0f, -(HalfHeight + Margin))]
        public void Offset_OnAxes_LandsJustOutsideTheMatchingViewEdge(double angle, float expectedX, float expectedY)
        {
            WaveScreenEdgePlacement.Offset(angle, HalfWidth, HalfHeight, Margin, out var x, out var y);

            Assert.AreEqual(expectedX, x, 1e-4f);
            Assert.AreEqual(expectedY, y, 1e-4f);
        }

        [Test]
        public void Offset_AnyAngle_IsOnTheGrownViewRectangle_AndCloserThanTheCornerRadius()
        {
            for (var i = 0; i < 64; i++)
            {
                var angle = i * Math.PI * 2d / 64d;
                WaveScreenEdgePlacement.Offset(angle, HalfWidth, HalfHeight, Margin, out var x, out var y);

                var onVertical = Math.Abs(Math.Abs(x) - (HalfWidth + Margin)) < 1e-3f && Math.Abs(y) <= HalfHeight + Margin + 1e-3f;
                var onHorizontal = Math.Abs(Math.Abs(y) - (HalfHeight + Margin)) < 1e-3f && Math.Abs(x) <= HalfWidth + Margin + 1e-3f;
                Assert.IsTrue(onVertical || onHorizontal, $"angle {angle}: ({x}, {y}) is not on the view edge.");
                Assert.IsTrue(Math.Abs(x) > HalfWidth || Math.Abs(y) > HalfHeight, $"angle {angle}: spawn is visible.");
            }
        }

        [Test]
        public void Director_OpeningIsActiveOnlyBeforeItsDuration()
        {
            var timeline = new WaveTimelineDefinition("FIXTURE-WAVE-OPENING", 7, WaveTestData.SpawnRadius,
                new[] { WaveTestData.Phase("FIXTURE-PHASE-ORDINARY", WavePhaseTag.Ordinary, 30f, 1f, 10, null, WaveTestData.Entry("FIXTURE-ENEMY-A")) },
                openingSpawn: new WaveOpeningSpawnDefinition(20f, Margin));
            var director = new WaveDirector(timeline, WaveTestData.TestEnemies(), 30f);

            director.Advance(0f, 0f, true, 0);
            Assert.IsTrue(director.IsOpeningSpawnActive);
            director.Advance(19.9f, 0f, true, 0);
            Assert.IsTrue(director.IsOpeningSpawnActive);
            director.Advance(20f, 0f, true, 0);
            Assert.IsFalse(director.IsOpeningSpawnActive);
        }

        [Test]
        public void Director_WithoutOpening_NeverUsesIt()
        {
            var director = new WaveDirector(WaveTestData.ThreePhaseTimeline(), WaveTestData.TestEnemies(), 30f);
            director.Advance(0f, 0f, true, 0);
            Assert.IsFalse(director.IsOpeningSpawnActive);
        }

        [Test]
        public void Catalog_ReadsOpening_AndRejectsMissingFieldsByName()
        {
            const string template = "{{\"id\":\"FIXTURE-T\",\"seed\":1,\"spawnRadius\":6,{0}\"phases\":[{{\"id\":\"FIXTURE-P\",\"displayName\":\"P\",\"tag\":\"Ordinary\",\"spawnMode\":\"Continuous\",\"durationSeconds\":10,\"spawnIntervalSeconds\":1,\"maxAliveEnemies\":4,\"composition\":[{{\"enemyId\":\"FIXTURE-ENEMY-A\",\"weight\":1}}]}}]}}";

            var timeline = FixtureWaveTimelineCatalog.FromJson(string.Format(template, "\"openingSpawn\":{\"durationSeconds\":20,\"screenMargin\":1},"));
            Assert.AreEqual(20f, timeline.OpeningSpawn.DurationSeconds);
            Assert.AreEqual(1f, timeline.OpeningSpawn.ScreenMargin);
            Assert.IsNull(FixtureWaveTimelineCatalog.FromJson(string.Format(template, string.Empty)).OpeningSpawn);

            var missing = Assert.Throws<InvalidOperationException>(() =>
                FixtureWaveTimelineCatalog.FromJson(string.Format(template, "\"openingSpawn\":{\"durationSeconds\":20},")));
            StringAssert.Contains("screenMargin", missing.Message);
            Assert.Throws<ArgumentOutOfRangeException>(() => new WaveOpeningSpawnDefinition(0f, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WaveOpeningSpawnDefinition(20f, -1f));
        }
    }
}
