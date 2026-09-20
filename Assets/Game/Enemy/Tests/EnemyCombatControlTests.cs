using System;
using System.Linq;
using System.Reflection;
using Game.Character;
using Game.Combat;
using Game.Pooling;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Enemy.Tests
{
    public sealed class EnemyCombatControlTests
    {
        private GameObject _root;
        private PlayerCharacterRuntime _player;
        private RunController _run;
        private GameObjectPool<EnemyRuntime> _pool;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("combat-controls-test");
            _run = _root.AddComponent<RunController>();
            if (!_run.IsInitialized) _run.Initialize();
            _run.Model.Start();
            var player = new GameObject("player");
            player.transform.SetParent(_root.transform);
            player.transform.position = Vector3.right * 10f;
            _player = player.AddComponent<PlayerCharacterRuntime>();
            _player.Initialize(new CharacterBaseStats(100f, 3f), _run, "FIXTURE-CHARACTER");
            _pool = new GameObjectPool<EnemyRuntime>(EnemyFactory.CreateInstance, _root.transform);
        }

        [TearDown] public void TearDown() => Object.DestroyImmediate(_root);

        [TestCase(false, 3f)]
        [TestCase(true, 6f)]
        public void Knockback_AddsToOrdinaryOrDashVelocity_AndDashClockContinues(bool dash, float baseSpeed)
        {
            var movement = dash ? new EnemyMovementProfile(EnemyMovementKind.TelegraphedDash,
                dashCooldownSeconds: 0.07f, dashTelegraphSeconds: 0f, dashDurationSeconds: 0.1f, dashSpeedMultiplier: 2f) : null;
            var enemy = Spawn(new EnemyDefinition("FIXTURE-CONTROL", 10f, 1f, 3f, 0f, 1f, movement: movement));
            if (dash) for (var i = 0; i < 3; i++) Invoke(enemy, "FixedUpdate");
            enemy.ResolveDamage(new CombatDamageRequest(default, 0f, new CombatControlProfile(1f, 1f), 0f, 1f));
            Invoke(enemy, "FixedUpdate");
            var velocity = enemy.GetComponent<Rigidbody2D>().linearVelocity;
            Assert.AreEqual(baseSpeed, velocity.x, 0.0001f);
            Assert.AreEqual(1f, velocity.y, 0.0001f);
            if (dash) Assert.AreEqual(EnemyMovementPhase.Dashing, enemy.MovementPhase);
            var remaining = enemy.Controls.KnockbackRemaining;
            _run.TogglePause();
            Invoke(enemy, "FixedUpdate");
            Assert.AreEqual(Vector2.zero, enemy.GetComponent<Rigidbody2D>().linearVelocity);
            Assert.AreEqual(remaining, enemy.Controls.KnockbackRemaining);
            _run.TogglePause();
            for (var i = 0; i < 6; i++) Invoke(enemy, "FixedUpdate");
            Assert.Less(enemy.Controls.KnockbackRemaining, remaining);
            if (dash) Assert.AreNotEqual(EnemyMovementPhase.Dashing, enemy.MovementPhase);
            _run.Model.Stop();
            Invoke(enemy, "FixedUpdate");
            Assert.AreEqual(Vector2.zero, enemy.GetComponent<Rigidbody2D>().linearVelocity);
        }

        [Test]
        public void FullSlow_DoesNotStopAttackCadence_OrDashPhaseTime()
        {
            var attack = new EnemyAttackProfile(EnemyProjectilePattern.Fan, 1f, 0.05f, 1f, 1f);
            var movement = new EnemyMovementProfile(EnemyMovementKind.TelegraphedDash,
                dashCooldownSeconds: 0.07f, dashTelegraphSeconds: 0f, dashDurationSeconds: 0.1f, dashSpeedMultiplier: 2f);
            var definition = new EnemyDefinition("FIXTURE-SLOW", 10f, 1f, 3f, 0f, 1f, movement: movement, attack: attack);
            var slow = Spawn(definition);
            var normal = Spawn(definition);
            slow.ResolveDamage(new CombatDamageRequest(default, 0f, new CombatControlProfile(slowFraction: 1f, slowSeconds: 1f)));
            for (var i = 0; i < 7; i++) { Invoke(slow, "FixedUpdate"); Invoke(normal, "FixedUpdate"); }
            Assert.AreEqual(Vector2.zero, slow.GetComponent<Rigidbody2D>().linearVelocity);
            Assert.AreEqual(normal.MovementPhase, slow.MovementPhase);
            var projectiles = _root.GetComponentsInChildren<EnemyProjectileRuntime>();
            var normalCount = projectiles.Count(p => p.Source.Owner.LifeId == normal.LifeId);
            Assert.Greater(normalCount, 0);
            Assert.AreEqual(normalCount, projectiles.Count(p => p.Source.Owner.LifeId == slow.LifeId));
        }

        [Test]
        public void LethalResult_RetainsBothIdentitiesAfterPoolReturn_AndControlsDoNotLeak()
        {
            var enemy = Spawn(new EnemyDefinition("FIXTURE-LETHAL", 10f, 1f, 0f, 0f, 1f));
            var life = enemy.LifeId;
            var source = new CombatSource(_player.Identity, "FIXTURE-SKILL", CombatSourceOrigin.ActiveSkill, 3);
            CombatResult observed = default;
            enemy.CombatResolved += value => observed = value;
            var result = enemy.ResolveDamage(new CombatDamageRequest(source, 100f,
                new CombatControlProfile(1f, 1f, 0.2f, 1f), 1f, 0f));
            var reused = Spawn(enemy.Definition);
            Assert.AreSame(enemy, reused);
            Assert.AreNotEqual(life, reused.LifeId);
            Assert.AreEqual(life, result.Target.LifeId);
            Assert.AreEqual(life, observed.Target.LifeId);
            Assert.AreEqual(_player.Identity.LifeId, result.Source.Owner.LifeId);
            Assert.AreEqual(_run.Model.RunId, result.Target.RunId);
            Assert.AreEqual(3, result.Source.SkillLevel);
            Assert.AreEqual(10f, result.Health.Actual);
            Assert.AreEqual(90f, result.Health.Overkill);
            Assert.AreEqual(0, reused.Controls.SlowSourceCount);
            Assert.AreEqual(0f, reused.Controls.KnockbackRemaining);
        }

        [Test]
        public void EnemyProjectile_RetainsDeadOwnersSource_PlayerResultAndResistanceUseSharedPipeline()
        {
            var enemy = Spawn(new EnemyDefinition("FIXTURE-SOURCE", 10f, 1f, 0f, 0f, 1f));
            var source = new CombatSource(enemy.Identity, enemy.ContentId, CombatSourceOrigin.EnemyProjectile);
            var profile = new EnemyAttackProfile(EnemyProjectilePattern.Fan, 20f, 1f, 1f, 1f,
                controls: new CombatControlProfile(2f, 0.15f));
            var projectile = EnemyProjectileFactory.Spawn(profile, Vector2.zero, Vector2.right, _player, _run, _root.transform, source: source);
            enemy.TakeDamage(100f);
            Spawn(enemy.Definition);
            _player.SetModifier("resistance", new CharacterStatModifier(incomingDamageReductionBonus: 0.5f, knockbackResistanceBonus: 0.4f));
            CombatResult hit = default;
            _player.CombatResolved += result => hit = result;
            Invoke(projectile, "ApplyImpact", Vector2.zero);
            Assert.AreEqual(source.Owner.LifeId, hit.Source.Owner.LifeId);
            Assert.AreEqual(CombatSourceOrigin.EnemyProjectile, hit.Source.Origin);
            Assert.AreEqual(10f, hit.Health.Actual);
            Assert.AreEqual(1.2f, hit.ResolvedKnockbackDistance, 0.0001f);
            Assert.AreEqual(8f, _player.TickAdditionalMovement(0.1f, true).x, 0.0001f);
        }

        private static void Invoke(object target, string method, params object[] args) =>
            target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);

        private EnemyRuntime Spawn(EnemyDefinition definition) => EnemyFactory.Spawn(definition, Vector2.zero,
            _player.transform, _run, _root.transform, pool: _pool);
    }
}
