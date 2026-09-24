using System.Collections;
using System.Linq;
using Game.Enemy;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Bootstrap.PlayModeTests
{
    /// <summary>F1-08: a new production profile starts Klepka on FIELD-001 with production content only.</summary>
    public sealed class ProductionRunSmokeTests
    {
        [UnityTest]
        public IEnumerator NewProductionProfile_StartsField001_WithAuthoredObstaclesAndProductionContent()
        {
            ProductionSmokeScene.Load();
            yield return null;
            yield return null;
            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            CharacterSelectionSmokeDriver.StartDefault(root);
            yield return null;
            try
            {
                Assert.IsTrue(root.Catalog.IsProduction);
                Assert.IsFalse(root.Catalog.BuildEntries.Any(e => e.Id.ToString().StartsWith("FIXTURE-")), "No fixture build entries.");
                Assert.IsFalse(root.Catalog.Enemies.Any(e => e.Id.ToString().StartsWith("FIXTURE-")), "No fixture enemies.");
                var run = Object.FindAnyObjectByType<RunController>();
                Assert.AreEqual("CHAR-001", run.Model.Selection.CharacterId.ToString());
                Assert.AreEqual("FIELD-001", run.Model.Selection.FieldId.ToString());
                var fieldArt = GameObject.Find("FieldEnvironmentArt");
                Assert.AreEqual(64, fieldArt.GetComponentsInChildren<Collider2D>().Length);
                Assert.IsFalse(GameObject.Find("Obstacle_Fixture").GetComponent<Collider2D>().enabled,
                    "The prototype scene obstacle is not part of the authored FIELD-001 layout.");
                for (var i = 0; i < 180; i++) yield return new WaitForFixedUpdate();
                Assert.AreEqual(RunState.Running, run.Model.State);
                foreach (var enemy in Object.FindObjectsByType<EnemyRuntime>(FindObjectsSortMode.None))
                    StringAssert.StartsWith("ENEMY-", enemy.ContentId.ToString());
            }
            finally
            {
                if (root != null && root.IsInitialized) root.Shutdown();
            }
            LogAssert.NoUnexpectedReceived();
        }
    }
}
