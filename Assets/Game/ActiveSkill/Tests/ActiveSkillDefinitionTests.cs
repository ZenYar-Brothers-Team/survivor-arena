using System;
using Game.Content;
using NUnit.Framework;

namespace Game.ActiveSkill.Tests
{
    public class ActiveSkillDefinitionTests
    {
        [Test]
        public void Definition_PreservesConfiguredFixtureValuesAndBuildsInRegistry()
        {
            var definition = CreateDefinition();
            var registry = ContentRegistry.BuildFrom(new IContentDefinition[] { definition });

            Assert.AreSame(definition, registry.Get<ActiveSkillDefinition>("FIXTURE-SKILL-BOLT"));
            Assert.AreEqual(5f, definition.BaseDamage);
            Assert.AreEqual(0.75f, definition.CooldownSeconds);
            Assert.AreEqual(10f, definition.ProjectileSpeed);
            Assert.AreEqual(2f, definition.ProjectileLifetimeSeconds);
            Assert.AreEqual(0.15f, definition.ProjectileCollisionRadius);
            Assert.AreEqual(0.5f, definition.ImpactAreaRadius);
        }

        [TestCase(-1f, 1f, 1f, 1f, 0.1f, 0f)]
        [TestCase(1f, 0f, 1f, 1f, 0.1f, 0f)]
        [TestCase(1f, 1f, 0f, 1f, 0.1f, 0f)]
        [TestCase(1f, 1f, 1f, 0f, 0.1f, 0f)]
        [TestCase(1f, 1f, 1f, 1f, 0f, 0f)]
        [TestCase(1f, 1f, 1f, 1f, 0.1f, -1f)]
        public void Definition_RejectsInvalidBalanceValues(
            float damage,
            float cooldown,
            float speed,
            float lifetime,
            float collisionRadius,
            float impactRadius)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ActiveSkillDefinition(
                "FIXTURE-SKILL",
                damage,
                cooldown,
                speed,
                lifetime,
                collisionRadius,
                impactRadius));
        }

        private static ActiveSkillDefinition CreateDefinition()
        {
            return new ActiveSkillDefinition(
                "FIXTURE-SKILL-BOLT",
                5f,
                0.75f,
                10f,
                2f,
                0.15f,
                0.5f);
        }
    }
}
