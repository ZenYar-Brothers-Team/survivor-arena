using System.Collections;
using System.Linq;
using Game.Content;
using Game.Meta;
using Game.Run;
using Game.UI;
using Game.Zones;
using Game.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Game.Bootstrap.PlayModeTests
{
    /// <summary>FIELD-DEV-ZONES (field-dev-zones-v1): the effect-zone test field starts on a fresh profile, ticks its zones and cleans up.</summary>
    public sealed class ProductionFieldDevZonesSmokeTests
    {
        [UnityTest]
        public IEnumerator ZonesField_StartsWithAllSixEffects_TicksWhileRunning_AndRemovesItselfOnShutdown()
        {
            yield return SmokeField("FIELD-DEV-ZONES", false);
        }

        [UnityTest]
        public IEnumerator Academy_UnlocksThroughDevButton_StartsThroughFieldCard_AndRunsSeals()
        {
            yield return SmokeField("FIELD-006", true);
        }

        private static IEnumerator SmokeField(string fieldId, bool unlockThroughDev)
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
                ProductionSmokeScene.Load(store, openSelection: !unlockThroughDev);
                yield return null;
                yield return null;
                root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
                if (unlockThroughDev)
                {
                    Assert.IsTrue(root.AtMainMenu);
                    Assert.IsFalse(root.Profile.IsUnlocked(fieldId), "Academy follows normal progression on a fresh profile.");
                    UiFoundationSmokeTests.Submit(root.ShellDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.EntryDevelopmentToggle));
                    UiFoundationSmokeTests.Submit(root.ShellDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.ShellDevelopmentUnlockAll));
                    yield return null; yield return null;
                    Assert.IsTrue(root.Profile.IsUnlocked(fieldId), "The real Dev command unlocks the academy.");
                    root.Play(); yield return null;
                }
                var character = root.SelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectStart);
                using (var submit = NavigationSubmitEvent.GetPooled()) { submit.target = character; character.SendEvent(submit); }
                root.UseReferenceSeeds = true;
                if (unlockThroughDev)
                {
                    var fieldUi = root.FieldSelectionDocument.rootVisualElement;
                    var card = fieldUi.Q<Button>(GameplayUiElementIds.FieldSelectCard(fieldId));
                    Assert.IsNotNull(card, "The actual academy card exists in field selection.");
                    UiFoundationSmokeTests.Submit(card);
                    Assert.AreEqual(fieldId, root.FieldSelection.SelectedId.ToString());
                    var start = fieldUi.Q<Button>(GameplayUiElementIds.FieldSelectStart);
                    Assert.IsTrue(start.enabledSelf, "Dev unlock makes the academy playable, not merely inspectable.");
                    UiFoundationSmokeTests.Submit(start);
                }
                else Assert.IsTrue(root.TryStartField(new ContentId(fieldId)), "The zones test field is always available.");
                yield return null;

                var run = Object.FindAnyObjectByType<RunController>();
                Assert.AreEqual(fieldId, run.Model.Selection.FieldId.ToString());
                Assert.AreEqual(6, root.ZoneSeed, "Reference seeds pin the zone layout.");
                var driver = Object.FindAnyObjectByType<ZoneRuntimeDriver>();
                if (unlockThroughDev)
                {
                    Assert.AreEqual(7, driver.Runtime.Zones.Count(z => z.IsScheduled));
                    Assert.LessOrEqual(driver.Runtime.Zones.Count(z => z.IsScheduled && z.IsPresent), 2);
                }
                Assert.IsNotNull(driver, "The zone driver exists on a zones field.");
                var zones = driver.Runtime.Zones;
                Assert.AreEqual(unlockThroughDev ? 14 : 15, zones.Count, "Academy has one shared green spring; legacy study retains both.");
                CollectionAssert.AreEquivalent(new[] { ZoneEffectKind.Slow, ZoneEffectKind.Haste, ZoneEffectKind.Regeneration,
                    ZoneEffectKind.ArcanePower, ZoneEffectKind.Rift, ZoneEffectKind.Portal, ZoneEffectKind.Protection,
                    ZoneEffectKind.SpeedBurst }, zones.Select(z => z.Effect.Kind).Distinct());
                if (unlockThroughDev) Assert.IsFalse(zones.Any(z => z.Effect.Lifetime == ZoneLifetimeMode.Permanent));
                else Assert.IsTrue(zones.Any(z => z.Effect.Lifetime == ZoneLifetimeMode.Permanent) &&
                              zones.Any(z => z.Effect.Lifetime == ZoneLifetimeMode.Pulsing) &&
                              zones.Any(z => z.Effect.Lifetime == ZoneLifetimeMode.Burst), "Permanent, pulsing and burst zones all exist.");
                Assert.AreEqual(zones.Count, driver.GetComponentsInChildren<ZoneSealPresentationRuntime>(true).Length, "One animated seal per zone.");
                Assert.AreEqual(zones.Count * 3, driver.GetComponentsInChildren<MeshRenderer>(true).Length, "Rim, glyph and internal motion per seal.");
                foreach (var zone in zones.Where(z => z.Effect.Lifetime == ZoneLifetimeMode.Pulsing))
                    Assert.AreEqual(5f, zone.Effect.PulsePrepareSeconds, "All pulsing academy study seals prepare for five seconds.");

                Object.FindAnyObjectByType<Game.Character.PlayerCharacterRuntime>().Health.IsLocked = true;
                for (var i = 0; i < 120; i++) yield return new WaitForFixedUpdate();
                Assert.AreEqual(RunState.Running, run.Model.State);
                Assert.Greater(driver.Runtime.Time, 0.5f, "Zones advance while the run is running.");

                SpritePresentationRuntime damagedBody = null;
                using var damageAdapter = new EnemyZoneSource(body => { damagedBody = body; driver.ShowRiftHit(body); },
                    suppressAreaUnitFeedback: unlockThroughDev);
                Assert.Greater(damageAdapter.Refresh(), 0, "The actual field has ordinary enemies to receive zone damage.");
                damageAdapter.Damage(0, .01f, zones.First(z => z.Effect.Kind == ZoneEffectKind.Rift).Effect.Id);
                damageAdapter.Refresh();
                ZoneRiftHitPresentationRuntime riftHit = null;
                if (unlockThroughDev)
                {
                    Assert.IsNull(damagedBody, "Area damage has no additional unit feedback.");
                    var enemies = new System.Collections.Generic.List<Game.Enemy.EnemyRuntime>();
                    Game.Enemy.EnemyRegistry.CopyAliveTo(enemies);
                    var enemy = enemies[0];
                    damageAdapter.Slow(0, .45f, 0f, zones.First(z => z.Effect.Kind == ZoneEffectKind.Slow).Effect.Id);
                    Assert.AreEqual(.45f, enemy.AreaSlowFraction);
                    Assert.AreEqual(0f, enemy.Controls.SlowRemaining01, "Area slow creates no timed ice/bar status.");
                    damageAdapter.Refresh();
                    Assert.AreEqual(0f, enemy.AreaSlowFraction, "Leaving or switching off clears area slow on the next zone tick.");
                }
                else
                {
                    Assert.IsNotNull(damagedBody, "Actual applied damage calls the legacy feedback bridge.");
                    riftHit = damagedBody.GetComponent<ZoneRiftHitPresentationRuntime>();
                    Assert.IsTrue(riftHit.IsShowing);
                }

                run.TogglePause();
                var pausedTime = driver.Runtime.Time;
                var visibleSeal = driver.GetComponentsInChildren<ZoneSealPresentationRuntime>().First();
                var pausedMotion = visibleSeal.transform.Find("Motion").localRotation;
                for (var i = 0; i < 5; i++) yield return null;
                Assert.AreEqual(pausedTime, driver.Runtime.Time, "Pause holds the entire preparation clock.");
                Assert.AreEqual(pausedMotion, visibleSeal.transform.Find("Motion").localRotation);
                if (riftHit != null) Assert.IsTrue(riftHit.IsShowing, "The legacy red hit also holds while paused.");
                run.TogglePause();

                if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null)
                {
                    var player = Object.FindAnyObjectByType<Game.Character.PlayerCharacterRuntime>();
                    var slowZone = zones.First(z => z.Effect.Kind == ZoneEffectKind.Slow);
                    player.transform.position = slowZone.Center;
                    player.GetComponent<Rigidbody2D>().position = slowZone.Center;
                    for (var i = 0; i < 15; i++) yield return null;
                    var camera = Camera.main; var previousTarget = camera.targetTexture;
                    var target = new RenderTexture(1280, 720, 24); target.Create();
                    try
                    {
                        camera.targetTexture = target;
                        yield return null; yield return null;
                        UiFoundationSmokeTests.Capture(target, unlockThroughDev ? "academy-field006-gameplay" : "academy-seals-gameplay");
                    }
                    finally { camera.targetTexture = previousTarget; target.Release(); Object.DestroyImmediate(target); }
                }

                if (unlockThroughDev)
                {
                    var player = Object.FindAnyObjectByType<Game.Character.PlayerCharacterRuntime>();
                    var camera = Camera.main; var follow = camera.GetComponent<Game.Movement.CameraFollowTarget>();
                    var from = (Vector2)player.transform.position;
                    var destination = from + Vector2.right * 25f;
                    var profile = ZoneSealPresentationProfile.Load();
                    var transit = player.GetComponent<PortalTransitRuntime>() ?? player.gameObject.AddComponent<PortalTransitRuntime>();
                    var body = player.GetComponent<Rigidbody2D>();
                    var presentation = player.GetComponentInChildren<SpritePresentationRuntime>();
                    Assert.IsTrue(transit.Begin(destination, run, profile, player.Health, presentation, follow));
                    Assert.IsFalse(body.simulated);
                    yield return new WaitForSeconds(.55f);
                    Assert.IsTrue(transit.IsActive); Assert.IsFalse(presentation.gameObject.activeSelf);
                    Assert.AreEqual(from, (Vector2)player.transform.position, "The actor arrives after the hidden flight.");
                    var cameraMiddle = camera.transform.position;
                    Assert.Greater(cameraMiddle.x, from.x); Assert.Less(cameraMiddle.x, destination.x);
                    run.TogglePause();
                    for (var i = 0; i < 5; i++) yield return null;
                    Assert.AreEqual(cameraMiddle, camera.transform.position, "Pause freezes the camera transit.");
                    run.TogglePause();
                    yield return new WaitForSeconds(1.1f);
                    Assert.IsFalse(transit.IsActive); Assert.IsTrue(body.simulated);
                    Assert.IsTrue(presentation.gameObject.activeSelf);
                    Assert.Less(Vector2.Distance(destination, player.transform.position), .01f);
                    Assert.Less(Vector2.Distance(destination, camera.transform.position), .01f);
                    var alive = new System.Collections.Generic.List<Game.Enemy.EnemyRuntime>();
                    Game.Enemy.EnemyRegistry.CopyAliveTo(alive);
                    var enemy = alive.First(e => e.Category == Game.Enemy.EnemyCategory.Ordinary && e.BodyPresentation != null);
                    var enemyTransit = enemy.GetComponent<PortalTransitRuntime>() ?? enemy.gameObject.AddComponent<PortalTransitRuntime>();
                    var enemyBody = enemy.GetComponent<Rigidbody2D>(); var healthLock = enemy.Health.IsLocked;
                    Assert.IsTrue(enemyTransit.Begin(enemy.Position + Vector2.right * 10f, run, profile, enemy.Health, enemy.BodyPresentation));
                    Assert.IsFalse(enemy.enabled); Assert.IsFalse(enemyBody.simulated);
                    yield return new WaitForSeconds(.55f);
                    Assert.IsTrue(enemyTransit.IsActive); Assert.IsNull(enemy.BodyPresentation, "Enemy visual root is hidden in transit.");
                    yield return new WaitForSeconds(1.1f);
                    Assert.IsFalse(enemyTransit.IsActive); Assert.IsTrue(enemy.enabled); Assert.IsTrue(enemyBody.simulated);
                    Assert.AreEqual(healthLock, enemy.Health.IsLocked);
                }

                root.Shutdown();
                yield return null;
                Assert.IsNull(Object.FindAnyObjectByType<ZoneRuntimeDriver>(), "Shutdown removes the zones.");
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
