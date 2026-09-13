using System;
using Game.Content;
using NUnit.Framework;

namespace Game.Enemy.Tests
{
    public class EnemyDefinitionTests
    {
        [Test]
        public void Definition_IsResolvedThroughContentRegistry()
        {
            var definition = new EnemyDefinition("FIXTURE-ENEMY", 10f, 1f, 2f, 3f, 0.5f);
            var registry = ContentRegistry.BuildFrom(new IContentDefinition[] { definition });

            var resolved = registry.Get<EnemyDefinition>("FIXTURE-ENEMY");

            Assert.AreSame(definition, resolved);
            Assert.AreEqual(10f, resolved.MaxHealth);
            Assert.AreEqual(1f, resolved.CollisionSize);
            Assert.AreEqual(2f, resolved.MovementSpeed);
            Assert.AreEqual(3f, resolved.ContactDamage);
            Assert.AreEqual(0.5f, resolved.ContactDamageInterval);
        }

        [Test]
        public void Definition_RejectsInvalidRuntimeValues()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new EnemyDefinition("FIXTURE", 0f, 1f, 1f, 1f, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new EnemyDefinition("FIXTURE", 1f, 0f, 1f, 1f, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new EnemyDefinition("FIXTURE", 1f, 1f, -1f, 1f, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new EnemyDefinition("FIXTURE", 1f, 1f, 1f, -1f, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new EnemyDefinition("FIXTURE", 1f, 1f, 1f, 1f, 0f));
        }
    }
}
