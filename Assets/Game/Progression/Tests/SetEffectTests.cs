using System;
using Game.Character;
using Game.Content;
using Game.Combat;
using NUnit.Framework;
namespace Game.Progression.Tests
{
    public sealed class SetEffectTests
    {
        private static SetDefinition Define(string id, params SetEffectDefinition[] effects) => new SetDefinition(id, id, "Fixture", effects,
            new SetRecipeComponent("FIXTURE-SKILL", BuildEntryKind.ActiveSkill, 1),
            new SetRecipeComponent("FIXTURE-P1", BuildEntryKind.PassiveItem, 2),
            new SetRecipeComponent("FIXTURE-P2", BuildEntryKind.PassiveItem, 1));
        [Test]
        public void SharedBuffs_ComposeByKey_AndShutdownRemovesOnlyOwnedEffects()
        {
            var host = new FakeSetEffectHost();
            host.Stats.SetModifier("external", new CharacterStatModifier(activeSkillDamageMultiplierBonus: 0.1f));
            var buff = new SetEffectDefinition(SetEffectKind.StatBuff, new CharacterStatModifier(activeSkillDamageMultiplierBonus: 0.2f));
            using var a = new SetEffectAbility(Define("FIXTURE-A", buff), host);
            using var b = new SetEffectAbility(Define("FIXTURE-B", buff), host);
            Assert.AreEqual(1.5f, host.Stats.ActiveSkillDamageMultiplier, 0.0001f);
            a.Dispose();
            Assert.AreEqual(1.3f, host.Stats.ActiveSkillDamageMultiplier, 0.0001f);
            b.Dispose();
            Assert.AreEqual(1, host.Stats.ModifierCount);
        }
        [Test]
        public void Counters_RejectSetAndSecondarySources_AndDoNotCountMultiwaveEffects()
        {
            var host = new FakeSetEffectHost();
            var proc = new SetEffectDefinition(SetEffectKind.ActivationProc, skill: new ContentId("FIXTURE-SKILL"),
                attackTemplate: new ContentId("FIXTURE-ATTACK"), cooldownSeconds: 2, activationCount: 2);
            using var a = new SetEffectAbility(Define("FIXTURE-A", proc), host);
            using var b = new SetEffectAbility(Define("FIXTURE-B", proc), host);
            host.Activate(); host.Activate(CombatSourceOrigin.Set); host.Activate(CombatSourceOrigin.SecondaryProc);
            Assert.AreEqual(0, host.Attacks);
            host.Activate();
            Assert.AreEqual(2, host.Attacks, "Each ordinary activation reaches each counter once; set attacks cannot recurse.");
            Assert.AreEqual(2, a.OrdinaryActivationCount);
            Assert.AreEqual(1, a.ProcCount);
            a.Dispose(); b.Dispose(); host.Activate(); host.Activate();
            Assert.AreEqual(2, host.Attacks);
        }
        [Test]
        public void RewardProc_CooldownAndTemporaryBuff_FreezeAndExpireWithoutRemovingPassive()
        {
            var host = new FakeSetEffectHost();
            host.Stats.SetModifier("passive", new CharacterStatModifier(incomingDamageReductionBonus: 0.1f));
            using var ability = new SetEffectAbility(Define("FIXTURE-REWARD",
                new SetEffectDefinition(SetEffectKind.RewardProc, new CharacterStatModifier(incomingDamageReductionBonus: 0.2f),
                    attackTemplate: new ContentId("FIXTURE-ATTACK"), cooldownSeconds: 5, buffSeconds: 2)), host);
            host.Reward(CombatSourceOrigin.Set); Assert.AreEqual(0, host.Attacks);
            host.Reward(); host.Reward(); Assert.AreEqual(1, host.Attacks);
            Assert.AreEqual(0.7f, host.Stats.IncomingDamageMultiplier, 0.0001f);
            host.IsRunning = false; ability.Tick(10, false); host.Reward(); Assert.AreEqual(1, host.Attacks);
            host.IsRunning = true; ability.Tick(2, true);
            Assert.AreEqual(0.9f, host.Stats.IncomingDamageMultiplier, 0.0001f);
            host.Reward(); Assert.AreEqual(1, host.Attacks);
            ability.Tick(3, true); host.Reward(); Assert.AreEqual(2, host.Attacks);
            ability.Dispose(); host.Reward(); Assert.AreEqual(2, host.Attacks);
            Assert.AreEqual(1, host.Stats.ModifierCount);
        }
        [Test]
        public void IndependentAttack_FixedTimerIgnoresActionSpeed_AndTerminalClearsIt()
        {
            var host = new FakeSetEffectHost();
            host.Stats.SetModifier("haste", new CharacterStatModifier(actionSpeedBonus: 10));
            using var ability = new SetEffectAbility(Define("FIXTURE-TIMER", new SetEffectDefinition(SetEffectKind.IndependentAttack,
                attackTemplate: new ContentId("FIXTURE-ATTACK"), cooldownSeconds: 6)), host);
            ability.Tick(5, true); Assert.AreEqual(0, host.Attacks);
            ability.Tick(20, false); Assert.AreEqual(0, host.Attacks);
            ability.Tick(1, true); Assert.AreEqual(1, host.Attacks);
            ability.Tick(6, true); Assert.AreEqual(2, host.Attacks);
            host.IsTerminal = true; ability.Tick(6, false); Assert.Greater(host.Clears, 0);
            host.IsTerminal = false; ability.Tick(60, true); Assert.AreEqual(2, host.Attacks);
        }
        [Test]
        public void LevelHeal_WorksDuringDraftPause_ButNotAfterTerminalOrDispose()
        {
            var host = new FakeSetEffectHost { IsRunning = false };
            using var ability = new SetEffectAbility(Define("FIXTURE-HEAL", new SetEffectDefinition(SetEffectKind.LevelHeal, healFraction: 0.05f)), host);
            host.Level(); Assert.AreEqual(5, host.Healed);
            host.IsTerminal = true; host.Level(); Assert.AreEqual(5, host.Healed);
            ability.Dispose(); host.IsTerminal = false; host.Level(); Assert.AreEqual(5, host.Healed);
        }
        [Test]
        public void InitializationFailure_RollsBackBuffAndSubscriptions()
        {
            var host = new FakeSetEffectHost { ThrowOnSkillModifier = true };
            var definition = Define("FIXTURE-FAIL", new SetEffectDefinition(SetEffectKind.StatBuff,
                new CharacterStatModifier(maxHealthMultiplierBonus: 0.2f)),
                new SetEffectDefinition(SetEffectKind.SkillTransform, skill: new ContentId("FIXTURE-SKILL")));
            Assert.Throws<InvalidOperationException>(() => new SetEffectAbility(definition, host));
            Assert.AreEqual(0, host.Stats.ModifierCount); Assert.AreEqual(0, host.SkillModifiers.Count);
            host.Activate(); host.Reward(); host.Level(); Assert.AreEqual(0, host.Attacks);
        }
        [Test]
        public void Recipes_RejectOutsideThreeToSixAndDuplicates()
        {
            var c = new SetRecipeComponent("FIXTURE-SKILL", BuildEntryKind.ActiveSkill, 1);
            Assert.Throws<ArgumentException>(() => new SetDefinition("FIXTURE-SET", "Set", c));
            Assert.Throws<ArgumentException>(() => new SetDefinition("FIXTURE-SET", "Set", c,c,c));
            Assert.Throws<ArgumentException>(() => new SetDefinition("FIXTURE-SET", "Set", c,c,c,c,c,c,c));
        }
        [Test]
        public void Catalog_AllAcceptedFamiliesHaveRealConfiguredFixtures()
        {
            var found = new System.Collections.Generic.HashSet<SetEffectKind>();
            foreach (var set in FixtureSetCatalog.Create())
            {
                Assert.That(set.Recipe.Count, Is.InRange(3, 6));
                foreach (var effect in set.Effects) found.Add(effect.Kind);
            }
            CollectionAssert.AreEquivalent(Enum.GetValues(typeof(SetEffectKind)), found);
            Assert.AreEqual(0.5f, FixtureRunSetupCatalog.Create().Draft.SetDraftChance);
        }
    }
}
