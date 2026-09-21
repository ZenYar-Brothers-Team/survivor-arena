using System;
using System.Collections.Generic;
using Game.Content;
using NUnit.Framework;

namespace Game.Progression.Tests
{
    public sealed class SetFrameworkTests
    {
        [Test]
        public void Recipe_RequiresEveryComponentAtItsMinimumLevel()
        {
            var active = Active("FIXTURE-ACTIVE");
            var passive = Passive("FIXTURE-PASSIVE");
            var set = Set("FIXTURE-SET",
                Component(active, 2),
                Component(passive, 3));
            var build = SetTestData.Build(active);

            Assert.IsFalse(build.IsEligible(set));
            build.Apply(active);
            build.Apply(passive);
            build.Apply(passive);
            Assert.IsFalse(build.IsEligible(set));
            build.Apply(passive);

            Assert.IsTrue(set.IsRecipeFulfilled(build));
            Assert.IsTrue(build.IsEligible(set));
        }

        [Test]
        public void FulfilledSet_UsesDraftProbabilityHook()
        {
            var active = Active("FIXTURE-ACTIVE");
            var set = Set("FIXTURE-SET", Component(active, 1));
            var build = SetTestData.Build(active);
            var pool = new DraftPool(new BuildEntryDefinition[] { active, Passive("FIXTURE-PASSIVE-A"), Passive("FIXTURE-PASSIVE-B"), set }, setOffers: new FixtureSetDraftOfferProvider(0.5f));

            var missed = pool.CreateOptions(build, 3, new FixedDraftRandom(0.75f));
            var appeared = pool.CreateOptions(build, 3, new FixedDraftRandom(0.25f));

            Assert.IsFalse(Contains(missed, set.Id));
            Assert.IsTrue(Contains(appeared, set.Id));
        }

        [Test]
        public void AcquiredSet_HasNoLevelsOrSlotCost_AndCannotBeDuplicated()
        {
            var active = Active("FIXTURE-ACTIVE");
            var set = Set("FIXTURE-SET", Component(active, 1));
            var build = SetTestData.Build(active);
            var activeCount = build.ActiveCount;
            var passiveCount = build.PassiveCount;

            var result = build.Apply(set);

            Assert.IsTrue(result.WasNewEntry);
            Assert.IsTrue(result.Entry.IsMaxLevel);
            Assert.AreEqual(1, result.Entry.Level);
            Assert.AreEqual(activeCount, build.ActiveCount);
            Assert.AreEqual(passiveCount, build.PassiveCount);
            Assert.AreEqual(1, build.SetCount);
            Assert.IsFalse(build.IsEligible(set));
            Assert.Throws<InvalidOperationException>(() => build.Apply(set));
        }

        [Test]
        public void SharedRecipeComponent_UnlocksIndependentUnlimitedSets()
        {
            var active = Active("FIXTURE-SHARED-ACTIVE");
            var first = Set("FIXTURE-SET-FIRST", Component(active, 1));
            var second = Set("FIXTURE-SET-SECOND", Component(active, 1));
            var build = SetTestData.Build(active);

            build.Apply(first);

            Assert.IsTrue(build.IsEligible(second));
            build.Apply(second);
            Assert.AreEqual(2, build.SetCount);
            Assert.AreEqual(1, build.ActiveCount);
            Assert.AreEqual(2, build.PassiveCount);
        }

        [Test]
        public void Runtime_CreatesTicksAndDisposesOneIndependentAbilityPerSet()
        {
            var active = Active("FIXTURE-ACTIVE");
            var first = Set("FIXTURE-SET-FIRST", Component(active, 1));
            var second = Set("FIXTURE-SET-SECOND", Component(active, 1));
            var build = SetTestData.Build(active);
            build.Apply(first);
            build.Apply(second);
            var runtime = new PlayerSetRuntime(
                new[] { first, second },
                new FixtureSetExtraAbilityFactory());

            runtime.Synchronize(build);
            runtime.Tick(0.5f, false);
            runtime.Tick(0.5f, true);

            Assert.AreEqual(2, runtime.Count);
            Assert.IsTrue(runtime.TryGetAbility(first.Id, out var firstRaw));
            Assert.IsTrue(runtime.TryGetAbility(second.Id, out var secondRaw));
            var firstAbility = (FixtureSetExtraAbility)firstRaw;
            var secondAbility = (FixtureSetExtraAbility)secondRaw;
            Assert.AreEqual(1, firstAbility.RunningTickCount);
            Assert.AreEqual(1, secondAbility.RunningTickCount);
            Assert.AreEqual(0.5f, firstAbility.RunningSeconds);

            runtime.Dispose();

            Assert.IsTrue(firstAbility.IsDisposed);
            Assert.IsTrue(secondAbility.IsDisposed);
        }

        [Test]
        public void ContentRegistry_ValidatesSetRecipeReferences()
        {
            var active = Active("FIXTURE-ACTIVE");
            var valid = Set("FIXTURE-SET-VALID", Component(active, 1));
            var missing = Set(
                "FIXTURE-SET-MISSING",
                new SetRecipeComponent("FIXTURE-NOT-REGISTERED", BuildEntryKind.ActiveSkill, 1));

            Assert.DoesNotThrow(() => ContentRegistry.BuildFrom(new IContentDefinition[] { active, valid,
                new BuildEntryDefinition("FIXTURE-RECIPE-0", BuildEntryKind.PassiveItem, "R0"),
                new BuildEntryDefinition("FIXTURE-RECIPE-1", BuildEntryKind.PassiveItem, "R1") }));
            Assert.Throws<ContentValidationException>(() =>
                ContentRegistry.BuildFrom(new IContentDefinition[] { active, missing }));
        }

        private static bool Contains(IReadOnlyList<DraftOption> options, ContentId id)
        {
            for (var i = 0; i < options.Count; i++)
            {
                if (options[i].Definition.Id == id)
                    return true;
            }
            return false;
        }

        private static BuildEntryDefinition Active(string id)
        {
            return new BuildEntryDefinition(id, BuildEntryKind.ActiveSkill, id);
        }

        private static BuildEntryDefinition Passive(string id)
        {
            return new BuildEntryDefinition(id, BuildEntryKind.PassiveItem, id);
        }

        private static SetRecipeComponent Component(BuildEntryDefinition definition, int minimumLevel)
        {
            return new SetRecipeComponent(definition.Id, definition.Kind, minimumLevel);
        }

        private static SetDefinition Set(string id, params SetRecipeComponent[] recipe)
        {
            return SetTestData.Define(id, id, recipe);
        }
    }
}
