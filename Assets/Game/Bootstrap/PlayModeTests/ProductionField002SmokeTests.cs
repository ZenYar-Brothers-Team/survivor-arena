using System.Collections;
using System.Linq;
using Game.Content;
using Game.Enemy;
using Game.Meta;
using Game.Presentation;
using Game.Run;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Game.Bootstrap.PlayModeTests
{
    /// <summary>FIELD-002 slice (DECISION-0063): the second field starts in the real scene with its own layout and pool.</summary>
    public sealed class ProductionField002SmokeTests
    {
        private static readonly string[] Pool = { "ENEMY-001", "ENEMY-002", "ENEMY-003", "ENEMY-004", "ENEMY-005", "ENEMY-006",
            "ENEMY-007", "ENEMY-009", "ENEMY-010" };

        [UnityTest]
        public IEnumerator Field002_StartsWithGeneratedBlobs_AndSpawnsOnlyItsPool()
        {
            var warnings = new System.Collections.Generic.List<string>();
            Application.LogCallback collect = (message, _, type) =>
            {
                if (type == LogType.Warning && !message.StartsWith("[Perf]")) warnings.Add(message);
            };
            Application.logMessageReceived += collect;
            GameplayCompositionRoot root = null;
            try
            {
                // A profile that has cleared FIELD-001, so FIELD-002 is open through the normal field screen.
                var codec = new ProfileCodec(MetaCatalog.Load());
                var profile = codec.Create();
                profile.ClearedFields.Add("FIELD-001");
                profile.Unlocked.Add("FIELD-002");
                var store = new MemoryProfileStore();
                store.WriteAsync(codec.Encode(profile)).GetAwaiter().GetResult();
                ProductionSmokeScene.Load(store);
                yield return null;
                yield return null;
                root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
                var character = root.SelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectStart);
                using (var submit = NavigationSubmitEvent.GetPooled()) { submit.target = character; character.SendEvent(submit); }
                root.UseReferenceSeeds = true;
                Assert.IsTrue(root.TryStartField(new ContentId("FIELD-002")), "FIELD-002 opens after FIELD-001 is cleared.");
                yield return null;

                var run = Object.FindAnyObjectByType<RunController>();
                Assert.AreEqual("FIELD-002", run.Model.Selection.FieldId.ToString());
                // DECISION-0136: the colliders are exactly this run's generated blob outlines in a 100-unit arena.
                var layout = FixtureFieldEnvironmentPresentationCatalog.Load(RuntimeContentCatalog.ProductionFieldPresentationPath)
                    .Values.Single(p => p.Id.ToString() == "FIELD-002-PRESENTATION").BlobLayout;
                Assert.AreEqual(layout.ReferenceSeed, root.LayoutSeed, "Reference seeds pin the layout.");
                var expected = FieldBlobLayoutGenerator.Generate(layout, 100f, Vector2.zero, root.LayoutSeed, "FIELD-002-ENVIRONMENT");
                var colliders = GameObject.Find("FieldEnvironmentArt").GetComponentsInChildren<PolygonCollider2D>();
                Assert.AreEqual(20, colliders.Length, "Twenty blobs.");
                CollectionAssert.AreEquivalent(expected.Select(s => s.Id), colliders.Select(c => c.gameObject.name));
                Assert.AreEqual(50.5f, GameObject.Find("Wall_Top").transform.position.y, .6f, "The boundary walls enclose a 100-unit arena.");
                for (var i = 0; i < 600; i++) yield return new WaitForFixedUpdate();
                Assert.AreEqual(RunState.Running, run.Model.State);
                var enemies = Object.FindObjectsByType<EnemyRuntime>(FindObjectsSortMode.None);
                Assert.Greater(enemies.Length, 0, "The FIELD-002 timeline spawns from the start.");
                foreach (var enemy in enemies)
                    CollectionAssert.Contains(Pool, enemy.ContentId.ToString());
            }
            finally
            {
                if (root != null && root.IsInitialized) root.Shutdown();
                Application.logMessageReceived -= collect;
            }
            CollectionAssert.IsEmpty(warnings, "Only PerfGuard timing warnings are tolerated.");
        }
    }
}
