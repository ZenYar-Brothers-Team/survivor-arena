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
    /// <summary>FIELD-DEV-BLOBS (field-dev-blobs-v1): the development blob field starts on a fresh profile in a 120-unit arena.</summary>
    public sealed class ProductionFieldDevBlobsSmokeTests
    {
        private static float WallY() => GameObject.Find("Wall_Top").transform.position.y;

        [UnityTest]
        public IEnumerator DevField_StartsOnAFreshProfile_WithGeneratedBlobs_InAResizedArena_AndRestoresTheScene()
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
                var codec = new ProfileCodec(MetaCatalog.Load());
                var store = new MemoryProfileStore();
                store.WriteAsync(codec.Encode(codec.Create())).GetAwaiter().GetResult();
                ProductionSmokeScene.Load(store);
                yield return null;
                yield return null;
                root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
                var sceneWallY = WallY();
                var character = root.SelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectStart);
                using (var submit = NavigationSubmitEvent.GetPooled()) { submit.target = character; character.SendEvent(submit); }
                root.UseReferenceSeeds = true;
                Assert.IsTrue(root.TryStartField(new ContentId("FIELD-DEV-BLOBS")), "The development field is always available.");
                yield return null;

                var run = Object.FindAnyObjectByType<RunController>();
                Assert.AreEqual("FIELD-DEV-BLOBS", run.Model.Selection.FieldId.ToString());
                var art = GameObject.Find("FieldEnvironmentArt");
                var colliders = art.GetComponentsInChildren<PolygonCollider2D>();
                Assert.AreEqual(20, colliders.Length, "Twenty blobs.");
                var layout = FixtureFieldEnvironmentPresentationCatalog.Load(RuntimeContentCatalog.ProductionFieldPresentationPath)
                    .Values.Single(p => p.Id.ToString() == "FIELD-DEV-BLOBS-PRESENTATION").BlobLayout;
                var expected = FieldBlobLayoutGenerator.Generate(layout, 120f, Vector2.zero, root.LayoutSeed, "FIELD-DEV-BLOBS-ENVIRONMENT");
                CollectionAssert.AreEquivalent(expected.Select(s => s.Id), colliders.Select(c => c.gameObject.name));
                var playerOnly = ~(1 << LayerMask.NameToLayer("Player"));
                Assert.IsTrue(colliders.All(c => c.excludeLayers == playerOnly),
                    "Blobs block only the player; every other layer passes through (DECISION-0003).");
                Assert.AreEqual(60.5f, WallY(), .6f, "The boundary walls enclose a 120-unit arena.");
                // Development map overlay: the button shows the arena frame, every obstacle outline and the camera frame.
                var ui = Object.FindAnyObjectByType<GameplayUiRoot>().Document.rootVisualElement;
                var mapToggle = ui.Q<Button>(GameplayUiElementIds.MapToggle);
                Assert.IsTrue(mapToggle.enabledSelf, "The map button is available in development builds.");
                using (var submit = NavigationSubmitEvent.GetPooled()) { submit.target = mapToggle; mapToggle.SendEvent(submit); }
                yield return null;
                Assert.AreEqual(DisplayStyle.Flex, ui.Q(GameplayUiElementIds.MapOverlay).style.display.value,
                    "The map overlay is shown after the click.");
                var summary = ui.Q<Label>(GameplayUiElementIds.MapSummary).text;
                StringAssert.Contains("120×120", summary);
                StringAssert.Contains("препятствий: 20", summary);
                Object.FindAnyObjectByType<Game.Character.PlayerCharacterRuntime>().Health.IsLocked = true;
                for (var i = 0; i < 300; i++) yield return new WaitForFixedUpdate();
                Assert.AreEqual(RunState.Running, run.Model.State);
                Assert.Greater(Object.FindObjectsByType<EnemyRuntime>(FindObjectsSortMode.None).Length, 0,
                    "Spawn settings are FIELD-001's, so enemies appear from the start.");
                root.Shutdown();
                Assert.AreEqual(sceneWallY, WallY(), 1e-3f, "Shutdown restores the shared scene arena.");
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
