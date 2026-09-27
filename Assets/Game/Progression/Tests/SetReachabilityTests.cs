using Game.Content;
using NUnit.Framework;

namespace Game.Progression.Tests
{
    /// <summary>DECISION-0073: a set is «missed» once its recipe can no longer be fulfilled in the run.</summary>
    public sealed class SetReachabilityTests
    {
        [Test]
        public void MissingPassive_IsReachableWhileAPassiveSlotIsFree()
        {
            var active = Active("FIXTURE-A0");
            var build = new PlayerBuild(active);
            for (var i = 0; i < PlayerBuild.PassiveSlotCapacity - 1; i++) build.Apply(Passive("FIXTURE-FILL-" + i));
            var set = Recipe(active, Passive("FIXTURE-P1"), Passive("FIXTURE-FILL-0"));

            Assert.IsTrue(set.CanStillBeFulfilled(build, _ => true));
        }

        [Test]
        public void MissingComponents_AreUnreachableWhenSlotsOfTheirKindAreFull()
        {
            var active = Active("FIXTURE-A0");
            var build = new PlayerBuild(active);
            for (var i = 0; i < PlayerBuild.PassiveSlotCapacity; i++) build.Apply(Passive("FIXTURE-FILL-" + i));
            var set = Recipe(active, Passive("FIXTURE-P1"), Passive("FIXTURE-FILL-0"));

            Assert.IsFalse(set.CanStillBeFulfilled(build, _ => true));
        }

        [Test]
        public void TwoMissingActives_NeedTwoFreeActiveSlots()
        {
            var active = Active("FIXTURE-A0");
            var build = new PlayerBuild(active);
            for (var i = 1; i < PlayerBuild.ActiveSlotCapacity - 1; i++) build.Apply(Active("FIXTURE-A" + i));
            var set = Recipe(Active("FIXTURE-X1"), Active("FIXTURE-X2"), Passive("FIXTURE-P1"));

            Assert.AreEqual(PlayerBuild.ActiveSlotCapacity - 1, build.ActiveCount);
            Assert.IsFalse(set.CanStillBeFulfilled(build, _ => true), "One free active slot cannot hold two missing actives.");
        }

        [Test]
        public void OwnedComponentBelowThreshold_StaysReachableEvenWithFullSlots()
        {
            var active = Active("FIXTURE-A0");
            var build = new PlayerBuild(active);
            for (var i = 0; i < PlayerBuild.PassiveSlotCapacity; i++) build.Apply(Passive("FIXTURE-FILL-" + i));
            var set = new SetDefinition("FIXTURE-SET", "Set",
                new SetRecipeComponent(active.Id, active.Kind, 3),
                new SetRecipeComponent("FIXTURE-FILL-0", BuildEntryKind.PassiveItem, 2),
                new SetRecipeComponent("FIXTURE-FILL-1", BuildEntryKind.PassiveItem, 1));

            Assert.IsTrue(set.CanStillBeFulfilled(build, _ => true));
        }

        [Test]
        public void MissingComponentThatCannotBeOffered_MakesTheSetUnreachable()
        {
            var active = Active("FIXTURE-A0");
            var build = new PlayerBuild(active);
            var banished = Passive("FIXTURE-BANISHED");
            var set = Recipe(active, banished, Passive("FIXTURE-P1"));

            Assert.IsFalse(set.CanStillBeFulfilled(build, id => id != banished.Id));
            Assert.IsTrue(set.CanStillBeFulfilled(build, id => id != active.Id), "Fulfilled components are not re-offered.");
        }

        private static SetDefinition Recipe(params BuildEntryDefinition[] components)
        {
            var recipe = new SetRecipeComponent[components.Length];
            for (var i = 0; i < components.Length; i++)
                recipe[i] = new SetRecipeComponent(components[i].Id, components[i].Kind, 1);
            return new SetDefinition("FIXTURE-SET", "Set", recipe);
        }

        private static BuildEntryDefinition Active(string id) => new BuildEntryDefinition(id, BuildEntryKind.ActiveSkill, id);
        private static BuildEntryDefinition Passive(string id) => new BuildEntryDefinition(id, BuildEntryKind.PassiveItem, id);
    }
}
