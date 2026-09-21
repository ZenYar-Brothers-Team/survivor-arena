using System;
using Game.Content.Json;
using NUnit.Framework;
using UnityEngine;
namespace Game.Pickup.Tests
{
    public sealed class PickupLifeTests
    {
        private static PickupLife Make(float? lifetime = null) => new PickupLife(new PickupDefinition("FIXTURE-POTION",
            PickupRewardKind.Potion, 20, .2f, lifetime, "+", Color.green, .1f), new PickupIdentity(Guid.NewGuid(), Guid.NewGuid(), 0));
        [Test]
        public void Collect_ReentrantAndDuplicate_AwardsOnce()
        {
            var life = Make(); var target = new FakePickupTarget();
            target.Applying = () => Assert.IsFalse(life.TryCollect(target).Accepted);
            Assert.IsTrue(life.TryCollect(target).Accepted);
            Assert.IsFalse(life.TryCollect(target).Accepted);
            Assert.AreEqual(1, target.Attempts); Assert.AreEqual(PickupLifeState.Collected, life.State);
        }
        [Test]
        public void Collect_Rejected_RemainsAvailableWithSameIdentity()
        {
            var life = Make(); var id = life.Identity.DropId; var target = new FakePickupTarget { Accept = false };
            Assert.IsFalse(life.TryCollect(target).Accepted); Assert.AreEqual(PickupLifeState.Available, life.State);
            target.Accept = true; Assert.IsTrue(life.TryCollect(target).Accepted); Assert.AreEqual(id, life.Identity.DropId);
        }
        [Test]
        public void Collect_CallbackThrows_DoesNotRetryUncertainReward()
        {
            var life = Make(); var target = new FakePickupTarget { Applying = () => throw new InvalidOperationException() };
            Assert.Throws<InvalidOperationException>(() => life.TryCollect(target));
            Assert.IsFalse(life.TryCollect(target).Accepted); Assert.AreEqual(1, target.Attempts);
        }
        [Test]
        public void Lifetime_PauseFreezes_ExactExpiryPrecedesCollect()
        {
            var life = Make(2); Assert.IsFalse(life.Tick(1, true)); Assert.IsFalse(life.Tick(50, false));
            Assert.AreEqual(1, life.Elapsed); Assert.IsTrue(life.Tick(1, true));
            Assert.IsFalse(life.TryCollect(new FakePickupTarget()).Accepted);
        }
        [Test]
        public void Lifetime_AbsentAndCancelled_NoExpiryOrReward()
        {
            var life = Make(); Assert.IsFalse(life.Tick(100000, true)); life.Cancel();
            Assert.IsFalse(life.TryCollect(new FakePickupTarget()).Accepted);
        }
        [TestCase(0, 1.6f, 0)]
        [TestCase(.05f, 1.6f, .08f)]
        [TestCase(.8f, 1.6f, 1)]
        public void Chance_RelativeBonusAndCap_MatchApprovedFormula(float basis, float multiplier, float expected) =>
            Assert.AreEqual(expected, PotionDropPolicy.Chance(basis, null, null, multiplier), .00001f);
        [Test]
        public void Chance_ExplicitZeroEnemyBeatsField_FieldBeatsGlobal()
        {
            Assert.AreEqual(0, PotionDropPolicy.Chance(.9f, .5f, 0, 2));
            Assert.AreEqual(.5f, PotionDropPolicy.Chance(.9f, .5f, null, 1));
            Assert.IsFalse(PotionDropPolicy.Roll(0, 0)); Assert.IsTrue(PotionDropPolicy.Roll(1, .999999));
        }
        [Test]
        public void Config_MissingTuningAndWrongKinds_Reject()
        {
            var json = JsonContentFile.ReadText("Content/Pickups/FixturePickups");
            Assert.Throws<ArgumentException>(() => FixturePickupCatalog.FromJson(json.Replace("\"baseChance\": 0.08,", "")));
            Assert.Throws<ArgumentException>(() => FixturePickupCatalog.FromJson(json.Replace("\"healing\": 20,", "")));
            Assert.Throws<ArgumentException>(() => FixturePickupCatalog.FromJson(json.Replace("\"potionId\": \"FIXTURE-POTION\"", "\"potionId\": \"FIXTURE-BOOK\"")));
        }
        [TestCase(-1)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Chance_InvalidMultiplier_Rejects(float value) => Assert.Throws<ArgumentOutOfRangeException>(() =>
            PotionDropPolicy.Chance(.5f, null, null, value));
    }
}
