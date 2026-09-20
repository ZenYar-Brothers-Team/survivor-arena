using System;
using Game.Content;
using NUnit.Framework;

namespace Game.Combat.Tests
{
    public sealed class CombatControlTests
    {
        [Test]
        public void Knockback_ResolvesDistanceAndFinalFraction_WithoutSlowScalingIt()
        {
            var state = new CombatControlState();
            var request = new CombatDamageRequest(default, 0f, new CombatControlProfile(2f, 0.15f, 0.2f, 1f), 3f, 4f, 1.25f);
            Assert.AreEqual(1.5f, state.Apply(request, 0.4f, true), 0.0001f);
            var a = state.Tick(0.1f, true);
            var paused = state.Tick(5f, false);
            Assert.AreEqual(0f, paused.KnockbackX);
            Assert.AreEqual(0.05f, state.KnockbackRemaining, 0.0001f);
            var b = state.Tick(0.1f, true);
            Assert.AreEqual(0.9f, (a.KnockbackX + b.KnockbackX) * 0.1f, 0.0001f);
            Assert.AreEqual(1.2f, (a.KnockbackY + b.KnockbackY) * 0.1f, 0.0001f);
            Assert.AreEqual(0.8f, a.MovementMultiplier, 0.0001f);
            Assert.AreEqual(0f, state.Tick(1f, true).KnockbackX);
        }

        [Test]
        public void Knockback_NewImpulseReplacesResidual_ImmunityAndZeroDirectionDoNotCancelIt()
        {
            var state = new CombatControlState();
            var first = new CombatDamageRequest(default, 0f, new CombatControlProfile(2f, 1f), 1f, 0f);
            state.Apply(first, 0f, false);
            state.Tick(0.5f, true);
            var next = new CombatDamageRequest(default, 0f, new CombatControlProfile(1f, 1f), 0f, 1f);
            state.Apply(next, 0f, false);
            Assert.AreEqual(0f, state.Apply(first, 1f, false));
            Assert.AreEqual(0f, state.Apply(first.WithDirection(0f, 0f), 0f, false));
            var motion = state.Tick(1f, true);
            Assert.AreEqual(0f, motion.KnockbackX);
            Assert.AreEqual(1f, motion.KnockbackY);
            state.Reset();
            Assert.AreEqual(0f, state.KnockbackRemaining);
            Assert.AreEqual(1f, state.MovementMultiplier);
        }

        [Test]
        public void Slow_SourceRefreshReplacesMagnitude_WeakerSourceSurvivesAndExpiresIndependently()
        {
            var state = new CombatControlState();
            var owner = new CombatIdentity(Guid.NewGuid(), null, null, CombatEntityCategory.Player);
            CombatDamageRequest Slow(string id, float fraction, float duration) => new CombatDamageRequest(
                new CombatSource(owner, new ContentId(id), CombatSourceOrigin.ActiveSkill), 0f,
                new CombatControlProfile(slowFraction: fraction, slowSeconds: duration));
            state.Apply(Slow("FIXTURE-A", 0.6f, 1f), 0f, true);
            state.Apply(Slow("FIXTURE-B", 0.2f, 3f), 0f, true);
            Assert.AreEqual(0.4f, state.MovementMultiplier, 0.0001f);
            state.Tick(0.5f, true);
            state.Apply(Slow("FIXTURE-A", 0.4f, 1f), 0f, true);
            Assert.AreEqual(2, state.SlowSourceCount);
            Assert.AreEqual(0.6f, state.MovementMultiplier, 0.0001f);
            state.Tick(0.75f, true);
            Assert.AreEqual(0.6f, state.MovementMultiplier, 0.0001f);
            state.Tick(0.25f, true);
            Assert.AreEqual(0.8f, state.MovementMultiplier, 0.0001f);
            state.Tick(1.5f, true);
            Assert.AreEqual(1f, state.MovementMultiplier);
            Assert.AreEqual(0, state.SlowSourceCount);
            state.Apply(Slow("FIXTURE-B", 1f, 3f), 0f, false);
            Assert.AreEqual(1f, state.MovementMultiplier, "Player does not accept movement-only enemy slow.");
        }

        [Test]
        public void ControlAuthoring_RequiresPositiveDurationWhenEnabled_RejectsNonfiniteValues()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CombatControlData { KnockbackDistance = 1f }.ToProfile());
            Assert.Throws<ArgumentOutOfRangeException>(() => new CombatControlData { SlowFraction = 0.2f }.ToProfile());
            Assert.Throws<ArgumentOutOfRangeException>(() => new CombatControlProfile(slowFraction: 1.1f, slowSeconds: 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CombatDamageRequest(default, 0f, directionX: float.NaN));
        }
    }
}
