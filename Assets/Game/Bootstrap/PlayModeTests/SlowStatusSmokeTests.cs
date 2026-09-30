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

                director.SetStyle(SlowStatusStyle.Off);
                yield return null;
                Assert.AreEqual(0, director.ShownCount);
                Assert.IsFalse(overlay.IsShowing);
                Assert.AreEqual(Color.white, slowed.BodyPresentation.StatusTint);
                Assert.IsTrue(slowed.Controls.IsSlowed, "The look switch never changes gameplay slow.");
            }
            finally { composition.Shutdown(); }
        }
    }
}
