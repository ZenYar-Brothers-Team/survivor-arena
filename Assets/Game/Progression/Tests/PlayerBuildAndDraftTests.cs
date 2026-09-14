using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Game.Progression.Tests
{
    public class PlayerBuildAndDraftTests
    {
        [Test]
        public void StartingActiveOccupiesSlot_AndSeparateCapsBlockNewEntries()
        {
            var starting = Active("FIXTURE-ACTIVE-0");
            var build = new PlayerBuild(starting);

            for (var i = 1; i < PlayerBuild.ActiveSlotCapacity; i++)
                build.Apply(Active($"FIXTURE-ACTIVE-{i}"));
            for (var i = 0; i < PlayerBuild.PassiveSlotCapacity; i++)
                build.Apply(Passive($"FIXTURE-PASSIVE-{i}"));

            Assert.AreEqual(6, build.ActiveCount);
            Assert.AreEqual(6, build.PassiveCount);
            Assert.IsFalse(build.IsEligible(Active("FIXTURE-ACTIVE-EXTRA")));
            Assert.IsFalse(build.IsEligible(Passive("FIXTURE-PASSIVE-EXTRA")));
            Assert.IsTrue(build.IsEligible(starting), "Existing entries remain upgradeable when slots are full.");
        }

        [Test]
        public void Upgrade_IncreasesLevelFromOneToSix_ThenBecomesIneligible()
        {
            var starting = Active("FIXTURE-ACTIVE-0");
            var build = new PlayerBuild(starting);

            for (var level = 2; level <= BuildEntryDefinition.MaxLevel; level++)
            {
                var result = build.Apply(starting);
                Assert.IsFalse(result.WasNewEntry);
                Assert.AreEqual(level, result.Entry.Level);
            }

            Assert.IsFalse(build.IsEligible(starting));
            Assert.Throws<InvalidOperationException>(() => build.Apply(starting));
        }

        [Test]
        public void UnifiedPool_FiltersFullTypesAndMaxLevelEntries()
        {
            var starting = Active("FIXTURE-ACTIVE-0");
            var passive = Passive("FIXTURE-PASSIVE-0");
            var newActive = Active("FIXTURE-ACTIVE-NEW");
            var pool = new DraftPool(new[] { starting, passive, newActive });
            var build = new PlayerBuild(starting);

            var options = pool.CreateOptions(build, 3);

            Assert.AreEqual(3, options.Count);
            Assert.IsTrue(options[0].IsUpgrade);
            Assert.AreEqual(BuildEntryKind.PassiveItem, options[1].Definition.Kind);

            for (var i = 1; i < PlayerBuild.ActiveSlotCapacity; i++)
                build.Apply(Active($"FIXTURE-FILL-ACTIVE-{i}"));
            for (var i = 2; i <= BuildEntryDefinition.MaxLevel; i++)
                build.Apply(starting);

            options = pool.CreateOptions(build, 3);

            Assert.AreEqual(1, options.Count);
            Assert.AreEqual(passive.Id, options[0].Definition.Id);
        }

        [Test]
        public void DraftSession_RejectsUnofferedOrStaleSelection()
        {
            var starting = Active("FIXTURE-ACTIVE-0");
            var offered = Passive("FIXTURE-PASSIVE-0");
            var build = new PlayerBuild(starting);
            var session = new DraftSession(
                build,
                new List<DraftOption> { new DraftOption(offered, false, 1) });

            Assert.IsFalse(session.TrySelect("FIXTURE-NOT-OFFERED", out _));
            Assert.IsTrue(session.IsOpen);
            Assert.AreEqual(0, build.PassiveCount);

            Assert.IsTrue(session.TrySelect(offered.Id, out var result));
            Assert.IsTrue(result.WasNewEntry);
            Assert.IsFalse(session.IsOpen);
            Assert.IsFalse(session.TrySelect(offered.Id, out _));
            Assert.AreEqual(1, build.PassiveCount);
        }

        [Test]
        public void Pool_RejectsDuplicateStableIds()
        {
            Assert.Throws<ArgumentException>(() => new DraftPool(new[]
            {
                Active("FIXTURE-DUPLICATE"),
                Passive("FIXTURE-DUPLICATE")
            }));
        }

        [Test]
        public void SeededSelection_IsReproducibleAndDoesNotDuplicateOptions()
        {
            var starting = Active("FIXTURE-ACTIVE-0");
            var definitions = new List<BuildEntryDefinition> { starting };
            for (var i = 1; i < 10; i++)
                definitions.Add(Passive($"FIXTURE-PASSIVE-{i}"));
            var pool = new DraftPool(definitions);
            var build = new PlayerBuild(starting);

            var first = pool.CreateOptions(build, 4, new SeededDraftRandom(12345));
            var second = pool.CreateOptions(build, 4, new SeededDraftRandom(12345));
            var ids = new HashSet<Game.Content.ContentId>();

            Assert.AreEqual(first.Count, second.Count);
            for (var i = 0; i < first.Count; i++)
            {
                Assert.AreEqual(first[i].Definition.Id, second[i].Definition.Id);
                Assert.IsTrue(ids.Add(first[i].Definition.Id));
            }
        }

        private static BuildEntryDefinition Active(string id)
        {
            return new BuildEntryDefinition(id, BuildEntryKind.ActiveSkill, id);
        }

        private static BuildEntryDefinition Passive(string id)
        {
            return new BuildEntryDefinition(id, BuildEntryKind.PassiveItem, id);
        }
    }
}
