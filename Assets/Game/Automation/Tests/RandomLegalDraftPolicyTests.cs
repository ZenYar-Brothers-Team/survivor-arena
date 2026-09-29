using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Game.Automation.Tests
{
    public sealed class RandomLegalDraftPolicyTests
    {
        [Test]
        public void Choose_EmptyAndShortOffers_NeverInventsAnId()
        {
            var policy = new RandomLegalDraftPolicy(new Random(17));
            Assert.IsNull(policy.Choose(Array.Empty<string>()));
            Assert.AreEqual("SKILL-001", policy.Choose(new[] { "SKILL-001" }));
            var offered = new List<string> { "SKILL-001", "SET-001" };
            for (var i = 0; i < 100; i++) CollectionAssert.Contains(offered, policy.Choose(offered));
        }

        [Test]
        public void Choose_SeededIndependentRandom_IsReproducibleForPolicyTests()
        {
            var first = new RandomLegalDraftPolicy(new Random(11));
            var second = new RandomLegalDraftPolicy(new Random(11));
            var offered = new[] { "SKILL-001", "PASSIVE-001", "SET-001" };
            for (var i = 0; i < 40; i++) Assert.AreEqual(first.Choose(offered), second.Choose(offered));
        }

        [Test]
        public void ChoosePreferred_OfferedActiveOptions_ChoosesOnlyActiveAndFallsBackWhenAbsent()
        {
            var policy = new RandomLegalDraftPolicy(new Random(23));
            var offered = new[] { "PASSIVE-001", "SKILL-001", "SET-001", "SKILL-002" };
            var active = new[] { "SKILL-001", "SKILL-002", "SKILL-NOT-OFFERED" };
            for (var i = 0; i < 40; i++)
                CollectionAssert.Contains(new[] { "SKILL-001", "SKILL-002" }, policy.ChoosePreferred(offered, active));
            Assert.AreEqual("PASSIVE-001", policy.ChoosePreferred(new[] { "PASSIVE-001" }, active));
        }

        [Test]
        public void ChooseActiveFirst15_LevelBoundary_ChangesToUniformChoiceAfterLevel15()
        {
            var offered = new[] { "PASSIVE-001", "SKILL-001" };
            var active = new[] { "SKILL-001" };
            var policy = new RandomLegalDraftPolicy(new Random(9));
            Assert.AreEqual("SKILL-001", policy.ChooseActiveFirst15(offered, active, 15));
            var after = new RandomLegalDraftPolicy(new Random(9));
            var uniform = new RandomLegalDraftPolicy(new Random(9));
            Assert.AreEqual(uniform.Choose(offered), after.ChooseActiveFirst15(offered, active, 16));
        }
    }
}
