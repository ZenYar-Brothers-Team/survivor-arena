using System.Linq;
using Game.Character;
using NUnit.Framework;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    public sealed class SkillTargetingTests
    {
        [Test]
        public void Movement_UsesConfiguredInitialThenCurrentAndLastNonzeroDirection()
        {
            using var context = new SkillFrameworkTestContext();
            var definition = FixtureActiveSkillCatalog.Create().Single(s => s.Id.ToString() == "FIXTURE-SKILL-MOVEMENT");
            var skill = new ActiveSkillInstance(definition);
            var recorder = new SkillActivationRecorder();
            var targets = new SkillTestTargetProvider(new SkillTestTarget(Vector2.left));
            Assert.IsTrue(skill.Tick(0f, true, context.Player, targets, recorder));
            Assert.Less(Vector2.Distance(Vector2.up, recorder.Activations[0].AimDirection), .0001f);
            skill.Tick(2f, true, context.Player, targets, recorder, Vector2.down);
            skill.Tick(2f, true, context.Player, targets, recorder);
            Assert.AreEqual(Vector2.down, recorder.Activations[1].AimDirection);
            Assert.AreEqual(Vector2.down, recorder.Activations[2].AimDirection);
            Assert.IsFalse(skill.Tick(20f, false, context.Player, targets, recorder, Vector2.right));
        }

        [Test]
        public void RandomWorldTargets_WithoutScreenLimit_AreSeededUniformWithinRadius()
        {
            using var context = new SkillFrameworkTestContext();
            var definition = FixtureActiveSkillCatalog.Create().Single(s => s.Id.ToString() == "FIXTURE-SKILL-WORLD-TARGET");
            var targets = new[] { new SkillTestTarget(new Vector2(20, 0)), new SkillTestTarget(new Vector2(35, 0)),
                new SkillTestTarget(new Vector2(45, 0)), new SkillTestTarget(new Vector2(60, 0)), new SkillTestTarget(Vector2.zero) { IsAlive = false } };
            var provider = new SkillTestTargetProvider(targets);
            var first = new ActiveSkillInstance(definition);
            var second = new ActiveSkillInstance(definition);
            var a = new SkillActivationRecorder();
            var b = new SkillActivationRecorder();
            for (var i = 0; i < 600; i++)
            {
                first.Tick(2f, true, context.Player, provider, a);
                second.Tick(2f, true, context.Player, provider, b);
                Assert.AreSame(a.Activations[i].InitialTarget, b.Activations[i].InitialTarget);
            }
            for (var i = 0; i < 3; i++) Assert.That(a.Activations.Count(c => ReferenceEquals(c.InitialTarget, targets[i])), Is.InRange(160, 240));
            Assert.IsFalse(a.Activations.Any(c => ReferenceEquals(c.InitialTarget, targets[3]) || ReferenceEquals(c.InitialTarget, targets[4])));
            Assert.IsFalse(first.Tick(2f, true, context.Player, new SkillTestTargetProvider(), a));
        }

        [Test]
        public void Cast_CapturesSizeRangeDamageAndAddsLevelActionSpeedWithoutChangingWaveTiming()
        {
            using var context = new SkillFrameworkTestContext();
            var definition = FixtureActiveSkillCatalog.Create().Single(s => s.Id.ToString() == "FIXTURE-SKILL-MOVEMENT");
            var skill = new ActiveSkillInstance(definition);
            skill.SetLevel(6);
            context.Player.SetModifier("IP08", new CharacterStatModifier(actionSpeedBonus: .25f, effectSizeMultiplierBonus: .5f, effectRangeMultiplierBonus: 1f));
            var recorder = new SkillActivationRecorder();
            var targets = new SkillTestTargetProvider();
            skill.Tick(0f, true, context.Player, targets, recorder);
            Assert.AreEqual(1.5f, recorder.Activations[0].SizeMultiplier);
            Assert.AreEqual(2f, recorder.Activations[0].RangeMultiplier);
            Assert.IsFalse(skill.Tick(1.32f, true, context.Player, targets, recorder));
            Assert.IsTrue(skill.Tick(.02f, true, context.Player, targets, recorder));
            context.Player.SetModifier("IP08", new CharacterStatModifier());
            Assert.AreEqual(1.5f, recorder.Activations[0].SizeMultiplier, "Previously launched effects keep their stat snapshot.");
            Assert.AreEqual(2f, definition.GetLevel(6).CooldownSeconds);
        }
    }
}
