using System;
using Game.Combat;
using Game.Enemy;
using Game.Pooling;
using Game.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.ActiveSkill.Tests
{
    public sealed class ProjectileLifecycleTests
    {
        [Test]
        public void CustomVisual_SpinsChildOnlyAndImpactTailPausesBeforePoolReturn()
        {
            using var context = new SkillFrameworkTestContext();
            var target = context.Enemy(Vector2.right);
            var root = new GameObject("Projectile presentation pool");
            var sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1f);
            try
            {
                var profile = new ProjectilePresentationProfile(1.5f, 140f, .16f, .2f,
                    Color.white, 3, .05f, .4f, Color.gray);
                var visual = new SpriteDefinition("FIXTURE-PROJECTILE-VISUAL", sprite,
                    SpriteRole.Projectile, projectilePresentation: profile);
                var pool = new GameObjectPool<FixtureProjectileRuntime>(FixtureProjectileFactory.Create, root.transform);
                var shot = new ActiveSkillProjectile(Vector2.zero, Vector2.right, 2f, 2f, .15f, 0f,
                    new EnemyDamageRequest("FIXTURE-VISUAL", 1f), visual: visual);
                var projectile = FixtureProjectileFactory.Spawn(shot, context.Run, root.transform, pool);
                var visualRoot = projectile.transform.Find("ProjectileVisual");
                projectile.Simulate(.5f);
                Assert.AreEqual(70f, visualRoot.localEulerAngles.z, .01f);
                Assert.AreEqual(0f, projectile.transform.eulerAngles.z, .001f);
                Assert.AreEqual(.15f, projectile.GetComponent<CircleCollider2D>().radius, .0001f);

                Assert.IsTrue(projectile.TryImpact(target, target.Position));
                Assert.IsTrue(projectile.IsDespawned);
                Assert.IsFalse(projectile.GetComponent<CircleCollider2D>().enabled);
                Assert.AreEqual(0, pool.InactiveCount);
                context.Run.Model.Pause();
                projectile.Simulate(1f);
                Assert.AreEqual(0, pool.InactiveCount);
                context.Run.Model.Resume();
                projectile.Simulate(.16f);
                Assert.AreEqual(1, pool.InactiveCount);

                var terminal = FixtureProjectileFactory.Spawn(shot, context.Run, root.transform, pool);
                Assert.IsTrue(terminal.TryImpact(target, target.Position));
                context.Run.Model.Kill();
                terminal.Despawn();
                Assert.AreEqual(1, pool.InactiveCount, "Terminal cleanup must not wait for the visual tail.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(sprite);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ExplosionPresentation_ExpiryUsesReusableBurstAndWaitsForPauseAwareTail()
        {
            using var context = new SkillFrameworkTestContext();
            var root = new GameObject("Explosion presentation pool");
            var sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1),
                new Vector2(.5f, .5f), 1f);
            try
            {
                var explosion = new ExplosionPresentationProfile(.2f, 1.1f, Color.yellow,
                    6, .1f, 1f, new Color(1f, .3f, .05f, 1f));
                var profile = new ProjectilePresentationProfile(1f, 0f, .1f, .1f,
                    Color.white, 3, .05f, .4f, Color.gray, explosion);
                var visual = new SpriteDefinition("FIXTURE-EXPLOSIVE-SPHERE-VISUAL", sprite,
                    SpriteRole.Projectile, projectilePresentation: profile);
                var pool = new GameObjectPool<FixtureProjectileRuntime>(FixtureProjectileFactory.Create,
                    root.transform);
                var shot = new ActiveSkillProjectile(Vector2.zero, Vector2.right, 1f, .5f, .1f, .5f,
                    new EnemyDamageRequest("FIXTURE-EXPLOSIVE-SPHERE", 1f), visual: visual,
                    behavior: new ProjectileBehavior(explosionDamageMultiplier: 2f, explodeOnExpiry: true));
                var projectile = FixtureProjectileFactory.Spawn(shot, context.Run, root.transform, pool);

                projectile.Simulate(.5f);
                Assert.IsTrue(projectile.IsDespawned);
                Assert.IsFalse(projectile.GetComponent<CircleCollider2D>().enabled);
                Assert.IsTrue(projectile.GetComponent<ExplosionBurstRuntime>().IsPlaying);
                // Regression (playtest 2026-09-24_e1e04fc4 OBS-02): code-created particles need a material,
                // otherwise they render as magenta squares.
                var particleMaterial = projectile.GetComponent<ExplosionBurstRuntime>().Particles.GetComponent<ParticleSystemRenderer>().sharedMaterial;
                Assert.IsNotNull(particleMaterial);
                Assert.AreSame(ProceduralShapeSprites.Disc.texture, particleMaterial.mainTexture);
                Assert.AreEqual(0, pool.InactiveCount);
                context.Run.Model.Pause();
                projectile.Simulate(1f);
                Assert.AreEqual(0, pool.InactiveCount);
                context.Run.Model.Resume();
                projectile.Simulate(.2f);
                Assert.AreEqual(1, pool.InactiveCount);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(sprite);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        // Regression: impact and explosion presenters both lived on the pooled projectile root; the explosion's
        // AddComponent<ParticleSystem> returned null after an impact and threw inside FixedUpdate on every
        // exploding hit (SKILL-014), dropping play below 1 FPS. Each presenter now owns a child object.
        [Test]
        public void ImpactThenExplosion_SamePooledProjectile_UsesSeparateParticleSystemsAcrossReuse()
        {
            using var context = new SkillFrameworkTestContext();
            var root = new GameObject("Impact explosion pool");
            var sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1f);
            try
            {
                var enemy = context.Enemy(Vector2.right);
                Physics2D.SyncTransforms();
                var explosionProfile = new ExplosionPresentationProfile(.2f, 1.1f, Color.yellow,
                    6, .1f, 1f, new Color(1f, .3f, .05f, 1f));
                var profile = new ProjectilePresentationProfile(1f, 0f, .1f, .1f,
                    Color.white, 3, .05f, .4f, Color.gray, explosionProfile);
                var visual = new SpriteDefinition("FIXTURE-IMPACT-EXPLOSION-VISUAL", sprite,
                    SpriteRole.Projectile, projectilePresentation: profile);
                var pool = new GameObjectPool<FixtureProjectileRuntime>(FixtureProjectileFactory.Create, root.transform);
                var shot = new ActiveSkillProjectile(Vector2.zero, Vector2.right, 1f, 1f, .1f, .5f,
                    new EnemyDamageRequest("FIXTURE-IMPACT-EXPLOSION", 1f), visual: visual,
                    behavior: new ProjectileBehavior(explosionDamageMultiplier: 2f, explodeOnExpiry: true));
                var projectile = FixtureProjectileFactory.Spawn(shot, context.Run, root.transform, pool);

                // One hit plays the impact and then the explosion on the same projectile (SKILL-014 path).
                Assert.IsTrue(projectile.TryImpact(enemy, enemy.Position));
                var impact = projectile.GetComponent<ProjectileImpactRuntime>();
                var explosion = projectile.GetComponent<ExplosionBurstRuntime>();
                Assert.IsTrue(impact.IsPlaying);
                Assert.IsTrue(explosion.IsPlaying);
                var impactParticles = impact.Particles;
                var explosionParticles = explosion.Particles;
                Assert.IsNotNull(impactParticles);
                Assert.IsNotNull(explosionParticles);
                Assert.AreNotSame(impactParticles.gameObject, explosionParticles.gameObject);
                Assert.AreSame(projectile.transform, impactParticles.transform.parent);
                Assert.AreSame(projectile.transform, explosionParticles.transform.parent);
                Assert.IsNull(projectile.GetComponent<ParticleSystem>(), "The shared projectile root carries no particle system.");
                // particleCount is not observable for manually simulated systems in EditMode (always 0 in Unity 6000.6
                // runs), so emission is guarded through the presenters' IsPlaying and the pause-aware tail instead.
                projectile.Simulate(.15f);
                Assert.IsFalse(impact.IsPlaying, "Impact tail (0.1 s) finished.");
                Assert.IsTrue(explosion.IsPlaying, "Explosion tail (0.2 s) still running independently.");
                Assert.AreEqual(0, pool.InactiveCount);
                projectile.Simulate(.5f);
                Assert.IsFalse(explosion.IsPlaying);
                Assert.AreEqual(1, pool.InactiveCount, "Both tails finished and the projectile returned to the pool.");

                var reused = FixtureProjectileFactory.Spawn(shot, context.Run, root.transform, pool);
                Assert.AreSame(projectile, reused);
                Assert.IsTrue(reused.TryImpact(enemy, enemy.Position));
                Assert.AreSame(impactParticles, impact.Particles, "Created once, reused for the next life.");
                Assert.AreSame(explosionParticles, explosion.Particles);
                Assert.AreEqual(2, reused.GetComponentsInChildren<ParticleSystem>(true).Length);
                Assert.AreEqual(94f, enemy.Health.CurrentHealth, 1e-4f, "Two lives × (1 impact + 2 explosion) damage.");
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(sprite);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ExplosionCallback_CannotDespawnReinitializedProjectile(bool expiry)
        {
            using var context = new SkillFrameworkTestContext();
            var enemy = context.Enemy(Vector2.right);
            Physics2D.SyncTransforms();
            var shot = new ActiveSkillProjectile(Vector2.zero, Vector2.right, 1f, 1f, .1f, .5f,
                new EnemyDamageRequest("FIXTURE-EXPLOSION", 1f),
                behavior: new ProjectileBehavior(explosionDamageMultiplier: 2f, explodeOnExpiry: true));
            var projectile = FixtureProjectileFactory.Spawn(shot, context.Run);
            // The impact itself deals 1; only the explosion callback starts a new projectile life.
            enemy.Health.Damaged += amount => { if (amount == 2f) projectile.Initialize(shot, context.Run); };
            try
            {
                if (expiry) projectile.Simulate(1f);
                else projectile.TryImpact(enemy, enemy.Position);
                Assert.IsTrue(projectile != null);
                Assert.IsFalse(projectile.IsDespawned);
                Assert.AreEqual(Vector2.zero, projectile.Position);
                Assert.IsTrue(projectile.GetComponent<Collider2D>().enabled);
            }
            finally { if (projectile != null) projectile.Despawn(); }
        }

        [Test]
        public void Deceleration_IntegratesDistanceFreezesOnPauseAndReturnsDisabledColliderToPool()
        {
            using var context = new SkillFrameworkTestContext();
            var poolRoot = new GameObject("Projectile test pool");
            try
            {
                var pool = new GameObjectPool<FixtureProjectileRuntime>(FixtureProjectileFactory.Create, poolRoot.transform);
                var shot = new ActiveSkillProjectile(Vector2.zero, Vector2.right, 8f, 10f, .1f, 0f,
                    new EnemyDamageRequest("FIXTURE-DECEL", 1f), behavior: new ProjectileBehavior(stopAfterSeconds: 1.4f));
                var projectile = FixtureProjectileFactory.Spawn(shot, context.Run, poolRoot.transform, pool);
                projectile.Simulate(.7f);
                Assert.AreEqual(4.2f, projectile.Position.x, .0001f);
                context.Run.Model.Pause();
                projectile.Simulate(20f);
                Assert.AreEqual(4.2f, projectile.Position.x, .0001f);
                context.Run.Model.Resume();
                projectile.Simulate(.699f);
                Assert.AreEqual(5.6f, projectile.Position.x, .0001f);
                projectile.Simulate(.01f);
                Assert.IsTrue(projectile.IsDespawned);
                Assert.IsFalse(projectile.GetComponent<Collider2D>().enabled);
                Assert.AreEqual(1, pool.InactiveCount);
                var reused = FixtureProjectileFactory.Spawn(shot, context.Run, poolRoot.transform, pool);
                Assert.AreSame(projectile, reused);
                Assert.AreEqual(Vector2.zero, reused.Position);
                Assert.IsTrue(reused.GetComponent<Collider2D>().enabled);
                reused.Simulate(.7f);
                Assert.AreEqual(4.2f, reused.Position.x, .0001f);
                context.Run.Model.Kill();
                reused.Simulate(0f);
                Assert.IsTrue(reused.IsDespawned);
                Assert.AreEqual(1, pool.InactiveCount);
            }
            finally { UnityEngine.Object.DestroyImmediate(poolRoot); }
        }

        [Test]
        public void Boomerangs_ShareCooldownAcrossCastsAndPassesAndDistinguishReusedTargetLife()
        {
            using var context = new SkillFrameworkTestContext();
            var ledger = new SkillHitLedger();
            var target = new SkillTestTarget(Vector2.right);
            var damage = new EnemyDamageRequest(new CombatDamageRequest(default, 2f, new CombatControlProfile(1f, .1f)));
            var shot = new ActiveSkillProjectile(Vector2.zero, Vector2.right, 2f, 10f, .1f, 0f, damage,
                returnAfterSeconds: 2f, returnDamageMultiplier: 1.75f, returnTarget: context.Owner.transform,
                hitLedger: ledger, hitCooldownSeconds: 1f, returnKnockbackMultiplier: 1.5f);
            var first = FixtureProjectileFactory.Spawn(shot, context.Run);
            var second = FixtureProjectileFactory.Spawn(shot, context.Run);
            try
            {
                Assert.IsTrue(first.TryImpact(target, target.Position));
                Assert.IsFalse(second.TryImpact(target, target.Position));
                ledger.Tick(.5f, true);
                ledger.Tick(100f, false);
                Assert.IsFalse(first.TryImpact(target, target.Position));
                ledger.Tick(.5f, true);
                Assert.IsTrue(second.TryImpact(target, target.Position));
                Assert.AreEqual(2, target.Hits.Count);
                first.Simulate(2f);
                Assert.IsFalse(first.TryImpact(target, target.Position), "Return does not reset the shared cooldown.");
                ledger.Tick(1f, true);
                Assert.IsTrue(first.TryImpact(target, target.Position));
                Assert.AreEqual(3.5f, target.Hits[2].Amount);
                Assert.AreEqual(1.5f, target.Hits[2].Combat.OutgoingKnockbackMultiplier);
                Assert.Greater(target.Hits[2].Combat.DirectionX, 0f, "Knockback is radial from owner, despite returning left.");
                target.LifeId = Guid.NewGuid();
                Assert.IsTrue(second.TryImpact(target, target.Position));
                ledger.Clear();
                Assert.AreEqual(0, ledger.Count);
            }
            finally { first.Despawn(); second.Despawn(); }
        }

        [Test]
        public void Ricochet_SelectsNearestUnhitAndPreservesDamageRetention()
        {
            using var context = new SkillFrameworkTestContext();
            var first = context.Enemy(Vector2.right);
            var second = context.Enemy(Vector2.right * 2f);
            var third = context.Enemy(Vector2.right * 3f);
            var shot = new ActiveSkillProjectile(Vector2.zero, Vector2.right, 4f, 5f, .1f, 0f,
                new EnemyDamageRequest("FIXTURE-RICOCHET", 10f),
                behavior: new ProjectileBehavior(ricochetCount: 2, ricochetRange: 2f, ricochetRetention: .8f));
            var projectile = FixtureProjectileFactory.Spawn(shot, context.Run);
            Assert.IsTrue(projectile.TryImpact(first, first.Position));
            Assert.IsFalse(projectile.TryImpact(first, first.Position));
            Assert.IsTrue(projectile.TryImpact(second, second.Position));
            Assert.IsTrue(projectile.TryImpact(third, third.Position));
            Assert.AreEqual(90f, first.Health.CurrentHealth);
            Assert.AreEqual(92f, second.Health.CurrentHealth);
            Assert.AreEqual(93.6f, third.Health.CurrentHealth, .0001f);
            Assert.IsTrue(projectile == null);
        }

        [Test]
        public void Sphere_PiercesFirstTargetThenAppliesSeparateExplosionAndCanExplodeOnExpiry()
        {
            using var context = new SkillFrameworkTestContext();
            var first = context.Enemy(Vector2.right);
            var second = context.Enemy(Vector2.right * 2f);
            Physics2D.SyncTransforms();
            var behavior = new ProjectileBehavior(explosionDamageMultiplier: 2f, explodeOnExpiry: true);
            var shot = new ActiveSkillProjectile(Vector2.zero, Vector2.right, 2f, 1f, .1f, .2f,
                new EnemyDamageRequest("FIXTURE-SPHERE", 3f), pierceCount: 1, behavior: behavior);
            var projectile = FixtureProjectileFactory.Spawn(shot, context.Run);
            Assert.IsTrue(projectile.TryImpact(first, first.Position));
            Assert.IsFalse(projectile.IsDespawned);
            Assert.AreEqual(97f, first.Health.CurrentHealth);
            Assert.IsTrue(projectile.TryImpact(second, second.Position));
            Assert.AreEqual(91f, second.Health.CurrentHealth);
            projectile = FixtureProjectileFactory.Spawn(shot, context.Run);
            projectile.Simulate(1f);
            Assert.AreEqual(85f, second.Health.CurrentHealth);
            Assert.IsTrue(projectile == null);
        }
    }
}
