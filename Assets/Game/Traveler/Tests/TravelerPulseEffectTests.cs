using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Game.Traveler.Tests
{
    /// <summary>DECISION-0123: the procedural aura outlines exist, animate and finish (a steady one never does).</summary>
    public sealed class TravelerPulseEffectTests
    {
        private TravelerPulseEffect _effect;
        [SetUp] public void SetUp() => _effect = TravelerPulseEffect.CreateInstance();
        [TearDown] public void TearDown() => Object.DestroyImmediate(_effect.gameObject);
        [TestCase(TravelerEffectShape.Ring)] [TestCase(TravelerEffectShape.Hexagon)] [TestCase(TravelerEffectShape.Ripples)] [TestCase(TravelerEffectShape.Dots)]
        public void Pulse_FinishesAfterItsDuration_AndDrawsSomething(TravelerEffectShape shape)
        {
            _effect.PlayPulse(Vector2.one, 6f, Color.white, .5f, .8f, shape);
            Assert.IsFalse(_effect.IsFinished);
            _effect.Tick(.25f);
            var any = false;
            foreach (var renderer in _effect.GetComponentsInChildren<SpriteRenderer>()) any |= renderer.enabled && renderer.color.a > 0f;
            Assert.IsTrue(any, "Something is visible mid-pulse.");
            Assert.AreEqual(6f * .8f, _effect.transform.Find("Flattened").localScale.y, 1e-4f, "Flattened by the vertical scale.");
            _effect.Tick(.3f);
            Assert.IsTrue(_effect.IsFinished);
        }
        [Test]
        public void SteadyOutline_NeverFinishes_AndFollowsItsTarget()
        {
            var target = new GameObject("target");
            _effect.ShowSteady(target.transform, 6f, Color.white, .8f, TravelerEffectShape.Hexagon);
            target.transform.position = new Vector3(3, 2, 0);
            _effect.Tick(100f);
            Assert.IsFalse(_effect.IsFinished);
            Assert.AreEqual(new Vector3(3, 2, 0), _effect.transform.position);
            Object.DestroyImmediate(target);
        }
        [Test]
        public void ShapeSprites_HaveAnOutlineAndAnEmptyCenter()
        {
            Assert.IsNotNull(TravelerShapeSprites.Hexagon); Assert.IsNotNull(TravelerShapeSprites.Dots); Assert.IsNotNull(TravelerShapeSprites.Lotus);
            Assert.AreEqual(0f, TravelerShapeSprites.HexagonAlpha(0f, 0f), 1e-4f);
            Assert.AreEqual(0f, TravelerShapeSprites.DotsAlpha(0f, 0f), 1e-4f);
            Assert.AreEqual(0f, TravelerShapeSprites.WardAlpha(0f, 0f), 1e-4f);
            Assert.AreEqual(0f, TravelerShapeSprites.LotusAlpha(0f, 0f), 1e-4f);
            // an edge midpoint of the flat-top hexagon (normal at 30 degrees, apothem 0.433) and the first dot at angle 0
            var edge = .433f - .02f;
            Assert.Greater(TravelerShapeSprites.HexagonAlpha(edge * Mathf.Cos(30 * Mathf.Deg2Rad), edge * Mathf.Sin(30 * Mathf.Deg2Rad)), .9f);
            Assert.AreEqual(0f, TravelerShapeSprites.HexagonAlpha(.3f, .4f), 1e-4f, "Outside the hexagon.");
            Assert.Greater(TravelerShapeSprites.DotsAlpha(.44f, 0f), .9f);
            Assert.Greater(TravelerShapeSprites.WardAlpha(.455f, 0f), .9f, "Ward rune on the outer radius.");
            Assert.Greater(TravelerShapeSprites.LotusAlpha(.475f, 0f), .9f, "Lotus petal at the radius.");
            Assert.AreEqual(0f, TravelerShapeSprites.LotusAlpha(.2f, 0f), 1e-4f, "No healing ornament over the actor.");
        }
        [Test]
        public void SteadyWard_BreathesOnlyInsideRadius_AndFreezesWithoutTick()
        {
            var target = new GameObject("target");
            try
            {
                _effect.ShowSteady(target.transform, 6f, Color.white, .8f, TravelerEffectShape.Hexagon);
                var main = _effect.transform.Find("Flattened/Main");
                var initial = main.localScale;
                _effect.Tick(.8f);
                Assert.AreNotEqual(initial, main.localScale);
                Assert.LessOrEqual(main.localScale.x, 1f);
                Assert.GreaterOrEqual(main.localScale.x, .95f);
                var frozen = main.localScale;
                var angle = main.localRotation;
                _effect.Tick(0f);
                Assert.AreEqual(frozen, main.localScale, "Paused presentation does not breathe.");
                Assert.AreEqual(angle, main.localRotation, "Paused presentation does not rotate.");
            }
            finally { Object.DestroyImmediate(target); }
        }
        [TestCase(TravelerEffectShape.Hexagon)]
        [TestCase(TravelerEffectShape.Dots)]
        [TestCase(TravelerEffectShape.Ripples)]
        public void ApprovedAuraArt_RotatesAsCircle_BeforeSingleGroundFlattening(TravelerEffectShape shape)
        {
            _effect.PlayPulse(Vector2.zero, 6f, Color.white, .5f, .8f, shape);
            var root = _effect.transform.Find("Flattened");
            var main = root.Find("Main").GetComponent<SpriteRenderer>();
            var expected = shape == TravelerEffectShape.Hexagon ? TravelerShapeSprites.ShieldArt :
                shape == TravelerEffectShape.Dots ? TravelerShapeSprites.SpeedArt : TravelerShapeSprites.HealArt;
            Assert.AreSame(expected, main.sprite);
            Assert.AreEqual(1f, main.sprite.bounds.size.x, 1e-4f);
            Assert.AreEqual(main.sprite.bounds.size.x, main.sprite.bounds.size.y, 1e-4f,
                "The approved art is circular before its shared runtime flattening.");
            Assert.AreEqual(.8f, root.localScale.y / root.localScale.x, 1e-4f);
            Assert.AreEqual(main.transform.localScale.x, main.transform.localScale.y, 1e-4f,
                "No inverse or second vertical scale belongs on the rotating image.");
            _effect.Tick(.2f);
            Assert.AreNotEqual(Quaternion.identity, main.transform.localRotation);
        }
    }
}
