using System;
using NUnit.Framework;
namespace Game.Enemy.Tests
{
    public sealed class EnemyProtectionTests
    {
        [Test]
        public void Aura_DeadlineExpiresBeforeDamage_WithoutWaitingForSourceUpdate()
        {
            var state=new EnemyProtection(); state.SetAura(Guid.NewGuid(),.5f,.5f,2);
            Assert.AreEqual(5,state.Absorb(10,1)); Assert.AreEqual(10,state.Absorb(10,2));
            Assert.AreEqual(0,state.ResistanceBonus);
        }
        [Test]
        public void Aura_MaxPerChannel_RemovesOnlyOwner_ThenShieldAbsorbsReducedDamage()
        {
            var state=new EnemyProtection(); var a=Guid.NewGuid(); var b=Guid.NewGuid();
            state.SetAura(a,.2f,.1f); state.SetAura(b,.1f,.3f);
            Assert.AreEqual(.2f,state.Reduction); Assert.AreEqual(.3f,state.ResistanceBonus);
            state.CastShield(a,5,2,0); Assert.AreEqual(3,state.Absorb(10,0),.001f);
            state.RemoveSource(a); Assert.AreEqual(.1f,state.Reduction); Assert.AreEqual(.3f,state.ResistanceBonus);
            state.Reset(); Assert.AreEqual(0,state.Reduction); Assert.AreEqual(0,state.ResistanceBonus);
        }
        [Test]
        public void Shield_EqualRefresh_StrongerReplacement_WeakerIgnored_ExpiryExact()
        {
            var state=new EnemyProtection(); var a=Guid.NewGuid(); var b=Guid.NewGuid();
            state.CastShield(a,20,2,0); state.Absorb(10,0);
            Assert.IsFalse(state.CastShield(b,15,4,1)); Assert.AreEqual(10,state.ShieldRemaining);
            Assert.IsTrue(state.CastShield(b,20,4,1)); Assert.AreEqual(20,state.ShieldRemaining);
            state.RemoveSource(a); Assert.AreEqual(20,state.ShieldRemaining); Assert.AreEqual(b,state.ShieldSource);
            Assert.AreEqual(10,state.Absorb(10,5)); Assert.AreEqual(0,state.ShieldRemaining);
        }
    }
}
