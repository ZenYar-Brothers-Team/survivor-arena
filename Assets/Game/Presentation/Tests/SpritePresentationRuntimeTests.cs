using System.Reflection;
using Game.Combat;
using Game.Run;
using NUnit.Framework;
using UnityEngine;

namespace Game.Presentation.Tests
{
    public sealed class SpritePresentationRuntimeTests
    {
        private GameObject _entity;
        private GameObject _visualRootObject;
        private GameObject _bodyRootObject;
        private GameObject _shadowObject;
        private GameObject _runObject;
        private Texture2D _texture;
        private Sprite _sprite;

        [TearDown]
        public void TearDown()
        {
            if (_entity != null)
                Object.DestroyImmediate(_entity);
            if (_runObject != null)
                Object.DestroyImmediate(_runObject);
            if (_sprite != null)
                Object.DestroyImmediate(_sprite);
            if (_texture != null)
                Object.DestroyImmediate(_texture);
        }

        [Test]
        public void Tick_ChangesOnlyBodyPoseAndShutdownRestoresBaseline()
        {
            var runtime = CreateRuntime(out var rootBody, out var collider, out var health, out var runController);
            var originalRootPosition = _entity.transform.position;
            var originalRootScale = _entity.transform.localScale;
            var originalColliderSize = collider.size;
            var originalBodyPosition = rootBody.localPosition;
            var originalBodyRotation = rootBody.localRotation;
            var initializedBodyScale = rootBody.localScale;

            runController.Model.Start();
            _entity.GetComponent<Rigidbody2D>().linearVelocity = Vector2.right * 3f;
            runtime.Tick(0.025f);
            health.TakeDamage(2f);

            Assert.AreEqual(originalRootPosition, _entity.transform.position);
            Assert.AreEqual(originalRootScale, _entity.transform.localScale);
            Assert.AreEqual(originalColliderSize, collider.size);
            Assert.AreNotEqual(originalBodyPosition, rootBody.localPosition);
            Assert.AreNotEqual(initializedBodyScale, rootBody.localScale);

            runtime.Shutdown();

            Assert.AreEqual(originalBodyPosition, rootBody.localPosition);
            Assert.AreEqual(originalBodyRotation, rootBody.localRotation);
            Assert.AreEqual(Vector3.one, rootBody.localScale);
            Assert.IsNull(rootBody.GetComponent<SpriteRenderer>().sprite);
            Assert.IsFalse(runtime.IsInitialized);
        }

        [Test]
        public void Pause_FreezesBodyPoseAndRepeatedInitializeResetsPreviousLife()
        {
            var runtime = CreateRuntime(out var rootBody, out _, out var health, out var runController);
            runController.Model.Start();
            _entity.GetComponent<Rigidbody2D>().linearVelocity = Vector2.left * 3f;
            runtime.Tick(0.025f);
            health.TakeDamage(1f);
            var pausedPosition = rootBody.localPosition;
            var pausedRotation = rootBody.localRotation;
            var pausedScale = rootBody.localScale;

            runController.Model.Pause();
            _entity.GetComponent<Rigidbody2D>().linearVelocity = Vector2.right * 3f;
            runtime.Tick(1f);

            Assert.AreEqual(pausedPosition, rootBody.localPosition);
            Assert.AreEqual(pausedRotation, rootBody.localRotation);
            Assert.AreEqual(pausedScale, rootBody.localScale);

            runtime.Initialize(
                new SpriteDefinition("FIXTURE-VISUAL-SECOND-LIFE", _sprite),
                SpriteMotionProfileTests.CreateProfile(),
                new Health(new FixedHealthProfile(10f)),
                _entity.GetComponent<Rigidbody2D>(),
                runController);

            Assert.IsFalse(rootBody.GetComponent<SpriteRenderer>().flipX);
            Assert.AreEqual(new Vector3(0.75f, 0.75f, 1f), rootBody.localScale);
        }

