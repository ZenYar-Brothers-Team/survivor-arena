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

        [Test]
        public void MovementVariants_RequireChanceAndRejectTotalsAboveOne()
        {
            var data = Data();
            data.MovementVariants = new[]
            {
                new EnemyMovementVariantData
                {
                    Movement = new EnemyMovementProfileData
                    {
                        Kind = "OffsetPursuit", PreferredDistance = 1f, DistanceTolerance = .5f,
                        CycleSeconds = 2f, DirectPursuitSeconds = 1f
                    }
                }
            };
            Assert.Throws<InvalidOperationException>(() => FixtureEnemyCatalog.ToDefinition(data));

            data.MovementVariants[0].Chance = .2f;
            data.MovementVariants[0].Movement = null;
            Assert.Throws<InvalidOperationException>(() => FixtureEnemyCatalog.ToDefinition(data));

            data.MovementVariants = new[]
            {
                new EnemyMovementVariantData { Chance = .6f, Movement = new EnemyMovementProfileData { Kind = "CommittedPursuit", CycleSeconds = 1f } },
                new EnemyMovementVariantData { Chance = .6f, Movement = new EnemyMovementProfileData { Kind = "CommittedPursuit", CycleSeconds = 2f } }
            };
            Assert.Throws<ArgumentException>(() => FixtureEnemyCatalog.ToDefinition(data));
        }

        [Test]
        public void AntiBlobMovementKinds_RequireTheirOwnParameters()
        {
            var data = Data();
            data.Movement = new EnemyMovementProfileData
            {
                Kind = "BlockedSidestep", PreferredDistance = 1.1f, LateralStrength = 2f,
                BlockedProgressFraction = .35f, SidestepSeconds = .9f,
                SidestepCooldownSeconds = 1.2f,
                SidestepNearDistance = 2.2f, SidestepNearSeconds = 1.2f
            };
            Assert.Throws<InvalidOperationException>(() => FixtureEnemyCatalog.ToDefinition(data));
            data.Movement.BlockedTriggerSeconds = .4f;
            Assert.AreEqual(EnemyMovementKind.BlockedSidestep, FixtureEnemyCatalog.ToDefinition(data).Movement.Kind);

            data.Movement = new EnemyMovementProfileData
            {
                Kind = "ArcPassPursuit", PreferredDistance = 3f, LateralStrength = 1.2f,
                CycleSeconds = 2.2f
            };
            Assert.Throws<InvalidOperationException>(() => FixtureEnemyCatalog.ToDefinition(data));
            data.Movement.DirectPursuitSeconds = .8f;
            Assert.AreEqual(EnemyMovementKind.ArcPassPursuit, FixtureEnemyCatalog.ToDefinition(data).Movement.Kind);

            data.Movement = new EnemyMovementProfileData { Kind = "InertialPursuit" };
            Assert.Throws<InvalidOperationException>(() => FixtureEnemyCatalog.ToDefinition(data));
            data.Movement.TurnResponseSeconds = 1.2f;
            Assert.AreEqual(EnemyMovementKind.InertialPursuit, FixtureEnemyCatalog.ToDefinition(data).Movement.Kind);
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
