using System;
using System.Linq;
using Game.ActiveSkill.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Game.ActiveSkill.Tests
{
    public sealed class SkillLevelResolutionTests
    {
        [Test]
        public void CumulativeFixture_ResolvesSeveralParametersFromBaseWithoutDoubleApplication()
        {
            var definition = FixtureActiveSkillCatalog.Create().Single(s => s.Id.ToString() == "FIXTURE-SKILL-MOVEMENT");
            var first = definition.GetLevel(1);
            var last = definition.GetLevel(6);
            var projectile = (ProjectileBurstEffect)last.Waves[0].Effects[0];
            Assert.AreEqual(20f, first.BaseDamage);
            Assert.AreEqual(26f, definition.GetLevel(2).BaseDamage);
            Assert.AreEqual(31f, last.BaseDamage, 0.0001f, "DECISION-0021: 20 * (1 + .30 + .25), not 32.5.");
            Assert.AreEqual(.2325f, projectile.CollisionRadius, .0001f);
            Assert.AreEqual(2.4f, projectile.LifetimeSeconds, .0001f);
            Assert.AreEqual(9.6f, projectile.Speed, .0001f);
            Assert.AreEqual(.24f, last.Waves[0].Controls.KnockbackDistance, .0001f);
            Assert.AreEqual(.25f, last.Targeting.ActionSpeedBonus);
            var instance = new ActiveSkillInstance(definition);
            instance.SetLevel(6);
            instance.SetLevel(6);
            Assert.AreSame(last, definition.GetLevel(instance.Level));
            Assert.AreEqual(.15f, ((ProjectileBurstEffect)first.Waves[0].Effects[0]).CollisionRadius);
        }

        [Test]
        public void Resolver_RejectsUnknownPathsAndInvalidDomainResultsWithoutMutatingSource()
        {
            var baseline = JObject.Parse(@"{'baseDamage':20,'cooldownSeconds':2,'targetingMode':'Self','waves':[{'effects':[{'kind':'Area','radius':1}]}]}");
            var changes = Enumerable.Range(0, 6).Select(_ => new ActiveSkillLevelChangeData()).ToArray();
            changes[1].Bonuses = new System.Collections.Generic.Dictionary<string, float> { ["missing"] = .3f };
            Assert.Catch<JsonException>(() => ActiveSkillLevelResolver.Resolve(baseline, changes,
                new JsonSerializerSettings { Converters = { new ActiveSkillEffectJsonConverter() } }));
            Assert.AreEqual(20, baseline["baseDamage"].Value<int>());
            changes[1].Bonuses = new System.Collections.Generic.Dictionary<string, float> { ["waves[0].effects[0].radius"] = -2f };
            Assert.Throws<ArgumentOutOfRangeException>(() => FixtureActiveSkillCatalog.ToDefinition(new ActiveSkillProgressionData
                { Id = "FIXTURE-INVALID", DisplayName = "Invalid", BaseLevel = baseline, LevelChanges = changes }));
        }
    }
}
