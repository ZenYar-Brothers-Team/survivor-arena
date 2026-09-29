using System;
using NUnit.Framework;

namespace Game.Progression.Tests
{
    public sealed class BookUpgradeCountTests
    {
        private static readonly float[] ApprovedWeights = { 0.5f, 0.35f, 0.15f };

        [TestCase(0f, 1)]
        [TestCase(0.49f, 1)]
        [TestCase(0.5f, 2)]
        [TestCase(0.84f, 2)]
        [TestCase(0.85f, 3)]
        [TestCase(0.999f, 3)]
        public void Roll_ApprovedWeights_MapsRandomPointToChoiceCount(float point, int expected)
        {
            // DECISION-0093: 50% one choice, 35% two, 15% three.
            Assert.AreEqual(expected, new BookUpgradeCount(ApprovedWeights).Roll(new FixedDraftRandom(point)));
        }

        [Test]
        public void Roll_SeededRandom_MatchesApprovedDistribution()
        {
            var count = new BookUpgradeCount(ApprovedWeights);
            var random = new SeededDraftRandom(20260929);
            var hits = new int[4];
            const int samples = 20000;
            for (var i = 0; i < samples; i++) hits[count.Roll(random)]++;
            Assert.AreEqual(0.50, hits[1] / (double)samples, 0.02);
            Assert.AreEqual(0.35, hits[2] / (double)samples, 0.02);
            Assert.AreEqual(0.15, hits[3] / (double)samples, 0.02);
        }

        [Test]
        public void Single_AlwaysOneChoice_WithoutConsumingRandom()
        {
            var random = new SequenceDraftRandom(0.9f);
            Assert.AreEqual(1, BookUpgradeCount.Single.Roll(random));
            Assert.AreEqual(0.9f, random.NextFloat01(), "The neutral rule must not shift the draft random sequence.");
        }

        [Test]
        public void Constructor_InvalidWeights_Throws()
        {
            Assert.Catch<ArgumentNullException>(() => new BookUpgradeCount(null));
            Assert.Catch<ArgumentException>(() => new BookUpgradeCount(new float[0]));
            Assert.Catch<ArgumentException>(() => new BookUpgradeCount(new[] { 0.5f, -0.1f }));
            Assert.Catch<ArgumentException>(() => new BookUpgradeCount(new[] { 0f, 0f }));
        }
    }
}
