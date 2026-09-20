using System.Collections.Generic;
using Game.Content;
using NUnit.Framework;

namespace Game.Progression.Tests
{
    public sealed class DraftBackfillTests
    {
        [TestCase(0f, "FIXTURE-SET-A")]
        [TestCase(0.49f, "FIXTURE-SET-A")]
        [TestCase(0.5f, "FIXTURE-SET-B")]
        [TestCase(0.99f, "FIXTURE-SET-B")]
        public void OneEmptySlot_TwoFailedSets_UseEqualHalfIntervals(float roll, string expected)
        {
            var active = new BuildEntryDefinition("FIXTURE-ACTIVE", BuildEntryKind.ActiveSkill, "Skill");
            var passive = new BuildEntryDefinition("FIXTURE-PASSIVE", BuildEntryKind.PassiveItem, "Passive");
            var a = Set("FIXTURE-SET-A", active);
            var b = Set("FIXTURE-SET-B", active);
            var pool = new DraftPool(new BuildEntryDefinition[] { active, passive, a, b }, setOffers: new RejectingSetOfferProvider());
            var result = pool.CreateOptions(new PlayerBuild(active), 3, new FixedDraftRandom(roll));
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(active.Id, result[0].Definition.Id);
            Assert.AreEqual(passive.Id, result[1].Definition.Id);
            Assert.AreEqual(new ContentId(expected), result[2].Definition.Id);
        }

        [Test]
        public void OnlySetsFailedChance_FillWithoutDuplicates_AndRespectBanishAndAcquired()
        {
            var active = new BuildEntryDefinition("FIXTURE-ACTIVE", BuildEntryKind.ActiveSkill, "Skill");
            var a = Set("FIXTURE-SET-A", active);
            var b = Set("FIXTURE-SET-B", active);
            var build = new PlayerBuild(active);
            for (var i = 1; i < 6; i++) build.Apply(active);
            var pool = new DraftPool(new BuildEntryDefinition[] { active, a, b });
            var options = pool.CreateOptions(build, 3, new FixedDraftRandom(0.9f));
            Assert.AreEqual(2, options.Count);
            Assert.AreNotEqual(options[0].Definition.Id, options[1].Definition.Id);
            build.Apply(a);
            options = pool.CreateOptions(build, 3, new FixedDraftRandom(0.9f), new HashSet<ContentId> { b.Id });
            Assert.AreEqual(0, options.Count);
        }

        [Test]
        public void FullOrdinaryPool_FailedSetsDoNotDisplaceItsCards()
        {
            var active = new BuildEntryDefinition("FIXTURE-ACTIVE", BuildEntryKind.ActiveSkill, "Skill");
            var pool = new DraftPool(new BuildEntryDefinition[] { active,
                new BuildEntryDefinition("FIXTURE-P1", BuildEntryKind.PassiveItem, "P1"),
                new BuildEntryDefinition("FIXTURE-P2", BuildEntryKind.PassiveItem, "P2"), Set("FIXTURE-SET", active) });
            var options = pool.CreateOptions(new PlayerBuild(active), 3, new FixedDraftRandom(0.9f));
            Assert.AreEqual(3, options.Count);
            foreach (var option in options) Assert.AreNotEqual(BuildEntryKind.Set, option.Definition.Kind);
        }

        private static SetDefinition Set(string id, BuildEntryDefinition active) => new SetDefinition(
            id, id, 0f, new SetRecipeComponent(active.Id, BuildEntryKind.ActiveSkill, 1));
    }
}
