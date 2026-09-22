using System.Linq;
using Game.Character;
using Game.Combat;
using Game.Content;
using Game.Progression;
using NUnit.Framework;
using UnityEngine;
namespace Game.ActiveSkill.Tests
{
    public sealed class SetCombatIntegrationTests
    {
        [Test]
        public void SkillTransform_AddsBonusesAndChangesSkillCooldown_ButSetAttackUsesOnlyGenericDamageAndKnockback()
        {
            using var context = new SkillFrameworkTestContext();
            context.Player.Stats.SetModifier("global", new CharacterStatModifier(activeSkillDamageMultiplierBonus: .2f,
                effectSizeMultiplierBonus: .5f, effectRangeMultiplierBonus: .5f, actionSpeedBonus: 1, outgoingKnockbackBonus: .2f));
            var level = new ActiveSkillLevelDefinition(10, 6, ActiveSkillTargetingMode.Self,
                new ActiveSkillActivationWave(0, 0, 1, new AreaEffect(1)));
            var definition = new ActiveSkillProgressionDefinition("FIXTURE-SKILL", "Skill", Enumerable.Repeat(level, 6).ToArray());
            var recorder = new SkillActivationRecorder();
            var skill = new ActiveSkillInstance(definition);
            var modifier = new CharacterStatModifier(activeSkillDamageMultiplierBonus: .3f, effectSizeMultiplierBonus: .2f,
                effectRangeMultiplierBonus: .3f, actionSpeedBonus: 1, outgoingKnockbackBonus: .1f);
            Assert.IsTrue(skill.Tick(0, true, context.Player, new SceneEnemyTargetProvider(), recorder, skillModifier: modifier));
            Assert.AreEqual(15, recorder.Activations[0].Damage, .0001f);
            Assert.AreEqual(1.7f, recorder.Activations[0].SizeMultiplier, .0001f);
            Assert.AreEqual(1.8f, recorder.Activations[0].RangeMultiplier, .0001f);
            Assert.IsFalse(skill.Tick(1.9f, true, context.Player, new SceneEnemyTargetProvider(), recorder, skillModifier: modifier));
            Assert.IsTrue(skill.Tick(.11f, true, context.Player, new SceneEnemyTargetProvider(), recorder, skillModifier: modifier));
            var source = new CombatSource(context.Player.Identity, new ContentId("FIXTURE-SET"), CombatSourceOrigin.Set);
            new ActiveSkillInstance(definition).Tick(0, true, context.Player, new SceneEnemyTargetProvider(), recorder, sourceOverride: source, forceActivation: true);
            var attack = recorder.Activations.Last();
            Assert.AreEqual(12, attack.Damage, .0001f); Assert.AreEqual(1, attack.SizeMultiplier); Assert.AreEqual(1, attack.RangeMultiplier);
            Assert.AreEqual(1.2f, attack.OutgoingKnockbackMultiplier, .0001f);
            Assert.AreEqual(CombatSourceOrigin.Set, attack.Source.Origin); Assert.IsNull(attack.Source.SkillLevel);
        }
        [Test]
        public void SetSource_SurvivesDelayedMultiwaveProjectiles_AndExecutorClearRemovesPendingWaves()
        {
            using var context = new SkillFrameworkTestContext();
            var launcher = new CombatRecordingLauncher();
            using var executor = new SceneActiveSkillEffectExecutor(context.Run, launcher);
            var wave = new ActiveSkillActivationWave(.5f, 0, 1, new ProjectileBurstEffect(1, ProjectileLayout.Single, 0, 0, 8, 2, .2f));
            var level = new ActiveSkillLevelDefinition(10, 6, ActiveSkillTargetingMode.Self, wave, wave);
            var source = new CombatSource(context.Player.Identity, new ContentId("FIXTURE-SET"), CombatSourceOrigin.Set);
            executor.Schedule(new ActiveSkillActivation("FIXTURE-TEMPLATE", 1, Vector2.zero, Vector2.right, null, 10, level,
                context.Owner.transform, sourceOverride: source));
            executor.Tick(1, false); Assert.AreEqual(0, launcher.Projectiles.Count);
            executor.Tick(.5f, true); Assert.AreEqual(2, launcher.Projectiles.Count);
            foreach (var shot in launcher.Projectiles)
            {
                Assert.AreEqual(source.ContentId, shot.Damage.Combat.Source.ContentId);
                Assert.AreEqual(CombatSourceOrigin.Set, shot.Damage.Combat.Source.Origin);
            }
            executor.Schedule(new ActiveSkillActivation("FIXTURE-TEMPLATE", 1, Vector2.zero, Vector2.right, null, 10, level,
                context.Owner.transform, sourceOverride: source));
            executor.Clear(); executor.Tick(10, true); Assert.AreEqual(2, launcher.Projectiles.Count);
        }
        [Test]
        public void Host_SharedSkillModifiersAreAdditive_AndDisposeRestoresSkillBinding()
        {
            using var context = new SkillFrameworkTestContext();
            var skills = context.Owner.AddComponent<PlayerActiveSkillSetRuntime>();
            var definitions = FixtureActiveSkillCatalog.Create();
            var id = definitions[0].Id;
            using var host = new SetEffectHost(context.Player, context.Run, skills, new ExperienceProgression(10), definitions);
            host.SetSkillModifier("one", id, new CharacterStatModifier(activeSkillDamageMultiplierBonus: .2f));
            host.SetSkillModifier("two", id, new CharacterStatModifier(activeSkillDamageMultiplierBonus: .3f));
            Assert.AreEqual(.5f, host.GetSkillModifier(id).ActiveSkillDamageMultiplierBonus, .0001f);
            host.RemoveSkillModifier("one"); Assert.AreEqual(.3f, host.GetSkillModifier(id).ActiveSkillDamageMultiplierBonus, .0001f);
            host.Dispose(); Assert.IsNull(skills.SetSkillModifier);
        }
    }
}
