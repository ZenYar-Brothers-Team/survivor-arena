using System.Collections.Generic;
using NUnit.Framework;

namespace Game.Progression.Tests
{
    public sealed class DraftActiveSkillGuaranteeTests
    {
        private static BuildEntryDefinition Active(string id) => new BuildEntryDefinition(id, BuildEntryKind.ActiveSkill, id);
        private static BuildEntryDefinition Passive(string id) => new BuildEntryDefinition(id, BuildEntryKind.PassiveItem, id);

        private static int CountActive(IReadOnlyList<DraftOption> options)
        {
            var count = 0;
            foreach (var option in options)
                if (option.Definition.Kind == BuildEntryKind.ActiveSkill) count++;
            return count;
        }

        [TestCase(0f)]
        [TestCase(0.5f)]
        [TestCase(0.99f)]
        public void MinimumTwo_OffersAtLeastTwoActiveSkills_WhenAvailable(float roll)
        {
            var start = Active("FIXTURE-START");
            var pool = new DraftPool(new[] { start, Active("FIXTURE-A1"), Active("FIXTURE-A2"),
                Passive("FIXTURE-P1"), Passive("FIXTURE-P2"), Passive("FIXTURE-P3"), Passive("FIXTURE-P4") });
            var options = pool.CreateOptions(SetTestData.Build(start), 3, new FixedDraftRandom(roll), null, null, 2);
            Assert.AreEqual(3, options.Count);
            Assert.GreaterOrEqual(CountActive(options), 2);
        }

        [Test]
        public void MinimumTwo_WithOnlyOneActiveSkillEligible_OffersWhatExists()
        {
            var start = Active("FIXTURE-START");
            var pool = new DraftPool(new[] { start, Passive("FIXTURE-P1"), Passive("FIXTURE-P2"), Passive("FIXTURE-P3") });
            var options = pool.CreateOptions(SetTestData.Build(start), 3, new FixedDraftRandom(0.9f), null, null, 2);
            Assert.AreEqual(3, options.Count);
            Assert.AreEqual(1, CountActive(options));
        }

        [Test]
        public void NoMinimum_KeepsOrdinaryWeightedPick()
        {
            var start = Active("FIXTURE-START");
            var pool = new DraftPool(new[] { start, Active("FIXTURE-A1"),
                Passive("FIXTURE-P1"), Passive("FIXTURE-P2"), Passive("FIXTURE-P3") });
            var with = pool.CreateOptions(SetTestData.Build(start), 3, new FixedDraftRandom(0.9f));
            var without = pool.CreateOptions(SetTestData.Build(start), 3, new FixedDraftRandom(0.9f), null, null, 0);
            for (var i = 0; i < with.Count; i++) Assert.AreEqual(with[i].Definition.Id, without[i].Definition.Id);
        }
    }
}
