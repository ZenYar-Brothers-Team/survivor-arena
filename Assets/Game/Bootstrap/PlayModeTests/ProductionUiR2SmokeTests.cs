using System;
using System.Collections;
using System.Linq;
using Game.ActiveSkill;
using Game.Character;
using Game.Content;
using Game.Presentation;
using Game.Progression;
using Game.Run;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Game.Bootstrap.PlayModeTests
{
    /// <summary>Production assets and owners; arranged build, never the user's saved profile.</summary>
    public sealed class ProductionUiR2SmokeTests
    {
        [Test]
        public void StoneCopy_HidesDerivedFlightLifetime_ButPreservesTrueLifetimeChanges()
        {
            var stone = ProductionActiveSkillCatalog.Create().Single(skill => skill.Id.ToString() == "SKILL-001");
            var text = GameplayUiCopy.DraftEffect(stone, stone.CreateDraftPreview(2, 3));
            StringAssert.Contains("Скорость полёта +15%", text);
            StringAssert.DoesNotContain("Время действия", text);
            StringAssert.Contains("Скорость использования +33%", GameplayUiCopy.DraftEffect(stone, stone.CreateDraftPreview(4, 5)));
            var lifetimeOnly = new DraftOptionPreview(2, 3, new[] { new DraftValueChange("Wave 1: ProjectileBurst: lifetime", 2, 3) });
            StringAssert.Contains("Время действия +50%", GameplayUiCopy.DraftEffect(stone, lifetimeOnly));
        }

        [UnityTest]
        public IEnumerator Production_HudDraftPause_AnchorsAndRetry_WithTwoResolutionCaptures()
        {
            ProductionSmokeScene.Load(); yield return null; yield return null;
            var composition = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            CharacterSelectionSmokeDriver.StartDefault(composition); yield return null;
            var ui = Object.FindAnyObjectByType<GameplayUiRoot>();
            var draft = Object.FindAnyObjectByType<LevelUpDraftRuntime>();
            var run = Object.FindAnyObjectByType<RunController>();
            var player = Object.FindAnyObjectByType<PlayerCharacterRuntime>();
            var camera = Camera.main;
            RenderTexture target = null;
            try
            {
                var active = new[] { "SKILL-001", "SKILL-002", "SKILL-003", "SKILL-004", "SKILL-007", "SKILL-013" };
                var passive = new[] { "PASSIVE-001", "PASSIVE-002", "PASSIVE-004", "PASSIVE-008", "PASSIVE-009", "PASSIVE-011" };
                for (var i = 0; i < active.Length; i++)
                    ApplyToLevel(draft.Build, composition.Catalog.BuildEntries.Single(e => e.Id.ToString() == active[i]), new[] { 3, 2, 4, 3, 2, 3 }[i]);
                for (var i = 0; i < passive.Length; i++)
                    ApplyToLevel(draft.Build, composition.Catalog.BuildEntries.Single(e => e.Id.ToString() == passive[i]), new[] { 2, 3, 2, 2, 1, 2 }[i]);
                draft.Build.Apply(composition.Catalog.Sets.Single(s => s.Id.ToString() == "SET-001"));
                // Publish the arranged build through the normal selection event.
                Assert.IsTrue(draft.RequestBook(Guid.NewGuid(), run.Model.RunId, new ContentId("PICKUP-002")));
                Assert.IsTrue(draft.Select(draft.CurrentDraft.Options[0].Definition.Id));
                DrainBookChoices(draft);
                var originalCameraTarget = camera.targetTexture;
                foreach (var size in new[] { new Vector2Int(1920, 1080), new Vector2Int(1280, 720) })
                {
                    target = new RenderTexture(size.x, size.y, 24); target.Create();
                    camera.targetTexture = target;
                    ui.Document.panelSettings.targetTexture = target;
                    ui.Document.panelSettings.clearColor = false;
                    yield return null; yield return null;
                    var tree = ui.Document.rootVisualElement;
                    Assert.IsNull(tree.Q<Button>(GameplayUiElementIds.PauseButton));
                    Assert.AreEqual(6, tree.Q(GameplayUiElementIds.ActiveSlots).childCount);
                    Assert.AreEqual(6, tree.Q(GameplayUiElementIds.PassiveSlots).childCount);
                    Assert.IsNotNull(tree.Q<Image>(GameplayUiElementIds.PausePortrait).sprite);
                    Assert.IsTrue(tree.Q(GameplayUiElementIds.PauseFooter).Contains(tree.Q(GameplayUiElementIds.ShellPauseSettings)));
                    AssertAnchor(ui, camera, player);
                    Assert.AreEqual(10, tree.Q(GameplayUiElementIds.HealthBar).Q(className: "unity-progress-bar__container").layout.height, 1);
                    player.transform.position += new Vector3(1.5f, .5f); yield return null;
                    AssertAnchor(ui, camera, player);
                    var baseline = camera.transform.position;
                    camera.transform.position += new Vector3(.18f, -.12f);
                    AssertAnchor(ui, camera, player); camera.transform.position = baseline;
                    yield return null; yield return null;
                    UiFoundationSmokeTests.Capture(target, $"r2-production-hud-{size.x}");

                    Assert.IsTrue(draft.RequestBook(Guid.NewGuid(), run.Model.RunId, new ContentId("PICKUP-002")));
                    yield return null; yield return null;
                    foreach (var label in tree.Q(GameplayUiElementIds.DraftOptions).Query<Label>().ToList()) StringAssert.DoesNotContain("Base damage", label.text);
                    foreach (var card in tree.Q(GameplayUiElementIds.DraftOptions).Children())
                    {
                        var icon = card.Q<Image>(GameplayUiElementIds.CardIcon);
                        var type = card.Q<Label>(GameplayUiElementIds.CardType);
                        var level = card.Q<Label>(GameplayUiElementIds.CardLevel);
                        var title = card.Q<Label>(GameplayUiElementIds.CardTitle);
                        Assert.IsNotNull(icon.sprite);
                        CollectionAssert.Contains(new[] { "Активное", "Пассивное", "Сет" }, type.text);
                        Assert.AreEqual(size.x == 1280 ? 88 : 112, icon.worldBound.width, 1);
                        Assert.AreEqual(size.x == 1280 ? 21 : 24, type.resolvedStyle.fontSize);
                        Assert.AreEqual(size.x == 1280 ? 18 : 21, level.resolvedStyle.fontSize);
                        Assert.AreEqual(0, type.resolvedStyle.backgroundColor.a);
                        Assert.AreEqual(0, type.resolvedStyle.borderTopLeftRadius);
                        Assert.LessOrEqual(icon.worldBound.xMax, type.worldBound.xMin);
                        Assert.Greater(type.worldBound.yMax, icon.worldBound.yMin);
                        Assert.LessOrEqual(level.worldBound.yMax, icon.worldBound.yMax + 1);
                        Assert.GreaterOrEqual(title.worldBound.yMin, icon.worldBound.yMax);
                        Assert.AreEqual(icon.worldBound.xMin, title.worldBound.xMin, 1);
                    }
                    foreach (var recipe in tree.Q<ScrollView>(GameplayUiElementIds.DraftRecipeList).Children())
                    {
                        var icon = recipe.Q<Image>(GameplayUiElementIds.CardIcon);
                        Assert.IsNotNull(icon.sprite, "Related sets must use their resolved production icons.");
                        Assert.LessOrEqual(icon.worldBound.xMax, recipe.Q<Label>(GameplayUiElementIds.CardTitle).worldBound.xMin);
                    }
                    var componentRows = tree.Q(GameplayUiElementIds.DraftDetails).Query<Label>().ToList();
                    Assert.IsNotEmpty(componentRows);
                    Assert.IsTrue(componentRows.Any(row => row.ClassListContains("draft-component-met")));
                    foreach (var row in componentRows.Where(row => row.ClassListContains("draft-component-met")))
                    {
                        StringAssert.Contains("Уровень набран", row.text);
                        Assert.Greater(row.resolvedStyle.color.g, row.resolvedStyle.color.r);
                    }
                    UiFoundationSmokeTests.Capture(target, $"r2-production-book-{size.x}");
                    UiFoundationSmokeTests.Submit(tree.Q<Button>(GameplayUiElementIds.DraftSelectButton(0)));
                    DrainBookChoices(draft);
                    Assert.IsFalse(draft.IsDraftOpen);

                    run.Model.Pause(); yield return null; yield return null;
                    Assert.AreEqual(RunState.Paused, run.Model.State);
                    var footer = tree.Q(GameplayUiElementIds.PauseFooter).worldBound;
                    Assert.LessOrEqual(footer.yMax, size.y);
                    Assert.LessOrEqual(tree.Q(GameplayUiElementIds.PauseSlots).worldBound.yMax, footer.yMin);
                    UiFoundationSmokeTests.Capture(target, $"r2-production-pause-{size.x}");
                    UiFoundationSmokeTests.Submit(tree.Q(GameplayUiElementIds.ReceivedSets).Q<Button>());
                    yield return null;
                    Assert.IsNotEmpty(tree.Q<Label>(GameplayUiElementIds.SetPopupEffect).text);
                    UiFoundationSmokeTests.Capture(target, $"r2-production-set-info-{size.x}");
                    Assert.IsTrue(ui.ConsumePauseShortcut(false));
                    Assert.AreEqual(RunState.Paused, run.Model.State);
                    UiFoundationSmokeTests.Submit(tree.Q<Button>(GameplayUiElementIds.ShellPauseSettings));
                    UiFoundationSmokeTests.Submit(composition.ShellDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.SettingsBack));
                    Assert.AreEqual(RunState.Paused, run.Model.State);
                    UiFoundationSmokeTests.Submit(tree.Q<Button>(GameplayUiElementIds.RunOverlayResumeButton));
                    Assert.AreEqual(RunState.Running, run.Model.State);
                    ui.Document.panelSettings.targetTexture = null; camera.targetTexture = originalCameraTarget;
                    Object.DestroyImmediate(target); target = null;
                }
                var oldHealth = ui.Document.rootVisualElement.Q(GameplayUiElementIds.HealthBar);
                player.TakeDamage(player.Health.MaxHealth * 100); yield return null;
                Assert.AreEqual(RunState.Lost, run.Model.State);
                composition.RetryProfileRun(); yield return null; yield return null;
                Assert.IsNull(oldHealth.panel);
                Assert.AreNotSame(oldHealth, ui.Document.rootVisualElement.Q(GameplayUiElementIds.HealthBar));
                AssertAnchor(ui, camera, player);
            }
            finally
            {
                if (camera != null) camera.targetTexture = null;
                if (target != null) Object.DestroyImmediate(target);
                composition.Shutdown();
            }
        }

        private static void ApplyToLevel(PlayerBuild build, BuildEntryDefinition definition, int level)
        {
            while (!build.TryGetEntry(definition.Id, out var entry) || entry.Level < level) build.Apply(definition);
        }

        private static void AssertAnchor(GameplayUiRoot ui, Camera camera, PlayerCharacterRuntime player)
        {
            ui.RefreshHealthAnchor();
            var tree = ui.Document.rootVisualElement;
            var body = player.GetComponentInChildren<SpritePresentationRig>().BodyRenderer.bounds;
            var projected = camera.WorldToViewportPoint(new Vector3(body.center.x, body.max.y, body.center.z));
            var bar = tree.Q<ProgressBar>(GameplayUiElementIds.HealthBar);
            Assert.AreEqual(projected.x * tree.layout.width - 32, bar.style.left.value.value, .1f);
            Assert.AreEqual((1 - projected.y) * tree.layout.height - 18, bar.style.top.value.value, .1f);
            Assert.AreEqual(PickingMode.Ignore, bar.pickingMode);
        }

        /// <summary>DECISION-0093: a Book may queue up to three choices; resolve the rest through the runtime.</summary>
        private static void DrainBookChoices(LevelUpDraftRuntime draft)
        {
            for (var i = 0; i < 3 && draft.IsDraftOpen; i++)
                Assert.IsTrue(draft.Select(draft.CurrentDraft.Options[0].Definition.Id));
        }
    }
}
