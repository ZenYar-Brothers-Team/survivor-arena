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
            Assert.IsNotNull(TravelerShapeSprites.Hexagon); Assert.IsNotNull(TravelerShapeSprites.Dots);
            Assert.AreEqual(0f, TravelerShapeSprites.HexagonAlpha(0f, 0f), 1e-4f);
            Assert.AreEqual(0f, TravelerShapeSprites.DotsAlpha(0f, 0f), 1e-4f);
            // an edge midpoint of the flat-top hexagon (normal at 30 degrees, apothem 0.433) and the first dot at angle 0
            var edge = .433f - .02f;
            Assert.Greater(TravelerShapeSprites.HexagonAlpha(edge * Mathf.Cos(30 * Mathf.Deg2Rad), edge * Mathf.Sin(30 * Mathf.Deg2Rad)), .9f);
            Assert.AreEqual(0f, TravelerShapeSprites.HexagonAlpha(.3f, .4f), 1e-4f, "Outside the hexagon.");
            Assert.Greater(TravelerShapeSprites.DotsAlpha(.44f, 0f), .9f);
        }
    }
}
