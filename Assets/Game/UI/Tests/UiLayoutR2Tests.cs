using Game.Character;
using Game.Progression;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace Game.UI.Tests
{
    public sealed class UiLayoutR2Tests
    {
        [Test]
        public void DraftComponents_PresenceAndCurrentLevelMet_AreIndependent()
        {
            var root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/Resources/UI/GameplayUi.uxml").CloneTree();
            using var view = new UiToolkitGameplayView(root);
            var components = new[] {
                new RecipeComponentViewState("Нет предмета", 0, 1, 1, true),
                new RecipeComponentViewState("Есть, не прокачан", 1, 3, 3, true),
                new RecipeComponentViewState("Прокачан", 4, 4, 3, false) };
            var recipe = new RecipeProjectionViewState("Рецепт", 1, 3, 3, true, false, null,
                ownedComponents: 2, componentStates: components);
            var option = new DraftOptionViewState(new Game.Content.ContentId("TEST-COMPONENTS"), "Умение", "Эффект", recipes: new[] { recipe });
            view.RenderDraft(new DraftViewState(true, 0, 0, new[] { option }, System.Guid.NewGuid()));
            var rows = root.Q(GameplayUiElementIds.DraftDetails).Query<Label>().ToList();
            StringAssert.StartsWith("○", rows[0].text);
            StringAssert.StartsWith("✓", rows[1].text);
            Assert.IsFalse(rows[0].ClassListContains("draft-component-met"));
            Assert.IsFalse(rows[1].ClassListContains("draft-component-met"));
            Assert.IsTrue(rows[2].ClassListContains("draft-component-met"));
            StringAssert.Contains("Уровень набран", rows[2].text);
            Assert.AreEqual("2/3", recipe.Progress);
        }

        [TestCase(2f, 1.5f, "Скорость использования +33%")]
        [TestCase(2f, 1f, "Скорость использования +100%")]
        [TestCase(1f, 2f, "Скорость использования -50%")]
        public void CooldownCopy_UsesReciprocalFrequency(float before, float after, string expected)
        {
            Assert.AreEqual(expected, GameplayUiCopy.Describe(new DraftValueChange("Base cooldown", before, after), false, true));
        }

        [Test]
        public void CooldownCopy_NewSkill_ShowsNoCooldownSeconds()
        {
            Assert.AreEqual("", GameplayUiCopy.Describe(new DraftValueChange("Base cooldown", 0f, 2.4f), true, true));
            var skill = System.Linq.Enumerable.Single(Game.ActiveSkill.ProductionActiveSkillCatalog.Create(), s => s.Id.ToString() == "SKILL-001");
            StringAssert.DoesNotContain("Перезарядка", GameplayUiCopy.DraftEffect(skill, skill.CreateDraftPreview(0, 1)));
        }

        [TestCase(12.5f, "+13")]
        [TestCase(-12.5f, "-13")]
        [TestCase(-.1f, "+0")]
        public void PercentCopy_RoundsToWholeWithoutNegativeZero(float value, string expected)
        {
            Assert.AreEqual(expected, GameplayUiCopy.SignedPercent(value));
        }

        [Test]
        public void DraftRecipes_RenderProjectedIconTitleAndProgress()
        {
            var icon = UnityEngine.Sprite.Create(UnityEngine.Texture2D.whiteTexture,
                new UnityEngine.Rect(0, 0, 1, 1), UnityEngine.Vector2.zero);
            try
            {
                var root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/Resources/UI/GameplayUi.uxml").CloneTree();
                using var view = new UiToolkitGameplayView(root);
                var recipe = new RecipeProjectionViewState("Связанный сет", 1, 2, 3, false, false,
                    System.Array.Empty<string>(), icon: icon);
                var noIcon = new RecipeProjectionViewState("Без иконки", 0, 1, 3, false, false, System.Array.Empty<string>());
                var option = new DraftOptionViewState(new Game.Content.ContentId("TEST-ICON"), "Умение", "Эффект",
                    recipes: new[] { recipe, noIcon });
                view.RenderDraft(new DraftViewState(true, 0, 0, new[] { option }, System.Guid.NewGuid()));
                var button = root.Q<Button>(GameplayUiElementIds.DraftRecipeButton(0));
                Assert.AreSame(icon, button.Q<Image>(GameplayUiElementIds.CardIcon).sprite);
                Assert.AreEqual(recipe.Title, button.Q<Label>(GameplayUiElementIds.CardTitle).text);
                Assert.AreEqual(recipe.Progress, button.Q<Label>(GameplayUiElementIds.CardStatus).text);
                Assert.AreEqual(PickingMode.Ignore, button.Q<Image>(GameplayUiElementIds.CardIcon).pickingMode);
                Assert.IsNull(root.Q<Button>(GameplayUiElementIds.DraftRecipeButton(1)).Q<Image>(GameplayUiElementIds.CardIcon).sprite);
            }
            finally { UnityEngine.Object.DestroyImmediate(icon); }
        }

        [Test]
        public void DamageCopy_HidesAbsoluteBaseAndQualifiesRetention()
        {
            Assert.IsEmpty(GameplayUiCopy.Describe(new DraftValueChange("Base damage", 0, 14), true, true));
            Assert.AreEqual("Урон +30%", GameplayUiCopy.Describe(new DraftValueChange("Base damage", 14, 18.2f), false, true));
            Assert.AreEqual("Повторные удары слабее теряют силу", GameplayUiCopy.Describe(new DraftValueChange("Wave 1: damage retention", .8f, .9f), false, true));
            StringAssert.DoesNotContain("80%", GameplayUiCopy.Describe(new DraftValueChange("Wave 1: ricochets", 0, 1), false, true));
            StringAssert.Contains("Урон +5% → +10%", GameplayUiCopy.Describe(new DraftValueChange("Damage", 5, 10, "%"), false, false));
            Assert.AreEqual("Урон +13% → +26%", GameplayUiCopy.Describe(new DraftValueChange("Damage", 12.5f, 25.5f, "%"), false, false));
        }

        [Test]
        public void RegenerationCopy_PreservesSmallUpgradeValues()
        {
            Assert.AreEqual("Регенерация +0.35/с → +0.5/с",
                GameplayUiCopy.Describe(new DraftValueChange("Regeneration", .35f, .5f, " HP/s"), false, false));
        }

        [TestCase(3, 100)] [TestCase(3.6f, 120)] [TestCase(4.5f, 150)]
        public void PauseSpeed_UsesCommonBaseline(float current, int percent)
        {
            var root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/Resources/UI/GameplayUi.uxml").CloneTree();
            using var view = new UiToolkitGameplayView(root);
            view.RenderHud(new HudViewState(50, 100, 0, 1, 0,
                new WaveViewState(1, 1, "", Game.Enemy.WavePhaseTag.Ordinary),
                new CharacterStatsViewState(new CharacterStats(new CharacterBaseStats(100, current))), baselineMovementSpeed: 3));
            StringAssert.Contains($"Скорость   {percent}%", root.Q<Label>(GameplayUiElementIds.PauseStats).text);
            Assert.IsEmpty(root.Q<ProgressBar>(GameplayUiElementIds.HealthBar).title);
            StringAssert.Contains("50/100", root.Q<Label>(GameplayUiElementIds.PauseCharacter).text);
        }

        [TestCase(1, 2)] [TestCase(2, 3)]
        public void Reachability_MissingMoreThanFreeSlots_IsImpossible(int free, int missing)
        {
            var active = new BuildEntryDefinition("TEST-A", BuildEntryKind.ActiveSkill, "Active");
            var build = new PlayerBuild(active);
            for (var i = 0; i < PlayerBuild.PassiveSlotCapacity - free; i++)
                build.Apply(new BuildEntryDefinition($"TEST-OWNED-{i}", BuildEntryKind.PassiveItem, "Owned"));
            var recipe = new SetRecipeComponent[missing + 1];
            recipe[0] = new SetRecipeComponent(active.Id, active.Kind, 1);
            for (var i = 0; i < missing; i++) recipe[i + 1] = new SetRecipeComponent($"TEST-MISSING-{i}", BuildEntryKind.PassiveItem, 1);
            Assert.IsFalse(new SetDefinition("TEST-SET", "Set", recipe).CanStillBeFulfilled(build, _ => true));
        }
    }
}
