using System.Reflection;
using Game.Combat;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Presentation.Tests
{
    /// <summary>Duration bars for shield, picked-up experience and power sit under the speed bar and drain.</summary>
    public sealed class TimedEffectBarsTests
    {
        private GameObject _entity, _runObject;
        private Texture2D _texture;
        private Sprite _sprite;
        private SpritePresentationRuntime _presentation;
        private TimedEffectBarsPresentationRuntime _bars;
        private SlowStatusPresentationProfile _profile;

        [SetUp]
        public void SetUp()
        {
            _entity = new GameObject("Entity");
            var body = _entity.AddComponent<Rigidbody2D>(); body.gravityScale = 0f;
            var visual = new GameObject("VisualRoot"); visual.transform.SetParent(_entity.transform, false);
            var rig = visual.AddComponent<SpritePresentationRig>();
            _presentation = visual.AddComponent<SpritePresentationRuntime>();
            var bodyRoot = new GameObject("BodyRoot"); bodyRoot.transform.SetParent(visual.transform, false);
            var renderer = bodyRoot.AddComponent<SpriteRenderer>(); renderer.sortingOrder = 10;
            rig.Configure(bodyRoot.transform, renderer);
            _runObject = new GameObject("RunController");
            var run = _runObject.AddComponent<RunController>();
            if (run.Model == null)
                typeof(RunController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(run, null);
            _texture = new Texture2D(2, 2);
            _sprite = Sprite.Create(_texture, new Rect(0f, 0f, 2f, 2f), new Vector2(.5f, .5f));
            _presentation.Initialize(new SpriteDefinition("FIXTURE-VISUAL", _sprite), SpriteMotionProfileTests.CreateProfile(),
                new Health(new FixedHealthProfile(10f)), body, run);
            _bars = visual.AddComponent<TimedEffectBarsPresentationRuntime>();
            _profile = FixtureSlowStatusPresentationCatalog.Create();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_entity); Object.DestroyImmediate(_runObject);
            Object.DestroyImmediate(_sprite); Object.DestroyImmediate(_texture);
        }

        private Transform Row(int index) => _presentation.Rig.transform.Find("TimedEffectBars/Bar" + index);

        [Test]
        public void Apply_ShowsOnlyActiveBars_StackedDownward_FillMatchesRemaining()
        {
            _bars.Apply(_presentation, _profile, .5f, 0f, 1f, 0);
            Assert.AreEqual(2, _bars.ShowingCount);
            Assert.IsTrue(Row(0).Find("Fill").GetComponent<SpriteRenderer>().enabled);
            Assert.IsFalse(Row(1).Find("Fill").GetComponent<SpriteRenderer>().enabled, "No experience multiplier, no experience bar.");
            Assert.IsTrue(Row(2).Find("Fill").GetComponent<SpriteRenderer>().enabled);
            var back = Row(0).Find("Back").localScale.x;
            Assert.AreEqual(back * .5f, Row(0).Find("Fill").localScale.x, 1e-4f);
            Assert.AreEqual(back, Row(2).Find("Fill").localScale.x, 1e-4f);
            Assert.Less(Row(2).localPosition.y, Row(0).localPosition.y, "The second visible bar sits below the first.");
        }

        [Test]
        public void Apply_FirstRowBelowTheSpeedBar_AndZeroClearsEverything()
        {
            _bars.Apply(_presentation, _profile, 1f, 0f, 0f, 0);
            var top = Row(0).localPosition.y;
            _bars.Apply(_presentation, _profile, 1f, 0f, 0f, 1);
            Assert.Less(Row(0).localPosition.y, top, "With the speed bar showing, the first bar starts one row lower.");
            _bars.Apply(_presentation, _profile, 0f, 0f, 0f, 0);
            Assert.AreEqual(0, _bars.ShowingCount);
            Assert.IsFalse(_presentation.Rig.transform.Find("TimedEffectBars").gameObject.activeSelf);
        }
    }
}
