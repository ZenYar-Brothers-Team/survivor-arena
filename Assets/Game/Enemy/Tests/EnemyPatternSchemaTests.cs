using System;
using Game.Combat;
using Game.Enemy.Json;
using NUnit.Framework;
namespace Game.Enemy.Tests
{
    public sealed class EnemyPatternSchemaTests
    {
        [TestCase("contact")]
        [TestCase("attack")]
        [TestCase("distance")]
        [TestCase("duration")]
        [TestCase("telegraph")]
        [TestCase("spread")]
        [TestCase("rotation")]
        [TestCase("explosion")]
        [TestCase("burst")]
        public void MissingRequiredField_RejectsContent(string field)
        {
            var data = Data();
            switch (field)
            {
                case "contact": data.ContactControls = null; break;
                case "attack": data.Attack.Controls = null; break;
                case "distance": data.Attack.Controls.KnockbackDistance = null; break;
                case "duration": data.Attack.Controls.KnockbackDistance = 1; break;
                case "telegraph": data.Attack.TelegraphSeconds = null; break;
                case "spread": data.Attack.SpreadDegrees = null; break;
                case "rotation": data.Attack.Pattern = "Spiral"; break;
                case "explosion": data.Attack.Pattern = "Explosive"; break;
                case "burst": data.Attack.Pattern = "Burst"; break;
            }
            Assert.That(() => FixtureEnemyCatalog.ToDefinition(data), Throws.Exception);
        }
        [Test]
        public void ExplicitZeroControls_AreValidAndNonfiniteWindupIsRejected()
        {
            var data = Data();
            Assert.AreEqual(0, FixtureEnemyCatalog.ToDefinition(data).Attack.Controls.KnockbackDistance);
            data.Attack.TelegraphSeconds = float.NaN;
            Assert.Throws<ArgumentOutOfRangeException>(() => FixtureEnemyCatalog.ToDefinition(data));
        }
        [Test]
        public void Dash_RequiresSeparateContactControlProfile()
        {
            var data = Data();
            data.Movement = new EnemyMovementProfileData
            {
                Kind = "TelegraphedDash", DashTelegraphSeconds = .5f, DashDurationSeconds = .25f,
                DashCooldownSeconds = 2, DashSpeedMultiplier = 3
            };
            Assert.Throws<InvalidOperationException>(() => FixtureEnemyCatalog.ToDefinition(data));
            data.DashContactControls = new CombatControlData { KnockbackDistance = .4f, KnockbackSeconds = .1f };
            var definition = FixtureEnemyCatalog.ToDefinition(data);
            Assert.AreEqual(.4f, definition.DashContactControls.KnockbackDistance);
            Assert.AreEqual(0, definition.ContactControls.KnockbackDistance);
        }
        private static EnemyDefinitionData Data() => new EnemyDefinitionData
        {
            Id = "FIXTURE-SCHEMA", MaxHealth = 10, CollisionSize = 1, MovementSpeed = 1,
            ContactDamage = 1, ContactDamageInterval = 1, KnockbackResistance = 0,
            ContactControls = new CombatControlData { KnockbackDistance = 0 },
            Attack = new EnemyAttackProfileData
            {
                Pattern = "Fan", Damage = 1, CooldownSeconds = 1, ProjectileSpeed = 1,
                ProjectileLifetimeSeconds = 1, ProjectileCount = 3, SpreadDegrees = 40,
                ProjectileRadius = .1f, TelegraphSeconds = .25f,
                Controls = new CombatControlData { KnockbackDistance = 0 }
            }
        };
    }
}
