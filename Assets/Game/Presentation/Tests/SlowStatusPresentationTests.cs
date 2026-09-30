using System;
using System.Linq;
using System.Reflection;
using Game.Combat;
using Game.Presentation.Json;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Presentation.Tests
{
    // DECISION-0108: four development-selectable slow looks on the shared pose writer.
    public sealed class SlowStatusPresentationTests
    {
        private GameObject _entity;
        private GameObject _runObject;
        private Texture2D _texture;
        private Sprite _sprite;
        private SpritePresentationRuntime _presentation;
        private SlowStatusPresentationRuntime _slow;
        private SlowStatusPresentationProfile _profile;

        [SetUp]
        public void SetUp()
        {
            _entity = new GameObject("Entity");
            var body = _entity.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            var visual = new GameObject("VisualRoot");
            visual.transform.SetParent(_entity.transform, false);
            var rig = visual.AddComponent<SpritePresentationRig>();
            _presentation = visual.AddComponent<SpritePresentationRuntime>();
            var bodyRoot = new GameObject("BodyRoot");
            bodyRoot.transform.SetParent(visual.transform, false);
            var renderer = bodyRoot.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 10;
            rig.Configure(bodyRoot.transform, renderer);
            _runObject = new GameObject("RunController");
            var run = _runObject.AddComponent<RunController>();
            if (run.Model == null)
                typeof(RunController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(run, null);
            _texture = new Texture2D(2, 2);
            _sprite = Sprite.Create(_texture, new Rect(0f, 0f, 2f, 2f), new Vector2(.5f, .5f));
            _presentation.Initialize(new SpriteDefinition("FIXTURE-VISUAL", _sprite), SpriteMotionProfileTests.CreateProfile(),
                new Health(new FixedHealthProfile(10f)), body, run);
            _slow = visual.AddComponent<SlowStatusPresentationRuntime>();
            _profile = FixtureSlowStatusPresentationCatalog.Create();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_entity);
            Object.DestroyImmediate(_runObject);
            Object.DestroyImmediate(_sprite);
            Object.DestroyImmediate(_texture);
        }

        [Test]
        public void Catalog_FixtureJson_LoadsValidatedProfile()
        {
            Assert.AreEqual(0.035f, _profile.OutlineWidth, 1e-5f);
            Assert.AreEqual(5f, _profile.PreviewSlowSeconds, 1e-5f);
            Assert.AreEqual(0.4f, _profile.PreviewSlowFraction, 1e-5f);
        }

        [Test]
        public void Map_MissingOrMalformedField_ThrowsByName()
        {
            var data = new SlowStatusPresentationProfileData
            {
                TintColor = new[] { 1f, 1f, 1f, 1f }, IceColor = new[] { 1f, 1f, 1f, 1f }, OutlineColor = new[] { 1f, 1f, 1f, 1f },
                OutlineWidth = .03f, BarWidth = .6f, BarHeight = .07f, BarOffsetY = .1f,
                BarFillColor = new[] { 1f, 1f, 1f, 1f }, BarBackColor = new[] { 0f, 0f, 0f, 1f }, PreviewSlowFraction = .4f
            };
            StringAssert.Contains("previewSlowSeconds", Assert.Throws<InvalidOperationException>(() => FixtureSlowStatusPresentationCatalog.Map(data)).Message);
            data.PreviewSlowSeconds = 5f;
            data.IceColor = new[] { 1f, 1f };
            StringAssert.Contains("iceColor", Assert.Throws<InvalidOperationException>(() => FixtureSlowStatusPresentationCatalog.Map(data)).Message);
        }

        [Test]
        public void Apply_Tint_MultipliesBodyColorAndClearRestoresWhite()
        {
            _slow.Apply(_presentation, _profile, SlowStatusStyle.Tint, true, 1f);

            Assert.AreEqual(_profile.TintColor, _presentation.StatusTint);
            Assert.AreEqual(_profile.TintColor, _presentation.Rig.BodyRenderer.color);
            Assert.IsFalse(Overlays().Any(r => r.enabled));
            _slow.Apply(_presentation, _profile, SlowStatusStyle.Tint, false, 0f);
            Assert.AreEqual(Color.white, _presentation.Rig.BodyRenderer.color);
            Assert.IsFalse(_slow.IsShowing);
        }

        [Test]
        public void Apply_Bar_FillShrinksWithRemainingDuration()
        {
            _slow.Apply(_presentation, _profile, SlowStatusStyle.Bar, true, .5f);

            var bar = _presentation.Rig.transform.Find("SlowBar");
            Assert.IsTrue(bar.gameObject.activeSelf);
            var back = bar.Find("Back");
            var fill = bar.Find("Fill");
            Assert.AreEqual(_profile.BarWidth, back.localScale.x, 1e-4f);
            Assert.AreEqual(_profile.BarWidth * .5f, fill.localScale.x, 1e-4f);
            Assert.AreEqual(-_profile.BarWidth * .25f, fill.localPosition.x, 1e-4f, "The fill stays left-aligned.");
            Assert.AreEqual(Color.white, _presentation.StatusTint);
            _slow.Apply(_presentation, _profile, SlowStatusStyle.Bar, true, .1f);
            Assert.AreEqual(_profile.BarWidth * .1f, fill.localScale.x, 1e-4f);
        }

        [Test]
        public void Apply_All_MirrorsBodyForIceAndOutlineThenOffClears()
        {
            // Ice alone does not re-run the pose writer, so an externally set facing is mirrored as-is.
            _presentation.Rig.BodyRenderer.flipX = true;
            _slow.Apply(_presentation, _profile, SlowStatusStyle.Ice, true, 1f);
            var ice = _presentation.Rig.BodyRoot.Find("SlowIce").GetComponent<SpriteRenderer>();
            Assert.IsTrue(ice.flipX);

            _slow.Apply(_presentation, _profile, SlowStatusStyle.All, true, 1f);
            Assert.IsTrue(ice.enabled);
            Assert.AreSame(_sprite, ice.sprite);
            Assert.AreEqual(_presentation.Rig.BodyRenderer.flipX, ice.flipX, "Overlay follows the pose writer's facing.");
            Assert.AreEqual(11, ice.sortingOrder);
            Assert.AreEqual(_profile.IceColor, ice.color);
            var outline = Overlays().Where(r => r.name.StartsWith("SlowOutline", StringComparison.Ordinal)).ToArray();
            Assert.AreEqual(8, outline.Length);
            Assert.IsTrue(outline.All(r => r.enabled && r.sortingOrder == 9 && r.sprite == _sprite));
            var worldWidth = outline.Max(r => r.transform.localPosition.x) * Mathf.Abs(_presentation.Rig.BodyRoot.lossyScale.x);
            Assert.AreEqual(_profile.OutlineWidth, worldWidth, 1e-4f, "Outline width is in world units despite body pose scale.");
            Assert.AreEqual("SurvivorArena/SpriteSolidColor", ice.sharedMaterial.shader.name);

            _slow.Apply(_presentation, _profile, SlowStatusStyle.Off, true, 1f);
            Assert.IsFalse(Overlays().Any(r => r.enabled));
            Assert.IsFalse(_presentation.Rig.transform.Find("SlowBar").gameObject.activeSelf);
            Assert.AreEqual(Color.white, _presentation.Rig.BodyRenderer.color);
        }

        [Test]
        public void PresentationShutdown_ResetsStatusTint()
        {
            _slow.Apply(_presentation, _profile, SlowStatusStyle.Tint, true, 1f);
            _presentation.Shutdown();
            Assert.AreEqual(Color.white, _presentation.StatusTint);
            Assert.AreEqual(Color.white, _presentation.Rig.BodyRenderer.color);
        }

        private SpriteRenderer[] Overlays() => _presentation.Rig.BodyRoot.GetComponentsInChildren<SpriteRenderer>(true)
            .Where(r => r != _presentation.Rig.BodyRenderer).ToArray();
    }
}
