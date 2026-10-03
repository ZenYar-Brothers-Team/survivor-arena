using System.Collections;
using System.Linq;
using Game.Character;
using Game.Content;
using Game.Meta;
using Game.Presentation;
using Game.Run;
using Game.ScreenEvents;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Game.Bootstrap.PlayModeTests
{
    /// <summary>DECISION-0157: the last field builds its screen events in the real scene, warns, strikes the player once and cleans up.</summary>
    public sealed class ProductionField010ScreenEventsSmokeTests
    {
        [UnityTest]
        public IEnumerator Field010_RunsScreenEvents_WarnsThenHurtsAPlayerOutsideTheSafeCircleOnce_AndRemovesThemOnShutdown()
        {
            GameplayCompositionRoot root = null;
            try
            {
                var codec = new ProfileCodec(MetaCatalog.Load());
                var profile = codec.Create();
                profile.Unlocked.Add("FIELD-010");
                var store = new MemoryProfileStore();
                store.WriteAsync(codec.Encode(profile)).GetAwaiter().GetResult();
                ProductionSmokeScene.Load(store);
                yield return null;
                yield return null;
                root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
                var character = root.SelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectStart);
                using (var submit = NavigationSubmitEvent.GetPooled()) { submit.target = character; character.SendEvent(submit); }
                root.UseReferenceSeeds = true;
                Assert.IsTrue(root.TryStartField(new ContentId("FIELD-010")));
                yield return null;

                var environmentArt = GameObject.Find("FieldEnvironmentArt");
                Assert.IsNotNull(environmentArt);
                Assert.IsNull(environmentArt.transform.Find("Stump"), "FIELD-010 cannot inherit a FIELD-001 stump.");
                Assert.IsFalse(environmentArt.GetComponentsInChildren<Collider2D>().Any(c => c.enabled),
                    "The screen-event arena has no blocking fixture props.");
                var presentation = root.Catalog.FieldEnvironmentPresentations[new ContentId("FIELD-010-ENVIRONMENT")];
                Assert.IsFalse(GameObject.Find(presentation.ObstacleName).GetComponent<Collider2D>().enabled,
                    "Removing foreign artwork must also remove its invisible collision.");
                Assert.AreEqual(presentation.GroundTint, environmentArt.transform.Find("Ground").GetComponent<SpriteRenderer>().color);
                var driver = root.ScreenEvents;
                Assert.IsNotNull(driver, "FIELD-010 builds its screen event driver.");
                Assert.AreEqual(10101, root.ScreenEventSeed);
                var runtime = driver.Runtime;
                var player = Object.FindAnyObjectByType<PlayerCharacterRuntime>();
                var run = Object.FindAnyObjectByType<RunController>();
                Assert.AreEqual(RunState.Running, run.Model.State);

                var view = Game.Bootstrap.ZoneRuntimeDriver.CameraRect(Camera.main);
                Assert.IsTrue(runtime.TryStart("SCREEN-EVENT-012", view));
                yield return null;
                var hazard = runtime.Active.Hazards.Single();
                Assert.IsNotNull(GameObject.Find("Hazard-Burst"), "The warning is drawn.");
                var art = driver.GetComponentInChildren<ScreenHazardArtView>();
                Assert.AreEqual(1, driver.ActiveViewCount);
                Assert.IsTrue(art.GetComponentsInChildren<MeshRenderer>().Any(r => r.enabled));
                var playerOrder = player.GetComponentsInChildren<SpriteRenderer>().Max(r => r.sortingOrder);
                Assert.IsTrue(art.GetComponentsInChildren<MeshRenderer>().All(r => r.sortingOrder < playerOrder),
                    "The player remains above every event artwork layer.");
                var elapsed = runtime.Active.Elapsed;
                run.TogglePause();
                yield return new WaitForSecondsRealtime(.15f);
                Assert.AreEqual(elapsed, runtime.Active.Elapsed, "Paused artwork and hazards share the same clock.");
                Assert.AreEqual(ScreenHazardPhase.Telegraph, art.Phase);
                run.TogglePause();

                // Walk out of the safe circle at once (a teleport, so the test does not depend on speed) and wait out the warning.
                var outside = hazard.Origin + new Vector2(view.width * .4f, 0f);
                player.transform.position = outside;
                player.GetComponent<Rigidbody2D>().position = outside;
                var before = player.Health.CurrentHealth;
                var deadline = Time.realtimeSinceStartup + 6f;
                while (runtime.Active != null && Time.realtimeSinceStartup < deadline) yield return null;
                yield return null;
                Assert.IsNull(runtime.Active, "The event finishes.");
                Assert.AreEqual(1, runtime.PlayerHitCount, "A strike hurts at most once.");
                Assert.Less(player.Health.CurrentHealth, before, "The player outside the safe circle lost health.");
                Assert.IsNull(GameObject.Find("Hazard-Burst"), "The drawing is removed when the event ends.");
                Assert.AreEqual(0, driver.ActiveViewCount);
                Assert.AreEqual(1, driver.PooledViewCount);
                Assert.IsTrue(runtime.TryStart("SCREEN-EVENT-012", view));
                Assert.AreSame(art, driver.GetComponentInChildren<ScreenHazardArtView>(), "A second event reuses its view.");
                runtime.Clear();

                root.Shutdown();
                yield return null;
                Assert.IsNull(GameObject.Find("FieldScreenEvents"), "Shutdown removes the driver.");
            }
            finally
            {
                if (root != null && root.IsInitialized) root.Shutdown();
            }
        }

        [UnityTest]
        public IEnumerator Field010_DevelopmentEventsTab_StartsTheChosenEventTwoSecondsAfterThePress()
        {
            GameplayCompositionRoot root = null;
            try
            {
                var codec = new ProfileCodec(MetaCatalog.Load());
                var profile = codec.Create();
                profile.Unlocked.Add("FIELD-010");
                var store = new MemoryProfileStore();
                store.WriteAsync(codec.Encode(profile)).GetAwaiter().GetResult();
                ProductionSmokeScene.Load(store);
                yield return null;
                yield return null;
                root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
                var character = root.SelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectStart);
                using (var submit = NavigationSubmitEvent.GetPooled()) { submit.target = character; character.SendEvent(submit); }
                root.UseReferenceSeeds = true;
                Assert.IsTrue(root.TryStartField(new ContentId("FIELD-010")));
                yield return null;
                var runtime = root.ScreenEvents.Runtime;
                var ui = Object.FindAnyObjectByType<GameplayUiRoot>().Document.rootVisualElement;
                for (var i = 0; i < 30; i++) yield return null;
                var tab = ui.Q<Button>(GameplayUiElementIds.DevelopmentEventsTab);
                Assert.AreEqual(DisplayStyle.Flex, tab.resolvedStyle.display, "The Events tab shows on a field with screen events.");
                var buttons = ui.Query<Button>().Where(b => b.name != null && b.name.StartsWith("development-events-start-")).ToList();
                Assert.AreEqual(13, buttons.Count, "One button per event.");
                var press = ui.Q<Button>(GameplayUiElementIds.ScreenEventStart("SCREEN-EVENT-006"));
                Assert.IsTrue(press.enabledSelf, "The buttons are enabled while nothing runs.");

                // The development drawer swallows keyboard navigation (real presses are pointer clicks), so the press itself is covered by the
                // view's EditMode test; here the control the button calls is driven directly.
                var pressed = Time.realtimeSinceStartup;
                Assert.IsTrue(((Game.UI.IScreenEventDevelopmentControl)root.ScreenEvents).TryQueue("SCREEN-EVENT-006"));
                Assert.AreEqual("SCREEN-EVENT-006", runtime.QueuedEventId);
                Assert.IsNull(runtime.Active, "Nothing starts at the press itself.");
                var refreshDeadline = Time.realtimeSinceStartup + 1f;
                while (press.enabledSelf && Time.realtimeSinceStartup < refreshDeadline) yield return null;
                Assert.IsFalse(press.enabledSelf, "The buttons are disabled while an event is queued.");
                while (runtime.Active == null && Time.realtimeSinceStartup < pressed + 5f) yield return null;
                var waited = Time.realtimeSinceStartup - pressed;
                Assert.AreEqual("SCREEN-EVENT-006", runtime.Active.Definition.Id.ToString());
                Assert.That(waited, Is.InRange(1.8f, 2.6f), "About two seconds after the press.");

                root.Shutdown();
                yield return null;
            }
            finally
            {
                if (root != null && root.IsInitialized) root.Shutdown();
            }
        }

        [UnityTest]
        public IEnumerator Field010_PaintsEveryEventLayout_WarningAndStrike_ForReview()
        {
            GameplayCompositionRoot root = null;
            try
            {
                var codec = new ProfileCodec(MetaCatalog.Load());
                var profile = codec.Create();
                profile.Unlocked.Add("FIELD-010");
                var store = new MemoryProfileStore();
                store.WriteAsync(codec.Encode(profile)).GetAwaiter().GetResult();
                ProductionSmokeScene.Load(store);
                yield return null;
                yield return null;
                root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
                var character = root.SelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectStart);
                using (var submit = NavigationSubmitEvent.GetPooled()) { submit.target = character; character.SendEvent(submit); }
                root.UseReferenceSeeds = true;
                Assert.IsTrue(root.TryStartField(new ContentId("FIELD-010")));
                yield return null;
                var runtime = root.ScreenEvents.Runtime;
                var player = Object.FindAnyObjectByType<PlayerCharacterRuntime>();
                player.Health.IsLocked = true;
                if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) yield break;
                var camera = Camera.main;
                camera.aspect = 1280f / 720f;
                var target = new RenderTexture(1280, 720, 24); target.Create();
                var captureObject = new GameObject("Screen event capture");
                try
                {
                    var capture = captureObject.AddComponent<Camera>(); capture.CopyFrom(camera);
                    capture.targetTexture = target;
                    foreach (var id in new[] { "SCREEN-EVENT-001", "SCREEN-EVENT-002", "SCREEN-EVENT-003", "SCREEN-EVENT-004", "SCREEN-EVENT-005", "SCREEN-EVENT-006",
                                 "SCREEN-EVENT-007", "SCREEN-EVENT-008", "SCREEN-EVENT-009", "SCREEN-EVENT-010", "SCREEN-EVENT-011",
                                 "SCREEN-EVENT-012", "SCREEN-EVENT-013" })
                    {
                        var view = Game.Bootstrap.ZoneRuntimeDriver.CameraRect(camera);
                        Assert.IsTrue(runtime.TryStart(id, view), id);
                        var lead = runtime.Active.Hazards.OrderBy(h => h.StartDelay).First();
                        var last = runtime.Active.Hazards.OrderBy(h => h.StartDelay).Last();
                        // The warning shortly before the first strike, then the strike while it is under way.
                        var warningAt = last.StartDelay + last.TelegraphSeconds * .85f;
                        yield return WaitUntil(runtime, Mathf.Min(warningAt, lead.StartDelay + lead.TelegraphSeconds - .05f));
                        var run = Object.FindAnyObjectByType<RunController>();
                        run.TogglePause();
                        capture.transform.position = camera.transform.position;
                        yield return null;
                        capture.Render();
                        UiFoundationSmokeTests.Capture(target, $"field010-{id.ToLowerInvariant()}-warning");
                        run.TogglePause();
                        yield return WaitUntil(runtime, lead.StartDelay + lead.TelegraphSeconds + lead.StrikeSeconds * .05f);
                        Assert.IsNotNull(runtime.Active, "The capture begins inside the live strike.");
                        if (id == "SCREEN-EVENT-005")
                        {
                            var voice = root.GetComponentsInChildren<AudioSource>().FirstOrDefault(source =>
                                source.isPlaying && source.clip != null && source.clip.name == "light-column-v1");
                            Assert.IsNotNull(voice, "A circle strike starts its configured audio family without a player hit.");
                            Assert.IsTrue(voice.mute || AudioListener.volume == 0f, "Automated capture stays silent on speakers.");
                            var frozen = runtime.Active.Elapsed;
                            run.TogglePause();
                            yield return new WaitForSecondsRealtime(.1f);
                            Assert.AreEqual(frozen, runtime.Active.Elapsed);
                            Assert.IsFalse(voice.isPlaying, "Gameplay strike audio pauses with its animation.");
                            run.TogglePause();
                            yield return null;
                            Assert.IsTrue(voice.isPlaying, "Resume continues the same strike voice.");
                        }
                        // PNG encoding can outlast a short strike. Freeze simulation and render exact
                        // presentation-clock samples so disk latency cannot skip the animation frames.
                        run.TogglePause();
                        var artwork = root.ScreenEvents.GetComponentsInChildren<ScreenHazardArtView>();
                        for (var frame = 0; frame < 12; frame++)
                        {
                            var sample = lead.StartDelay + lead.TelegraphSeconds + lead.StrikeSeconds * (.05f + .8f * frame / 11f);
                            foreach (var art in artwork) art.Apply(sample, view);
                            capture.Render();
                            UiFoundationSmokeTests.Capture(target, $"field010-{id.ToLowerInvariant()}-motion-{frame:00}");
                            if (frame == 6) UiFoundationSmokeTests.Capture(target, $"field010-{id.ToLowerInvariant()}-strike");
                        }
                        run.TogglePause();
                        var deadline = Time.realtimeSinceStartup + 12f;
                        while (runtime.Active != null && Time.realtimeSinceStartup < deadline) yield return null;
                        Assert.IsNull(runtime.Active, id);
                        capture.Render();
                        UiFoundationSmokeTests.Capture(target, $"field010-{id.ToLowerInvariant()}-done");
                    }
                }
                finally
                {
                    Object.DestroyImmediate(captureObject);
                    target.Release(); Object.DestroyImmediate(target);
                }
            }
            finally
            {
                if (root != null && root.IsInitialized) root.Shutdown();
            }
        }

        private static IEnumerator WaitUntil(ScreenEventRuntime runtime, float eventSeconds)
        {
            var deadline = Time.realtimeSinceStartup + 12f;
            while (runtime.Active != null && runtime.Active.Elapsed < eventSeconds && Time.realtimeSinceStartup < deadline) yield return null;
        }
    }
}