        [Test]
        public void PreviewMotion_OverridesVelocityWithoutChangingGameplayBody_AndResetReturnsToLive()
        {
            var runtime = CreateRuntime(out var rootBody, out _, out _, out var runController);
            runController.Model.Start();
            var rigidbody = _entity.GetComponent<Rigidbody2D>();
            rigidbody.linearVelocity = Vector2.right * 0.5f;

            runtime.SetPreviewMotion(SpritePresentationPreviewMotion.Left);
            runtime.Tick(0.04f);

            Assert.AreEqual(SpritePresentationPreviewMotion.Left, runtime.PreviewMotion);
            Assert.IsTrue(rootBody.GetComponent<SpriteRenderer>().flipX);
            Assert.AreEqual(Vector2.right * 0.5f, rigidbody.linearVelocity);

            runtime.SetPreviewMotion(SpritePresentationPreviewMotion.Idle);
            runtime.Tick(0.04f);
            Assert.AreNotEqual(Vector3.zero, rootBody.localPosition);
            Assert.AreNotEqual(Vector3.one, rootBody.localScale);

            runtime.ResetPresentation();
            Assert.AreEqual(SpritePresentationPreviewMotion.Live, runtime.PreviewMotion);
        }

        [TestCase(RunState.Won)]
        [TestCase(RunState.Lost)]
        public void EndedRun_FreezesPresentationPose(RunState endState)
        {
            var runtime = CreateRuntime(out var rootBody, out _, out _, out var runController);
            runController.Model.Start();
            var rigidbody = _entity.GetComponent<Rigidbody2D>();
            rigidbody.linearVelocity = new Vector2(1f, 1f).normalized * 3f;
            runtime.Tick(0.04f);
            var endedPosition = rootBody.localPosition;
            var endedRotation = rootBody.localRotation;
            var endedScale = rootBody.localScale;

            if (endState == RunState.Won)
                runController.Model.Tick(runController.Model.Duration);
            else
                runController.Model.Kill();

            rigidbody.linearVelocity = Vector2.left * 3f;
            runtime.Tick(1f);

            Assert.AreEqual(endState, runController.Model.State);
            Assert.AreEqual(endedPosition, rootBody.localPosition);
            Assert.AreEqual(endedRotation, rootBody.localRotation);
            Assert.AreEqual(endedScale, rootBody.localScale);
        }

        private SpritePresentationRuntime CreateRuntime(
            out Transform bodyRoot,
            out BoxCollider2D collider,
            out Health health,
            out RunController runController)
        {
            _entity = new GameObject("Entity");
            var rigidbody = _entity.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0f;
            collider = _entity.AddComponent<BoxCollider2D>();

            _visualRootObject = new GameObject("VisualRoot");
            _visualRootObject.transform.SetParent(_entity.transform, false);
            var rig = _visualRootObject.AddComponent<SpritePresentationRig>();
            var runtime = _visualRootObject.AddComponent<SpritePresentationRuntime>();

            _bodyRootObject = new GameObject("BodyRoot");
            _bodyRootObject.transform.SetParent(_visualRootObject.transform, false);
            var bodyRenderer = _bodyRootObject.AddComponent<SpriteRenderer>();

            _shadowObject = new GameObject("ShadowRenderer");
            _shadowObject.transform.SetParent(_visualRootObject.transform, false);
            var shadowRenderer = _shadowObject.AddComponent<SpriteRenderer>();
            rig.Configure(_bodyRootObject.transform, bodyRenderer, shadowRenderer);

            _runObject = new GameObject("RunController");
            runController = _runObject.AddComponent<RunController>();
            if (runController.Model == null)
                InvokeAwake(runController);

            _texture = new Texture2D(2, 2);
            _sprite = Sprite.Create(_texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f));
            health = new Health(new FixedHealthProfile(10f));
            runtime.Initialize(
                new SpriteDefinition("FIXTURE-VISUAL", _sprite),
                SpriteMotionProfileTests.CreateProfile(),
                health,
                rigidbody,
                runController);

            bodyRoot = _bodyRootObject.transform;
            return runtime;
        }

        private static void InvokeAwake(MonoBehaviour behaviour)
        {
            behaviour.GetType()
                .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(behaviour, null);
        }
    }
}
