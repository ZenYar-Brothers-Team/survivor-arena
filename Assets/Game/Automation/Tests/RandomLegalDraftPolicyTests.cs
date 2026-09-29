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
    }
}
