using NUnit.Framework;

namespace Game.Enemy.Tests
{
    public sealed class EnemyZoneInfluenceTests
    {
        [Test]
        public void Area_ExitClearsBonuses_BurstPersistsPausesAndExpires_ReuseRestoresNeutral()
        {
            var state = new EnemyZoneInfluence(); state.SetArea(.4f, .5f, .8f); state.ApplyBurst(.6f, 8f);
            Assert.AreEqual(2f, state.MovementMultiplier); Assert.AreEqual(1.5f, state.ActionMultiplier);
            Assert.AreEqual(.8f, state.DamageReduction);
            state.SetArea(0f, 0f, 0f); Assert.AreEqual(1.6f, state.MovementMultiplier);
            state.Tick(4f, false); Assert.AreEqual(8f, state.BuffRemaining);
            state.Tick(4f, true); Assert.AreEqual(.5f, state.BuffRemaining01);
            state.ApplyBurst(.6f, 8f); Assert.AreEqual(8f, state.BuffRemaining, "Refresh does not stack the movement bonus.");
            state.Tick(8f, true); Assert.AreEqual(1f, state.MovementMultiplier);
            state.SetArea(.4f, .5f, .8f); state.ApplyBurst(.6f, 8f); state.Reset();
            Assert.AreEqual(1f, state.MovementMultiplier); Assert.AreEqual(1f, state.ActionMultiplier); Assert.AreEqual(0f, state.DamageReduction);
        }
    }
}
