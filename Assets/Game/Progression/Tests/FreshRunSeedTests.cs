using System.Collections.Generic;
using NUnit.Framework;

namespace Game.Progression.Tests
{
    /// <summary>DECISION-0057 (playtest 2026-09-25_5233a664 OBS-02): each run draws its own draft seed.</summary>
    public sealed class FreshRunSeedTests
    {
        [Test]
        public void Next_ProducesDistinctSeedsAcrossRuns()
        {
            var seeds = new HashSet<int>();
            for (var i = 0; i < 32; i++)
                seeds.Add(FreshRunSeed.Next());

            Assert.Greater(seeds.Count, 30, "Consecutive runs must not share one draft seed.");
        }

        [Test]
        public void DifferentSeeds_GiveDifferentDraftSequences()
        {
            var first = new SeededDraftRandom(230923);
            var second = new SeededDraftRandom(230924);
            var differs = false;
            for (var i = 0; i < 16 && !differs; i++)
                differs = first.NextInt(1000) != second.NextInt(1000);

            Assert.IsTrue(differs);
        }
    }
}
