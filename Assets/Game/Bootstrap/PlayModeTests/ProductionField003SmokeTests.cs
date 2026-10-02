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
    /// <summary>FIELD-003 roads start in the existing scene with unchanged encounters and one-choice field books.</summary>
    public sealed class ProductionField003SmokeTests
    {
        private static readonly string[] Pool = { "ENEMY-001", "ENEMY-002", "ENEMY-003", "ENEMY-004", "ENEMY-005", "ENEMY-006",
            "ENEMY-007", "ENEMY-008", "ENEMY-009", "ENEMY-010" };

        [UnityTest]
        public IEnumerator Field003_StartsWithRoadsBooksAndPlayerOnlyGrass_AndSpawnsOnlyItsPool()
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
                // A profile that has cleared FIELD-001 and FIELD-002, so FIELD-003 is open through the normal field screen.
                var codec = new ProfileCodec(MetaCatalog.Load());
                var profile = codec.Create();
                profile.ClearedFields.Add("FIELD-001");
                profile.ClearedFields.Add("FIELD-002");
                profile.Unlocked.Add("FIELD-002");
                profile.Unlocked.Add("FIELD-003");
                var store = new MemoryProfileStore();
                store.WriteAsync(codec.Encode(profile)).GetAwaiter().GetResult();
                ProductionSmokeScene.Load(store);
                yield return null;
                yield return null;
                root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
                var character = root.SelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectStart);
                using (var submit = NavigationSubmitEvent.GetPooled()) { submit.target = character; character.SendEvent(submit); }
                root.UseReferenceSeeds = true;
                Assert.IsTrue(root.TryStartField(new ContentId("FIELD-003")), "FIELD-003 opens after FIELD-002 is cleared.");
                yield return null;

                var run = Object.FindAnyObjectByType<RunController>();
                Assert.AreEqual("FIELD-003", run.Model.Selection.FieldId.ToString());
                var art = GameObject.Find("FieldEnvironmentArt");
                var definition = FixtureFieldEnvironmentPresentationCatalog.Load(RuntimeContentCatalog.ProductionFieldPresentationPath)
                    .Values.Single(p => p.Id.ToString() == "FIELD-003-PRESENTATION");
                var expected = FieldRoadLayoutGenerator.Generate(definition.RoadLayout, definition.RoadFallbackLayouts,root.LayoutSeed);
                var boundaries = art.GetComponentsInChildren<EdgeCollider2D>();
                Assert.Greater(boundaries.Length,0);
                foreach (var boundary in boundaries)
                    Assert.AreEqual(~(1 << LayerMask.NameToLayer("Player")),boundary.excludeLayers.value);
                Assert.AreEqual(expected.DeadEnds.Count,root.Pickups.Snapshot.Active);
                var pickups = new System.Collections.Generic.List<Game.Pickup.WorldPickupVisual>(); root.Pickups.CopyActiveTo(pickups);
                foreach (var pickup in pickups)
                {
                    Assert.AreEqual(1,pickup.Life.Definition.FixedBookUpgradeCount);
                    Assert.IsTrue(expected.DeadEnds.Any(b => Vector2.Distance(b.EndCenter,pickup.transform.position) < 1e-4f));
                }
                var player = Object.FindAnyObjectByType<Game.Character.PlayerCharacterRuntime>();
                Assert.Less(Vector2.Distance(player.transform.position,expected.SpawnPosition),.1f);
                if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null)
                {
                    // Integration evidence only: capture the actual meshes at gameplay scale and as a whole-map overview.
                    var camera = Camera.main; var previousTarget = camera.targetTexture;
                    var target = new RenderTexture(1280,720,24); target.Create();
                    GameObject overview = null;
                    try
                    {
                        camera.targetTexture = target;
                        yield return null; yield return null;
                        UiFoundationSmokeTests.Capture(target,"field003-roads-gameplay");
                        camera.targetTexture = previousTarget;
                        overview = new GameObject("Road overview capture");
                        var overviewCamera = overview.AddComponent<Camera>(); overviewCamera.CopyFrom(camera);
                        overviewCamera.targetTexture = target; overviewCamera.orthographicSize = definition.RoadLayout.ArenaSideLength*.55f;
                        overviewCamera.transform.position = new Vector3(0,0,camera.transform.position.z);
                        yield return null; yield return null;
                        UiFoundationSmokeTests.Capture(target,"field003-roads-overview");
                        overviewCamera.orthographicSize = camera.orthographicSize;
                        var book = expected.DeadEnds[0].EndCenter;
                        overviewCamera.transform.position = new Vector3(book.x,book.y,camera.transform.position.z);
                        yield return null; yield return null;
                        UiFoundationSmokeTests.Capture(target,"field003-roads-book-art");
                        // Show front and back curb faces together, in addition to the gameplay-scale crop.
                        overviewCamera.orthographicSize = definition.RoadLayout.DeadEndEndRadius*1.2f;
                        yield return null; yield return null;
                        UiFoundationSmokeTests.Capture(target,"field003-roads-curb-review");
                    }
                    finally
                    {
                        camera.targetTexture = previousTarget;
                        if (overview != null) Object.DestroyImmediate(overview);
                        target.Release(); Object.DestroyImmediate(target);
                    }
                }
                // The smoke checks layout and spawn pool; an idle player may not survive 12 s of FIELD-003 after the
                // DECISION-0073 skill nerf, so health is locked as with the development toggle.
                Object.FindAnyObjectByType<Game.Character.PlayerCharacterRuntime>().Health.IsLocked = true;
                for (var i = 0; i < 600; i++) yield return new WaitForFixedUpdate();
                Assert.AreEqual(RunState.Running, run.Model.State);
                var enemies = Object.FindObjectsByType<EnemyRuntime>(FindObjectsSortMode.None);
                Assert.Greater(enemies.Length, 0, "The FIELD-003 timeline spawns from the start.");
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
