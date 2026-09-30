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
        private SpeedStatusPresentationRuntime _speed;
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
            _speed = visual.AddComponent<SpeedStatusPresentationRuntime>();
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
            Assert.AreEqual(SpriteRole.Mask, _profile.IceMask.Role);
            Assert.AreEqual("SLOW-STATUS-VISUAL-MASK", _profile.IceMask.Id.ToString());
        }

        [Test]
        public void Map_MissingOrMalformedField_ThrowsByName()
        {
            var data = new SlowStatusPresentationProfileData
            {
                TintColor = new[] { 1f, 1f, 1f, 1f }, IceColor = new[] { 1f, 1f, 1f, 1f }, OutlineColor = new[] { 1f, 1f, 1f, 1f },
                OutlineWidth = .03f, BarWidth = .6f, BarHeight = .07f, BarOffsetY = .1f,
                BarFillColor = new[] { 1f, 1f, 1f, 1f }, BarBackColor = new[] { 0f, 0f, 0f, 1f },
                SpeedBarFillColor = new[] { 1f, .8f, .2f, 1f }, SpeedBoltColor = new[] { 1f, .9f, .4f, .8f },
                SpeedBoltScale = .24f, SpeedBlinkPeriod = .7f, PreviewSlowFraction = .4f
            };
            StringAssert.Contains("previewSlowSeconds", Assert.Throws<InvalidOperationException>(() => FixtureSlowStatusPresentationCatalog.Map(data)).Message);
            data.PreviewSlowSeconds = 5f;
            data.IceColor = new[] { 1f, 1f };
            StringAssert.Contains("iceColor", Assert.Throws<InvalidOperationException>(() => FixtureSlowStatusPresentationCatalog.Map(data)).Message);
            data.IceColor = new[] { 1f, 1f, 1f, 1f };
            StringAssert.Contains("iceVisualId", Assert.Throws<InvalidOperationException>(() => FixtureSlowStatusPresentationCatalog.Map(data)).Message);
            data.IceVisualId = "SLOW-STATUS-MISSING-MASK";
            StringAssert.Contains("not registered", Assert.Throws<InvalidOperationException>(() => FixtureSlowStatusPresentationCatalog.Map(data)).Message);
            data.IceVisualId = "ENEMY-001-VISUAL-BODY";
            StringAssert.Contains("expected Mask", Assert.Throws<InvalidOperationException>(() => FixtureSlowStatusPresentationCatalog.Map(data)).Message);
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
        public void SpeedBoost_BlinksAndStacksImmediatelyBelowSlowBar()
        {
            _slow.Apply(_presentation, _profile, SlowStatusStyle.Ice, true, .75f);
            _speed.Apply(_presentation, _profile, true, .5f, true, 0f);
            var slowBack = _presentation.Rig.transform.Find("SlowBar/Back").GetComponent<SpriteRenderer>();
            var speedBar = _presentation.Rig.transform.Find("SpeedBar");
            var speedBack = speedBar.Find("Back").GetComponent<SpriteRenderer>();
            var speedFill = speedBar.Find("Fill");
            var bolt = _presentation.Rig.BodyRoot.Find("SpeedBolt").GetComponent<SpriteRenderer>();
            Assert.IsTrue(_slow.IsShowing && _speed.IsShowing);
            Assert.AreEqual(slowBack.bounds.min.y, speedBack.bounds.max.y, 1e-4f,
                "The two bars meet without overlapping.");
            Assert.AreEqual(_profile.BarWidth * .5f, speedFill.localScale.x, 1e-4f);
            Assert.IsTrue(bolt.enabled);
            _speed.Apply(_presentation, _profile, true, .5f, true, _profile.SpeedBlinkPeriod * .5f);
            Assert.IsFalse(bolt.enabled, "The bolt flashes periodically using run time.");
            _speed.Apply(_presentation, _profile, true, .5f, true, _profile.SpeedBlinkPeriod * .5f);
            Assert.IsFalse(bolt.enabled, "A paused run keeps the bolt phase.");
            _slow.Clear();
            _speed.Apply(_presentation, _profile, true, .25f, false, 0f);
            Assert.IsFalse(_slow.IsShowing);
            Assert.IsTrue(_speed.IsShowing);
            Assert.AreEqual(_presentation.Rig.BodyRenderer.bounds.min.y - _profile.BarOffsetY,
                speedBack.bounds.max.y, 1e-4f, "Speed alone uses the top bar slot.");
            _speed.Clear();
            Assert.IsFalse(speedBar.gameObject.activeSelf);
            Assert.IsFalse(bolt.enabled);
        }

        [Test]
        public void Apply_IceOnCompactBody_ScalesWithSpriteAndBarClearsItsBottom()
        {
            var texture = new Texture2D(80, 112);
            var sprite = Sprite.Create(texture, new Rect(8f, 4f, 64f, 96f), new Vector2(.5f, .5f), 100f);
            try
            {
                var rig = _presentation.Rig;
                rig.BodyRenderer.sprite = sprite;
                rig.transform.localScale = new Vector3(.7f, .7f, 1f);
                rig.BodyRoot.localScale = new Vector3(.55f, .72f, 1f);

                _slow.Apply(_presentation, _profile, SlowStatusStyle.Ice, true, .5f);

                var ice = rig.BodyRoot.Find("SlowIce").GetComponent<SpriteRenderer>();
                Assert.AreSame(sprite, ice.sprite);
                Assert.Less(Vector3.Distance(rig.BodyRenderer.bounds.size, ice.bounds.size), 1e-4f,
                    "The ice covers the same scaled sprite rectangle as the compact body.");
                var properties = new MaterialPropertyBlock();
                ice.GetPropertyBlock(properties);
                Assert.Less(Vector4.Distance(new Vector4(80f / 64f, 112f / 96f, -8f / 64f, -4f / 96f),
                    properties.GetVector("_IceUvTransform")), 1e-4f,
                    "The shared mask is remapped to this body's sprite rectangle.");
                var barBack = rig.transform.Find("SlowBar/Back").GetComponent<SpriteRenderer>();
                Assert.LessOrEqual(barBack.bounds.max.y,
                    rig.BodyRenderer.bounds.min.y - _profile.BarOffsetY + 1e-4f,
                    "The bar sits below the body and cannot cover its ice.");
            }
            finally
            {
                Object.DestroyImmediate(sprite);
                Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void Apply_All_MirrorsBodyForIceAndOutlineThenOffClears()
        {
            // Facing is mirrored through the overlay transform: a silhouette shader never sees SpriteRenderer.flipX.
            _presentation.Rig.BodyRenderer.flipX = true;
            _slow.Apply(_presentation, _profile, SlowStatusStyle.Ice, true, 1f);
            var ice = _presentation.Rig.BodyRoot.Find("SlowIce").GetComponent<SpriteRenderer>();
            Assert.IsTrue(_presentation.Rig.transform.Find("SlowBar").gameObject.activeSelf,
                "The selected ice look includes the duration bar.");
            Assert.IsFalse(ice.flipX);
            Assert.AreEqual(-1f, ice.transform.localScale.x);
            _presentation.Rig.BodyRenderer.flipX = false;
            _slow.Apply(_presentation, _profile, SlowStatusStyle.Ice, true, 1f);
            Assert.AreEqual(1f, ice.transform.localScale.x, "The overlay turns with the monster.");

            _slow.Apply(_presentation, _profile, SlowStatusStyle.All, true, 1f);
            Assert.IsTrue(ice.enabled);
            Assert.AreSame(_sprite, ice.sprite);
            Assert.AreEqual(_presentation.Rig.BodyRenderer.flipX ? -1f : 1f, ice.transform.localScale.x);
            Assert.AreEqual(11, ice.sortingOrder);
            Assert.AreEqual("SurvivorArena/SpriteIceMask", ice.sharedMaterial.shader.name);
            Assert.Less(Vector4.Distance(_profile.IceColor, ice.sharedMaterial.GetColor("_Color")), 1e-5f,
                "Color comes from the material, not SpriteRenderer.color.");
            Assert.AreSame(_profile.IceMask.Sprite.texture,
                ice.sharedMaterial.GetTexture("_IceTex"), "All bodies share one imported texture.");
            var outline = Overlays().Where(r => r.name.StartsWith("SlowOutline", StringComparison.Ordinal)).ToArray();
            Assert.AreEqual(8, outline.Length);
            Assert.IsTrue(outline.All(r => r.enabled && r.sortingOrder == 9 && r.sprite == _sprite));
            Assert.IsTrue(outline.All(r => r.sharedMaterial.GetColor("_Color") == _profile.OutlineColor));
            Assert.Less(_profile.OutlineColor.a, 1f, "Outline is translucent.");
            var worldWidth = outline.Max(r => r.transform.localPosition.x) * Mathf.Abs(_presentation.Rig.BodyRoot.lossyScale.x);
            Assert.AreEqual(_profile.OutlineWidth, worldWidth, 1e-4f, "Outline width is in world units despite body pose scale.");
            var bar = _presentation.Rig.transform.Find("SlowBar");
            Assert.IsTrue(bar.gameObject.activeSelf, "The selected ice look includes the duration bar.");
            Assert.Less(Vector4.Distance(_profile.BarFillColor,
                bar.Find("Fill").GetComponent<SpriteRenderer>().sharedMaterial.GetColor("_Color")), 1e-5f);
            Assert.IsTrue(bar.Find("Fill").GetComponent<SpriteRenderer>().enabled);

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
