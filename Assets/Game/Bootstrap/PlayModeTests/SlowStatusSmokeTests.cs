using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Enemy;
using Game.Presentation;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Game.Bootstrap.PlayModeTests
{
    /// <summary>DECISION-0108: the development slow-look switcher on production enemy bodies.</summary>
    public sealed class SlowStatusSmokeTests
    {
        [UnityTest]
        public IEnumerator Production_SlowAllWithEachStyle_ShowsOnSlowedBodiesAndOffClears()
        {
            ProductionSmokeScene.Load(); yield return null; yield return null;
            var composition = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            CharacterSelectionSmokeDriver.StartDefault(composition); yield return null;
            var run = Object.FindAnyObjectByType<RunController>();
            var alive = new List<EnemyRuntime>();
            try
            {
                Assert.AreEqual(RunState.Running, run.Model.State);
                for (var frame = 0; frame < 600 && !alive.Any(e => e.BodyPresentation != null); frame++)
                {
                    yield return new WaitForFixedUpdate();
                    EnemyRegistry.CopyAliveTo(alive);
                }
                Assert.IsTrue(alive.Any(e => e.BodyPresentation != null), "Production enemies with body art spawned.");

                var director = composition.SlowStatus;
                Assert.AreEqual(SlowStatusStyle.Ice, director.Style, "Bar and ice are the production default.");
                Assert.Greater(director.SlowAllForPreview(), 0);
                yield return null;
                var selected = alive.First(e => e.BodyPresentation != null && e.Controls.IsSlowed)
                    .BodyPresentation.GetComponent<SlowStatusPresentationRuntime>();
                Assert.IsTrue(selected.IsShowing);
                Assert.IsTrue(selected.transform.Find("SlowBar").gameObject.activeSelf);
                Assert.IsTrue(selected.transform.Find("BodyRoot/SlowIce").GetComponent<SpriteRenderer>().enabled);
                director.SetStyle(SlowStatusStyle.All);
                yield return null;
                Assert.Greater(director.ShownCount, 0);
                var slowed = alive.First(e => e.BodyPresentation != null && e.Controls.IsSlowed);
                var overlay = slowed.BodyPresentation.GetComponent<SlowStatusPresentationRuntime>();
                Assert.IsTrue(overlay.IsShowing);
                Assert.AreEqual(SlowStatusStyle.All, overlay.AppliedStyle);
                Assert.AreNotEqual(Color.white, slowed.BodyPresentation.StatusTint);
                slowed.Protection.SetSpeedBoost(System.Guid.NewGuid(), .5f, run.Model.Elapsed + 5f, run.Model.Elapsed);
                yield return null;
                var speed = slowed.BodyPresentation.GetComponent<SpeedStatusPresentationRuntime>();
                Assert.IsNotNull(speed);
                Assert.IsTrue(speed.IsShowing);
                var slowBack = slowed.BodyPresentation.Rig.transform.Find("SlowBar/Back").GetComponent<SpriteRenderer>();
                var speedBack = slowed.BodyPresentation.Rig.transform.Find("SpeedBar/Back").GetComponent<SpriteRenderer>();
                Assert.AreEqual(slowBack.bounds.min.y, speedBack.bounds.max.y, 1e-3f);

                director.SetStyle(SlowStatusStyle.Off);
                yield return null;
                Assert.AreEqual(0, director.ShownCount);
                Assert.IsFalse(overlay.IsShowing);
                Assert.AreEqual(Color.white, slowed.BodyPresentation.StatusTint);
                Assert.IsTrue(slowed.Controls.IsSlowed, "The look switch never changes gameplay slow.");
                Assert.IsTrue(speed.IsShowing, "Hiding the slow preview never hides a real speed boost.");
            }
            finally { composition.Shutdown(); }
        }
    }
}
