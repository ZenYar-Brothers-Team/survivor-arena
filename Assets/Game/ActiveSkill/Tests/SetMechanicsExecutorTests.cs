using Game.Combat;
using Game.Content;
using Game.Enemy;
using Game.Presentation;
using Game.Progression;
using NUnit.Framework;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    /// <summary>sets-v1 (DECISION-0061): skill-specific set mechanics applied by the executor from the activation snapshot.</summary>
    public sealed class SetMechanicsExecutorTests
    {
        private SkillFrameworkTestContext _context;
        private CombatRecordingLauncher _launcher;
        private SceneActiveSkillEffectExecutor _executor;

        [SetUp]
        public void SetUp()
        {
            _context = new SkillFrameworkTestContext();
            _launcher = new CombatRecordingLauncher();
            _executor = new SceneActiveSkillEffectExecutor(_context.Run, _launcher);
        }

        [TearDown]
        public void TearDown()
        {
            _executor.Dispose();
            _context.Dispose();
        }

        [Test]
        public void ProjectileSpeedBonus_KeepsTravelDistance_AndExtraPierceSkipsUnlimited()
        {
            Fire(Burst(new ProjectileBurstEffect(1, ProjectileLayout.Single, 0f, 2, 10f, 0.5f, 0.2f)),
                new SkillMechanicBonus(projectileSpeedBonus: 0.3f, extraPierce: 2));
            var fast = _launcher.Projectiles[0];
            Assert.AreEqual(13f, fast.Speed, 1e-4f);
            Assert.AreEqual(5f, fast.Speed * fast.LifetimeSeconds, 1e-4f, "Speed never grows reach (sets-v1).");
            Assert.AreEqual(4, fast.PierceCount);

            Fire(Burst(new ProjectileBurstEffect(1, ProjectileLayout.Single, 0f, 0, 10f, 0.5f, 0.2f,
                behavior: new ProjectileBehavior(unlimitedPierce: true))), new SkillMechanicBonus(extraPierce: 2));
            Assert.AreEqual(0, _launcher.Projectiles[1].PierceCount, "Unlimited pierce stays the unlimited flag.");
        }

        [Test]
        public void ReturnBonus_BoostsDiscReboundsAndBoomerangReturn_OnlyThere()
        {
            var bonus = new SkillMechanicBonus(returnDamageBonus: 0.5f, returnSpeedBonus: 0.3f);
            Fire(Burst(new ProjectileBurstEffect(1, ProjectileLayout.Single, 0f, 0, 9f, 1f, 0.2f,
                behavior: new ProjectileBehavior(ricochetCount: 2, ricochetRange: 3f))), bonus);
            Assert.AreEqual(1.5f, _launcher.Projectiles[0].ReboundDamageMultiplier, 1e-5f);
            Assert.AreEqual(1.3f, _launcher.Projectiles[0].ReboundSpeedMultiplier, 1e-5f);

            Fire(Burst(new ProjectileBurstEffect(1, ProjectileLayout.Single, 0f, 0, 9f, 1f, 0.2f)), bonus);
            Assert.AreEqual(1f, _launcher.Projectiles[1].ReboundDamageMultiplier, "No ricochet, no rebound phase.");

            Fire(Burst(new BoomerangEffect(1, 0f, 6f, 4f, 0.2f, 1.75f, hitCooldownSeconds: 1f, lifetimeSeconds: 4f)), bonus);
            Assert.AreEqual(2.25f, _launcher.Projectiles[2].ReturnDamageMultiplier, 1e-5f, "Additive to the card multiplier.");
            Assert.AreEqual(1.3f, _launcher.Projectiles[2].ReturnSpeedMultiplier, 1e-5f);
        }

        [Test]
        public void HeavyReplacement_EveryEighthProjectile_StopsLaterAndExplodesWithoutOtherSetsDamage()
        {
            var mechanics = new SkillMechanicBonus(explosionRadiusBonus: 0.15f, heavyEveryNth: 8, heavySizeMultiplier: 2f,
                heavyStopMultiplier: 1.5f, heavyExplosionRadius: 1.2f, heavyExplosionDamageMultiplier: 4f, heavyExplosionKnockback: 0.6f);
            var junk = Burst(new ProjectileBurstEffect(1, ProjectileLayout.Single, 0f, 1, 8f, 1.4f, 0.12f,
                behavior: new ProjectileBehavior(stopAfterSeconds: 1.4f)), new CombatControlProfile(0.08f, 0.12f));
            var sequence = new SkillProjectileSequence();
            for (var i = 0; i < 16; i++) Fire(junk, mechanics, sequence, setDamageNormalization: 1f / 1.5f);

            for (var i = 0; i < 16; i++)
            {
                var projectile = _launcher.Projectiles[i];
                var heavy = i == 7 || i == 15;
                Assert.AreEqual(heavy, projectile.Behavior.ExplodeOnExpiry, $"projectile {i}");
                Assert.AreEqual(heavy ? 0.24f : 0.12f, projectile.CollisionRadius, 1e-5f, $"projectile {i}");
            }
            var heavyJunk = _launcher.Projectiles[7];
            Assert.IsTrue(heavyJunk.Behavior.UnlimitedPierce, "Passes through until it stops.");
            Assert.AreEqual(2.1f, heavyJunk.Behavior.StopAfterSeconds, 1e-5f);
            Assert.AreEqual(1.38f, heavyJunk.ImpactAreaRadius, 1e-5f, "1.2 × (1 + 0.15).");
            Assert.AreEqual(4f / 1.5f, heavyJunk.Behavior.ExplosionDamageMultiplier, 1e-5f, "SET-015 junk damage is not amplified (G-05).");
            Assert.AreEqual(0.6f / 0.08f, heavyJunk.Behavior.ExplosionKnockbackMultiplier, 1e-4f);
            Assert.AreEqual(0f, _launcher.Projectiles[0].ImpactAreaRadius, "Ordinary junk has no explosion to enlarge.");
        }

        [Test]
        public void HeavyReplacement_UsesSetSpriteWhileOrdinaryJunkKeepsSkillSprite()
        {
            var sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), Vector2.one * .5f, 1f);
            try
            {
                var skillVisual = new SpriteDefinition("SKILL-016-VISUAL-PROJECTILE", sprite, SpriteRole.Projectile);
                var heavyVisual = new SpriteDefinition("SET-008-VISUAL-PROJECTILE", sprite, SpriteRole.Projectile);
                var registry = ContentRegistry.BuildFrom(new IContentDefinition[] { skillVisual, heavyVisual });
                _executor.Dispose();
                _executor = new SceneActiveSkillEffectExecutor(_context.Run, _launcher, contentRegistry: registry);
                var level = new ActiveSkillLevelDefinition(10f, 1f, ActiveSkillTargetingMode.Self,
                    new ContentRef<SpriteDefinition>(skillVisual.Id),
                    new ActiveSkillActivationWave(0f, 0f, 1f,
                        new ProjectileBurstEffect(1, ProjectileLayout.Single, 0f, 0, 8f, 1f, .12f,
                            behavior: new ProjectileBehavior(stopAfterSeconds: 1f))));
                var mechanics = new SkillMechanicBonus(heavyEveryNth: 2, heavySizeMultiplier: 2f,
                    heavyStopMultiplier: 1.5f, heavyExplosionRadius: 1f, heavyExplosionDamageMultiplier: 2f,
                    heavyExplosionKnockback: .1f);
                var sequence = new SkillProjectileSequence();

                Fire(level, mechanics, sequence);
                Fire(level, mechanics, sequence);

                Assert.AreSame(skillVisual, _launcher.Projectiles[0].Visual);
                Assert.AreSame(heavyVisual, _launcher.Projectiles[1].Visual);
            }
            finally { Object.DestroyImmediate(sprite); }
        }

        [Test]
        public void ExplosionBonus_ScalesOnlyTheSphereExplosion()
        {
            Fire(Burst(new ProjectileBurstEffect(1, ProjectileLayout.Single, 0f, 0, 3f, 1.8f, 0.22f, impactAreaRadius: 1.3f,
                    behavior: new ProjectileBehavior(explosionDamageMultiplier: 2f, explodeOnExpiry: true))),
                new SkillMechanicBonus(explosionDamageBonus: 0.35f, explosionRadiusBonus: 0.25f));
            var sphere = _launcher.Projectiles[0];
            Assert.AreEqual(2.7f, sphere.Behavior.ExplosionDamageMultiplier, 1e-5f);
            Assert.AreEqual(1.625f, sphere.ImpactAreaRadius, 1e-5f);
            Assert.AreEqual(0.22f, sphere.CollisionRadius, 1e-5f, "Impact body is not an explosion.");
            Assert.IsTrue(sphere.Behavior.ExplodeOnExpiry);
        }

        [Test]
        public void ChainBonus_AddsTargetsAndHalvesFalloff()
        {
            var enemies = new EnemyRuntime[6];
            for (var i = 0; i < enemies.Length; i++) enemies[i] = _context.Enemy(new Vector2(i + 1f, 0f));
            Physics2D.SyncTransforms();
            Fire(Burst(new ChainEffect(3, 1.2f, 0.8f)), new SkillMechanicBonus(extraChainTargets: 2, chainFalloffReduction: 0.5f),
                target: enemies[0], damage: 10f);

            var expected = new[] { 90f, 91f, 91.9f, 92.71f, 93.439f, 100f };
            for (var i = 0; i < enemies.Length; i++)
                Assert.AreEqual(expected[i], enemies[i].Health.CurrentHealth, 1e-3f, $"enemy {i}: retention 0.8 → 0.9, 5 targets");
        }

        [Test]
        public void FanOut_HitsPrimaryThenNearestOthersAtOnce_WithoutFurtherJumps()
        {
            var primary = _context.Enemy(new Vector2(2f, 0f));
            var right = _context.Enemy(new Vector2(3f, 0f));
            var above = _context.Enemy(new Vector2(2f, 1.2f));
            var below = _context.Enemy(new Vector2(2f, -1.5f));
            var far = _context.Enemy(new Vector2(5.5f, 0f));
            Physics2D.SyncTransforms();
            Fire(Burst(new ChainEffect(3, 2f, 0.5f, fanOut: true)), default, target: primary, damage: 20f);

            Assert.AreEqual(80f, primary.Health.CurrentHealth, 1e-4f);
            Assert.AreEqual(90f, right.Health.CurrentHealth, 1e-4f);
            Assert.AreEqual(90f, above.Health.CurrentHealth, 1e-4f);
            Assert.AreEqual(100f, below.Health.CurrentHealth, 1e-4f, "Only TargetCount-1 = 2 branches, nearest first.");
            Assert.AreEqual(100f, far.Health.CurrentHealth, 1e-4f, "Branches never jump on.");
        }

        [Test]
        public void MineExplosionBonus_WidensAndStrengthensTheBlast()
        {
            var edge = _context.Enemy(new Vector2(1.9f, 0f)); // collider edge 1.4: outside 1.0, inside 1.5
            Physics2D.SyncTransforms();
            Fire(Burst(new MineEffect(0.8f, 1f, 5f, 4)), new SkillMechanicBonus(explosionDamageBonus: 0.5f, explosionRadiusBonus: 0.5f),
                damage: 10f, tick: false);
            _executor.Tick(0f, true);
            _executor.Tick(5.1f, true);
            Assert.AreEqual(85f, edge.Health.CurrentHealth, 1e-4f);
        }

        [Test]
        public void OrbitRotationBonus_SpeedsUpThePersistentOrbit()
        {
            var orbit = new OrbitEffect(2, 1f, 90f, 0f, 0.6f, bladeHitboxRadius: 0.1f, persistent: true);
            Fire(Burst(orbit), new SkillMechanicBonus(orbitAngularSpeedBonus: 0.35f), tick: false);
            _executor.Tick(0f, true);
            _executor.TryGetPersistentOrbit(SkillId, out _, out var start);
            _executor.Tick(1f, true);
            _executor.TryGetPersistentOrbit(SkillId, out _, out var end);
            Assert.AreEqual(90f * 1.35f, Mathf.DeltaAngle(start, end) + (Mathf.DeltaAngle(start, end) < 0f ? 360f : 0f), 1e-2f);
        }

        [Test]
        public void Instance_SnapshotsMechanics_AndNormalizesOnlySkillSpecificSetDamage()
        {
            var junk = new ActiveSkillInstance(System.Linq.Enumerable.Single(ProductionActiveSkillCatalog.Create(),
                s => s.Id.ToString() == "SKILL-016"));
            var capture = new CapturingEffectExecutor();
            var mechanics = new SkillMechanicBonus(explosionRadiusBonus: 0.15f);
            _context.Player.Stats.SetModifier("whetstone", new Game.Character.CharacterStatModifier(activeSkillDamageMultiplierBonus: 0.5f));
            Assert.IsTrue(junk.Tick(1f, true, _context.Player, new SceneEnemyTargetProvider(), capture,
                skillModifier: new Game.Character.CharacterStatModifier(activeSkillDamageMultiplierBonus: 0.5f), skillMechanics: mechanics));
            var activation = capture.Activations[0];
            Assert.AreEqual(mechanics, activation.Mechanics);
            Assert.AreSame(junk.ProjectileSequence, activation.ProjectileSequence);
            // Passive damage (+50%) stays in the heavy explosion; only the set's own transform (+50%) is divided out.
            Assert.AreEqual(1.5f / 2f, activation.SetTransformDamageNormalization, 1e-5f);
        }

        private const string SkillId = "FIXTURE-SKILL-MECHANICS";

        private static ActiveSkillLevelDefinition Burst(IActiveSkillEffect effect, CombatControlProfile controls = null) =>
            new ActiveSkillLevelDefinition(10f, 1f, ActiveSkillTargetingMode.Self,
                new ActiveSkillActivationWave(0f, 0f, 1f, controls ?? CombatControlProfile.None, effect));

        private void Fire(ActiveSkillLevelDefinition level, SkillMechanicBonus mechanics, SkillProjectileSequence sequence = null,
            IEnemyDamageReceiver target = null, float damage = 10f, float setDamageNormalization = 1f, bool tick = true)
        {
            _executor.Schedule(new ActiveSkillActivation(SkillId, 1, Vector2.zero, Vector2.right, target, damage, level,
                _context.Owner.transform, hitLedger: new SkillHitLedger(), mechanics: mechanics,
                setTransformDamageNormalization: setDamageNormalization, projectileSequence: sequence));
            if (tick) _executor.Tick(0f, true);
        }
    }
}
