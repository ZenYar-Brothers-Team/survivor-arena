using System.Linq;
using Game.Content;
using Game.Pooling;
using Game.Presentation;
using Game.Run;
using NUnit.Framework;
using UnityEngine;

namespace Game.Enemy.Tests
{
    public sealed class EnemyBodyPresentationTests
    {
        private GameObject _root;
        private RunController _run;
        private EnemyDefinition _definition;
        private SpriteDefinition _sprite;
        private SpriteMotionProfile _motion;
        private GameObjectPool<EnemyRuntime> _pool;
        private GroundShadowPresentationProfile _shadow;
        private int _baseline;

        [SetUp]
        public void SetUp()
        {
            _baseline = EnemyRegistry.Count;
            _root = new GameObject("Enemy body test");
            _run = _root.AddComponent<RunController>();
            if (!_run.IsInitialized) _run.Initialize();
            _run.Model.Start();
            _definition = FixtureEnemyCatalog.Create().First();
            _sprite = FixtureSpriteCatalog.CreateFor(new[] { _definition.Visual.Id }).Single();
            _motion = FixtureSpriteMotionProfileCatalog.Create().Single(p => p.Id == _definition.MotionProfile.Id);
            _pool = new GameObjectPool<EnemyRuntime>(EnemyFactory.CreateInstance, _root.transform);
            _shadow = FixtureGroundShadowPresentationCatalog.Create();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Assert.AreEqual(_baseline, EnemyRegistry.Count);
        }

        private EnemyRuntime Spawn() => EnemyFactory.Spawn(_definition, new Vector2(2, 3),
            _root.transform, _run, _root.transform, _sprite.Sprite, _pool, motionProfile: _motion,
            contact: _sprite.Contact, groundShadowPresentation: _shadow);

        [Test]
        public void Body_MovesAndFlashesOnlyChild_AndFreezesOnPause()
        {
            var enemy = Spawn();
            var rig = enemy.GetComponentInChildren<SpritePresentationRig>();
            var presentation = rig.GetComponent<SpritePresentationRuntime>();
            var rootPosition = enemy.transform.position;
            var rootScale = enemy.transform.localScale;
            var colliderRadius = enemy.GetComponent<CircleCollider2D>().radius;
            Assert.IsFalse(enemy.GetComponent<SpriteRenderer>().enabled);
            Assert.IsTrue(enemy.GetComponent<GroundShadowRuntime>().Renderer.enabled);
            Assert.AreEqual(Color.white, rig.BodyRenderer.color);
            Assert.IsEmpty(rig.GetComponentsInChildren<Collider2D>());
            enemy.GetComponent<Rigidbody2D>().linearVelocity = Vector2.left;
            presentation.Tick(.3f);
            Assert.IsTrue(rig.BodyRenderer.flipX);
            enemy.TakeDamage(1);
            Assert.AreNotEqual(Color.white, rig.BodyRenderer.color);
            var pose = rig.BodyRoot.localPosition;
            var scale = rig.BodyRoot.localScale;
            var tint = rig.BodyRenderer.color;
            _run.TogglePause();
            presentation.Tick(1);
            Assert.AreEqual(pose, rig.BodyRoot.localPosition);
            Assert.AreEqual(scale, rig.BodyRoot.localScale);
            Assert.AreEqual(tint, rig.BodyRenderer.color);
            Assert.AreEqual(rootPosition, enemy.transform.position);
            Assert.AreEqual(rootScale, enemy.transform.localScale);
            Assert.AreEqual(colliderRadius, enemy.GetComponent<CircleCollider2D>().radius);
        }

        [Test]
        public void Body_DeathAndMixedPoolReuse_ClearOldSpritePoseAndSubscription()
        {
            var enemy = Spawn();
            var rig = enemy.GetComponentInChildren<SpritePresentationRig>();
            var presentation = rig.GetComponent<SpritePresentationRuntime>();
            enemy.GetComponent<Rigidbody2D>().linearVelocity = Vector2.left;
            presentation.Tick(.3f);
            enemy.TakeDamage(1);
            enemy.TakeDamage(100);
            Assert.IsFalse(enemy.gameObject.activeSelf);
            Assert.IsFalse(presentation.IsInitialized);
            Assert.IsNull(rig.BodyRenderer.sprite);
            Assert.AreEqual(Color.white, rig.BodyRenderer.color);
            Assert.IsFalse(rig.BodyRenderer.flipX);
            var plain = FixtureEnemyCatalog.Create()[1];
            var plainEnemy = EnemyFactory.Spawn(plain, Vector2.zero, _root.transform, _run,
                _root.transform, pool: _pool, groundShadowPresentation: _shadow);
            Assert.AreSame(enemy, plainEnemy);
            Assert.IsTrue(enemy.GetComponent<SpriteRenderer>().enabled);
            Assert.IsFalse(rig.gameObject.activeSelf);
            Assert.AreEqual(.5f, enemy.GetComponent<CircleCollider2D>().radius);
            Assert.AreEqual(_shadow.FallbackWidth,
                enemy.GetComponent<GroundShadowRuntime>().Renderer.transform.localScale.x, .0001f);
            plainEnemy.Despawn();
            var reused = Spawn();
            Assert.AreSame(enemy, reused);
            Assert.AreEqual(1, reused.GetComponentsInChildren<SpritePresentationRig>(true).Length);
            Assert.AreSame(_sprite.Sprite, rig.BodyRenderer.sprite);
            Assert.IsFalse(rig.BodyRenderer.flipX);
            Assert.AreEqual(Color.white, rig.BodyRenderer.color);
            Assert.AreEqual(_definition.MaxHealth, reused.Health.CurrentHealth);
            Assert.AreEqual(_sprite.Contact.Radius, reused.GetComponent<CircleCollider2D>().radius);
            Assert.IsTrue(reused.GetComponent<GroundShadowRuntime>().Renderer.enabled);
        }

        [Test]
        public void MotionReference_IsValidatedAndSurvivesWaveScaling()
        {
            var scaled = WaveEnemyScaler.Apply(_definition, new WaveEnemyModifiers(2, 1.5f));
            Assert.AreEqual(_definition.MotionProfile.Id, scaled.MotionProfile.Id);
            Assert.AreEqual(_definition.Visual.Id, scaled.Visual.Id);
            Assert.Throws<ContentValidationException>(() => ContentRegistry.BuildFrom(
                new IContentDefinition[] { _definition, _sprite }));
            var registry = ContentRegistry.BuildFrom(new IContentDefinition[] { _definition, _sprite, _motion });
            Assert.AreSame(_motion, _definition.MotionProfile.Resolve(registry));
            Assert.AreEqual("FIXTURE-ENEMY-SEEKER", _definition.Id.ToString());
        }
    }
}
