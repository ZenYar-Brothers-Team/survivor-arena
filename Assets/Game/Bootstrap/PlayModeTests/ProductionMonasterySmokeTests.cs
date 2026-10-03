using System.Collections;
using System.Linq;
using Game.Content;
using Game.Meta;
using Game.Presentation;
using Game.Run;
using Game.UI;
using Game.Zones;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Game.Bootstrap.PlayModeTests
{
    public sealed class ProductionMonasterySmokeTests
    {
        [UnityTest]
        public IEnumerator DevUnlock_Field007Card_StartsThirtySixVisibleAltarsAndPausesCleanly()
        {
            GameplayCompositionRoot root = null;
            try
            {
                ProductionSmokeScene.Load(new MemoryProfileStore(), false);
                yield return null; yield return null;
                root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
                Assert.IsFalse(root.Profile.IsUnlocked("FIELD-007"), "Fresh profile cannot select monastery before Dev unlock.");
                root.UnlockAllForDevelopment();
                for (var i = 0; i < 30 && !root.Profile.IsUnlocked("FIELD-007"); i++) yield return null;
                Assert.IsTrue(root.Profile.IsUnlocked("FIELD-007"));
                root.Play(); yield return null;
                UiFoundationSmokeTests.Submit(root.SelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectStart));
                yield return null;
                var card = root.FieldSelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.FieldSelectCard("FIELD-007"));
                Assert.IsNotNull(card); Assert.IsTrue(card.enabledSelf);
                UiFoundationSmokeTests.Submit(card);
                root.UseReferenceSeeds = true;
                UiFoundationSmokeTests.Submit(root.FieldSelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.FieldSelectStart));
                yield return null; yield return null;
                var run = Object.FindAnyObjectByType<RunController>();
                Assert.AreEqual("FIELD-007", run.Model.Selection.FieldId.ToString());
                var driver = Object.FindAnyObjectByType<ZoneRuntimeDriver>();
                Assert.AreEqual(36, driver.Runtime.Zones.Count);
                Assert.AreEqual(36, driver.GetComponentsInChildren<AltarPresentationRuntime>(true).Length);
                Assert.AreEqual(36, driver.GetComponentsInChildren<CircleCollider2D>(true).Length);
                Object.FindAnyObjectByType<Game.Character.PlayerCharacterRuntime>().Health.IsLocked = true;
                var centers = driver.Runtime.Zones.Select(z => z.Center).ToArray();
                var fieldPresentation = root.Catalog.FieldEnvironmentPresentations[root.FieldConfiguration.Environment.Id];
                var spawn = FieldEnvironmentBinding.Validate(root.FieldConfiguration.Environment, root.gameObject.scene);
                var expectedAltars = ZoneLayoutGenerator.Generate(fieldPresentation.ZoneLayout,
                    fieldPresentation.ArenaSideLength.Value, spawn.position, null, root.ZoneSeed,
                    ZoneRuntimeDriver.CameraRect(Camera.main).size);
                CollectionAssert.AreEqual(expectedAltars.Select(zone => zone.Center), centers,
                    "Altar layout must be chosen independently before obstacle generation.");
                var radii = driver.Runtime.Zones.Select(z => z.Radius).ToArray();
                Assert.Greater(radii.Distinct().Count(), 9);
                for (var i = 0; i < 500 && Object.FindObjectsByType<Game.Enemy.EnemyRuntime>(FindObjectsSortMode.None).Length == 0; i++)
                    yield return new WaitForFixedUpdate();
                Assert.Greater(driver.Runtime.Time, 0f);
                Assert.Greater(Object.FindObjectsByType<Game.Enemy.EnemyRuntime>(FindObjectsSortMode.None).Length, 0);
                run.TogglePause(); var paused = driver.Runtime.Time;
                for (var i = 0; i < 5; i++) yield return null;
                Assert.AreEqual(paused, driver.Runtime.Time);
                CollectionAssert.AreEqual(centers, driver.Runtime.Zones.Select(z => z.Center));
                CollectionAssert.AreEqual(radii, driver.Runtime.Zones.Select(z => z.Radius));
                run.TogglePause();
                var uiDocument = Object.FindAnyObjectByType<GameplayUiRoot>().Document;
                var ui = uiDocument.rootVisualElement;
                UiFoundationSmokeTests.Click(ui.Q<Button>(GameplayUiElementIds.DevelopmentToggleButton));
                yield return null; yield return null;
                UiFoundationSmokeTests.Click(ui.Q<Button>(GameplayUiElementIds.DevelopmentMapTab));
                yield return null; yield return null;
                UiFoundationSmokeTests.Click(ui.Q<Button>(GameplayUiElementIds.MapToggle));
                for (var i = 0; i < 20; i++) yield return null;
                Assert.AreEqual(DisplayStyle.Flex, ui.Q(GameplayUiElementIds.MapOverlay).resolvedStyle.display);
                StringAssert.Contains("алтарей: 36", ui.Q<Label>(GameplayUiElementIds.MapSummary).text);
                if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null)
                {
                    var camera = Camera.main;
                    var prior = camera.targetTexture;
                    var target = new RenderTexture(1920, 1080, 24); target.Create();
                    var priorUiTarget = uiDocument.panelSettings.targetTexture;
                    var player = Object.FindAnyObjectByType<Game.Character.PlayerCharacterRuntime>();
                    try
                    {
                        camera.targetTexture = target;
                        foreach (var positive in new[] { true, false })
                        {
                            var altar = driver.Runtime.Zones.First(z => z.Effect.Kind != ZoneEffectKind.Shrine &&
                                (z.Effect.Polarity == ZoneAltarPolarity.Positive) == positive);
                            var standingPosition = altar.Center + Vector2.down * 1.25f;
                            player.transform.position = standingPosition; player.GetComponent<Rigidbody2D>().position = standingPosition;
                            for (var i = 0; i < 30; i++) yield return null;
                            var altarView = driver.GetComponentsInChildren<AltarPresentationRuntime>().Single(view =>
                                Vector2.Distance(view.transform.position, altar.Center) < .001f);
                            Assert.GreaterOrEqual(Physics2D.Distance(player.GetComponent<CircleCollider2D>(),
                                altarView.GetComponent<CircleCollider2D>()).distance, -.02f, "Player stands outside the altar foundation.");
                            UiFoundationSmokeTests.Capture(target, positive ? "field007-positive" : "field007-negative");
                        }
                        var shrine = driver.Runtime.Zones.First(z => z.Effect.Kind == ZoneEffectKind.Shrine);
                        var shrineStandingPosition = shrine.Center + Vector2.down * 1.25f;
                        player.transform.position = shrineStandingPosition; player.GetComponent<Rigidbody2D>().position = shrineStandingPosition;
                        for (var i = 0; i < 30; i++) yield return null;
                        UiFoundationSmokeTests.Capture(target, "field007-shrine");
                        uiDocument.panelSettings.targetTexture = target;
                        for (var i = 0; i < 10; i++) yield return null;
                        UiFoundationSmokeTests.Capture(target, "field007-minimap");
                    }
                    finally
                    {
                        uiDocument.panelSettings.targetTexture = priorUiTarget;
                        camera.targetTexture = prior; target.Release(); Object.DestroyImmediate(target);
                    }
                }
                root.Shutdown(); yield return null;
                Assert.IsNull(Object.FindAnyObjectByType<ZoneRuntimeDriver>());
                Assert.IsNull(Object.FindAnyObjectByType<AltarPresentationRuntime>());
            }
            finally { if (root != null && root.IsInitialized) root.Shutdown(); }
        }
    }
}
