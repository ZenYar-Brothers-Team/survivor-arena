using System;
using Game.Content;
using Game.Enemy;
using Game.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    public sealed class UiToolkitGameplayView : IGameplayUiView, IDisposable
    {
        private readonly VisualElement _draftDetails;
        private readonly VisualElement _root;
        private readonly PauseBuildPanel _pause;
        private readonly VisualElement _recipeInspector;
        private readonly ScrollView _recipeList;
        private readonly Label _recipeTitle;
        private readonly Label _recipeEffect;
        private DraftViewState _draftState;
        private readonly Label _pauseCharacter;
        private readonly VisualElement _pauseBuild;
        private string _characterName = "";
        private string _healthText = "";
        private BuildViewState? _renderedBuild;
        private CharacterSelectionViewState _renderedCharacters;
        private readonly ProgressBar _healthBar;
        private readonly ProgressBar _bossBar;
        private readonly ProgressBar _experienceBar;
        private readonly Label _levelLabel;
        private readonly Label _timerLabel;
        private readonly Label _waveLabel;
        private readonly Button _speedHalfButton;
        private readonly Button _speedNormalButton;
        private readonly Button _speedDoubleButton;
        private readonly Button _speedTripleButton;
        private readonly Button _speedQuintupleButton;
        private readonly VisualElement _activeSlots;
        private readonly VisualElement _passiveSlots;
        private readonly VisualElement _sets;
        private readonly VisualElement _setRecipeProgress;
        private readonly VisualElement _draftOverlay;
        private readonly VisualElement _draftOptions;
        private readonly Label _draftHeading;
        private readonly Label _draftQueue;
        private readonly Label _bookCurrency;
        private readonly Button _addBookButton;
        private Guid _renderedDraftRevision;
        private bool _renderedBanishMode;
        private readonly Button _banishModeButton;
        private readonly Label _draftControlHint;
        private readonly Button _rerollButton;
        private readonly Label _banishCount;
        private readonly VisualElement _runOverlay;
        private readonly Label _runOverlayTitle;
        private readonly Button _runOverlayResumeButton;
        private readonly Button _developmentToggleButton;
        private readonly VisualElement _developmentPanel;
        private readonly Button _developmentCloseButton;
        private readonly Button _developmentRunTab;
        private readonly Button _developmentBuildTab;
        private readonly Button _developmentPresentationTab;
        private readonly Button _developmentPlaytestTab;
        private readonly VisualElement _developmentPlaytestPane;
        private readonly Button _developmentTravelersTab;
        private readonly VisualElement _developmentTravelersPane;
        private readonly VisualElement _developmentRunPane;
        private readonly VisualElement _developmentBuildPane;
        private readonly VisualElement _developmentPresentationPane;
        private readonly Button _addExperienceButton;
        private readonly Button _addLargeExperienceButton;
        private readonly Button _addRerollsButton;
        private readonly Button _unlockAllDraftEntriesButton;
        private readonly Button _damageButton;
        private readonly Button _healButton;
        private readonly Button _healthLockButton;
        private readonly Label _enemyObservation;
        private readonly Label _waveObservation;
        private readonly Label _skillObservation;
        private readonly Label _statsObservation;
        private readonly Label _experienceObservation;
        private readonly Button _presentationLiveButton;
        private readonly Button _presentationIdleButton;
        private readonly Button _presentationLeftButton;
        private readonly Button _presentationRightButton;
        private readonly Button _presentationResetButton;
        private readonly VisualElement _characterSelection;
        private bool _developmentControlsAvailable;
        private bool _developmentPanelExpanded;

        public event Action<ContentId, Guid> DraftOptionSelected;
        public event Action<Guid> DraftRerollRequested;
        public event Action<Guid> DraftBanishModeRequested;
        public event Action PauseRequested;
        public event Action<float> SpeedRequested;
        public event Action AddExperienceRequested;
        public event Action AddLargeExperienceRequested;
        public event Action AddRerollsRequested;
        public event Action UnlockAllDraftEntriesRequested;
        public event Action AddBookRequested;
        public event Action ApplyDamageRequested;
        public event Action ApplyHealingRequested;
        public event Action ToggleHealthLockRequested;
        public event Action<SpritePresentationPreviewMotion> PresentationMotionPreviewRequested;
        public event Action PresentationResetRequested;

        public UiToolkitGameplayView(VisualElement root)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            _root = root;
            _root.styleSheets.Add(Resources.Load<StyleSheet>("UI/FolioChromeStyles"));
            FolioPanelTexture.Attach(root.Q(className: "draft-panel"));
            FolioPanelTexture.Attach(root.Q(className: "pause-panel"));
            FolioPanelTexture.Attach(root.Q(className: "pause-left"));
            FolioPanelTexture.Attach(root.Q(className: "pause-right"));
            _root.EnableInClassList("ui-compact", root.layout.width > 0 && root.layout.width < 1600);
            _pause = new PauseBuildPanel(root);
            _root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            _recipeInspector = Require<VisualElement>(root, GameplayUiElementIds.DraftRecipeInspector);
            _recipeList = Require<ScrollView>(root, GameplayUiElementIds.DraftRecipeList);
            _recipeTitle = Require<Label>(root, GameplayUiElementIds.DraftRecipeTitle);
            _recipeEffect = Require<Label>(root, GameplayUiElementIds.DraftRecipeEffect);
            _draftDetails = Require<VisualElement>(root, GameplayUiElementIds.DraftDetails);
            _pauseCharacter = Require<Label>(root, GameplayUiElementIds.PauseCharacter);
            _pauseBuild = Require<VisualElement>(root, GameplayUiElementIds.PauseBuild);
            _healthBar = Require<ProgressBar>(root, GameplayUiElementIds.HealthBar);
            SetVisible(_healthBar, false); // The world anchor reveals it only after valid projection.
            _bossBar = Require<ProgressBar>(root, GameplayUiElementIds.BossBar);
            SetVisible(_bossBar, false);
            _experienceBar = Require<ProgressBar>(root, GameplayUiElementIds.ExperienceBar);
            _levelLabel = Require<Label>(root, GameplayUiElementIds.LevelLabel);
            _timerLabel = Require<Label>(root, GameplayUiElementIds.TimerLabel);
            _waveLabel = Require<Label>(root, GameplayUiElementIds.WaveLabel);
            _speedHalfButton = Require<Button>(root, GameplayUiElementIds.SpeedHalfButton);
            _speedNormalButton = Require<Button>(root, GameplayUiElementIds.SpeedNormalButton);
            _speedDoubleButton = Require<Button>(root, GameplayUiElementIds.SpeedDoubleButton);
            _speedTripleButton = Require<Button>(root, GameplayUiElementIds.SpeedTripleButton);
            _speedQuintupleButton = Require<Button>(root, GameplayUiElementIds.SpeedQuintupleButton);
            _activeSlots = Require<VisualElement>(root, GameplayUiElementIds.ActiveSlots);
            _passiveSlots = Require<VisualElement>(root, GameplayUiElementIds.PassiveSlots);
            _activeSlots.pickingMode = _passiveSlots.pickingMode = PickingMode.Ignore;
            _healthBar.Query<VisualElement>().ForEach(element => element.pickingMode = PickingMode.Ignore);
            _experienceBar.Query<VisualElement>().ForEach(element => element.pickingMode = PickingMode.Ignore);
            _sets = Require<VisualElement>(root, GameplayUiElementIds.Sets);
            _setRecipeProgress = Require<VisualElement>(root, GameplayUiElementIds.SetRecipeProgress);
            _draftOverlay = Require<VisualElement>(root, GameplayUiElementIds.DraftOverlay);
            _draftOptions = Require<VisualElement>(root, GameplayUiElementIds.DraftOptions);
            _draftHeading = Require<Label>(root, GameplayUiElementIds.DraftHeading);
            _draftQueue = Require<Label>(root, GameplayUiElementIds.DraftQueue);
            _bookCurrency = Require<Label>(root, GameplayUiElementIds.BookCurrency);
            _addBookButton = Require<Button>(root, GameplayUiElementIds.AddBookButton);
            _rerollButton = Require<Button>(root, GameplayUiElementIds.DraftRerollButton);
            _banishCount = Require<Label>(root, GameplayUiElementIds.DraftBanishCount);
            _banishModeButton = Require<Button>(root, GameplayUiElementIds.DraftBanishModeButton);
            _draftControlHint = Require<Label>(root, GameplayUiElementIds.DraftControlHint);
            _runOverlay = Require<VisualElement>(root, GameplayUiElementIds.RunOverlay);
            _runOverlayTitle = Require<Label>(root, GameplayUiElementIds.RunOverlayTitle);
            _runOverlayResumeButton = Require<Button>(root, GameplayUiElementIds.RunOverlayResumeButton);
            _developmentToggleButton = Require<Button>(root, GameplayUiElementIds.DevelopmentToggleButton);
            _developmentPanel = Require<VisualElement>(root, GameplayUiElementIds.DevelopmentPanel);
            _developmentCloseButton = Require<Button>(root, GameplayUiElementIds.DevelopmentCloseButton);
            _developmentRunTab = Require<Button>(root, GameplayUiElementIds.DevelopmentRunTab);
            _developmentBuildTab = Require<Button>(root, GameplayUiElementIds.DevelopmentBuildTab);
            _developmentPresentationTab = Require<Button>(root, GameplayUiElementIds.DevelopmentPresentationTab);
            _developmentPlaytestTab = Require<Button>(root, GameplayUiElementIds.DevelopmentPlaytestTab);
            _developmentPlaytestPane = Require<VisualElement>(root, GameplayUiElementIds.DevelopmentPlaytestPane);
            _developmentTravelersTab = Require<Button>(root, GameplayUiElementIds.DevelopmentTravelersTab);
            _developmentTravelersPane = Require<VisualElement>(root, GameplayUiElementIds.DevelopmentTravelersPane);
            _developmentRunPane = Require<VisualElement>(root, GameplayUiElementIds.DevelopmentRunPane);
            _developmentBuildPane = Require<VisualElement>(root, GameplayUiElementIds.DevelopmentBuildPane);
            _developmentPresentationPane = Require<VisualElement>(root, GameplayUiElementIds.DevelopmentPresentationPane);
            _addExperienceButton = Require<Button>(root, GameplayUiElementIds.AddExperienceButton);
            _addLargeExperienceButton = Require<Button>(root, GameplayUiElementIds.AddLargeExperienceButton);
            _addRerollsButton = Require<Button>(root, GameplayUiElementIds.AddRerollsButton);
            _unlockAllDraftEntriesButton = Require<Button>(root, GameplayUiElementIds.UnlockAllDraftEntriesButton);
            _damageButton = Require<Button>(root, GameplayUiElementIds.DamageButton);
            _healButton = Require<Button>(root, GameplayUiElementIds.HealButton);
            _healthLockButton = Require<Button>(root, GameplayUiElementIds.HealthLockButton);
            _enemyObservation = Require<Label>(root, GameplayUiElementIds.EnemyObservation);
            _skillObservation = Require<Label>(root, GameplayUiElementIds.SkillObservation);
            _waveObservation = Require<Label>(root, GameplayUiElementIds.WaveObservation);
            _statsObservation = Require<Label>(root, GameplayUiElementIds.StatsObservation);
            _experienceObservation = Require<Label>(root, GameplayUiElementIds.ExperienceObservation);
            _presentationLiveButton = Require<Button>(root, GameplayUiElementIds.PresentationLiveButton);
            _presentationIdleButton = Require<Button>(root, GameplayUiElementIds.PresentationIdleButton);
            _presentationLeftButton = Require<Button>(root, GameplayUiElementIds.PresentationLeftButton);
            _presentationRightButton = Require<Button>(root, GameplayUiElementIds.PresentationRightButton);
            _presentationResetButton = Require<Button>(root, GameplayUiElementIds.PresentationResetButton);
            _characterSelection = Require<VisualElement>(root, GameplayUiElementIds.CharacterSelection);

            _speedHalfButton.clicked += HandleHalfSpeedClicked;
            _speedNormalButton.clicked += HandleNormalSpeedClicked;
            _speedDoubleButton.clicked += HandleDoubleSpeedClicked;
            _speedTripleButton.clicked += HandleTripleSpeedClicked;
            _speedQuintupleButton.clicked += HandleQuintupleSpeedClicked;
            _runOverlayResumeButton.clicked += HandlePauseClicked;
            _rerollButton.clicked += HandleRerollClicked;
            _banishModeButton.clicked += HandleBanishModeClicked;
            _addExperienceButton.clicked += HandleAddExperienceClicked;
            _addLargeExperienceButton.clicked += HandleAddLargeExperienceClicked;
            _addRerollsButton.clicked += HandleAddRerollsClicked;
            _unlockAllDraftEntriesButton.clicked += HandleUnlockAllDraftEntriesClicked;
            _addBookButton.clicked += HandleAddBookClicked;
            _damageButton.clicked += HandleDamageClicked;
            _healButton.clicked += HandleHealingClicked;
            _healthLockButton.clicked += HandleHealthLockClicked;
            _developmentToggleButton.clicked += HandleDevelopmentToggleClicked;
            _developmentCloseButton.clicked += HandleDevelopmentCloseClicked;
            _developmentRunTab.clicked += ShowDevelopmentRunTab;
            _developmentBuildTab.clicked += ShowDevelopmentBuildTab;
            _developmentPresentationTab.clicked += ShowDevelopmentPresentationTab;
            _developmentPlaytestTab.clicked += ShowDevelopmentPlaytestTab;
            _developmentTravelersTab.clicked += ShowDevelopmentTravelersTab;
            _presentationLiveButton.clicked += HandlePresentationLiveClicked;
            _presentationIdleButton.clicked += HandlePresentationIdleClicked;
            _presentationLeftButton.clicked += HandlePresentationLeftClicked;
            _presentationRightButton.clicked += HandlePresentationRightClicked;
            _presentationResetButton.clicked += HandlePresentationResetClicked;
            ShowDevelopmentRunTab();
            UpdateDevelopmentVisibility();
        }

        public void RenderHud(HudViewState state)
        {
            RenderSpeedButton(_speedHalfButton, .5f, state);
            RenderSpeedButton(_speedNormalButton, 1, state);
            RenderSpeedButton(_speedDoubleButton, 2, state);
            RenderSpeedButton(_speedTripleButton, 3, state);
            RenderSpeedButton(_speedQuintupleButton, 5, state);
            SetVisible(_bossBar, state.Boss.Visible);
            if (state.Boss.Visible)
            {
                _bossBar.value = 100f * state.Boss.CurrentHealth / state.Boss.MaxHealth;
                // DECISION-0110: health bars never print HP numbers; the final boss bar keeps only its name.
                _bossBar.title = state.Boss.Name;
            }
            _bookCurrency.text = $"Из книг: +{state.BookCurrency}";
            SetVisible(_bookCurrency, state.BookCurrency > 0);
            var health01 = state.MaxHealth > 0f ? state.CurrentHealth / state.MaxHealth : 0f;
            _healthBar.value = health01 * 100f;
            _healthBar.title = "";
            _healthText = $"Здоровье {MathF.Ceiling(state.CurrentHealth)}/{MathF.Ceiling(state.MaxHealth)}";
            _experienceBar.value = state.ExperienceProgress01 * 100f;
            _experienceBar.title = "";
            _levelLabel.text = $"Ур. {state.Level}";
            var remaining = Math.Max(0, (int)Math.Ceiling(state.RunDurationSeconds - state.ElapsedSeconds));
            _timerLabel.text = $"{remaining / 60:00}:{remaining % 60:00}";
            RenderWave(state.Wave);
            _healthLockButton.text = state.IsHealthLocked ? "HP locked" : "Lock HP";
            _healthLockButton.EnableInClassList("development-lock-active", state.IsHealthLocked);
            if (!string.IsNullOrEmpty(state.CharacterName)) _characterName = state.CharacterName;
            _root.Q<Image>(GameplayUiElementIds.PausePortrait).sprite = state.CharacterPortrait;
            if (state.Stats != null)
            {
                var speed = state.BaselineMovementSpeed.HasValue
                    ? $"{MathF.Round(100 * state.Stats.MovementSpeed / state.BaselineMovementSpeed.Value):0}%" : "—";
                _root.Q<Label>(GameplayUiElementIds.PauseStats).text = $"Скорость   {speed}   ·   Темп   +{state.Stats.ActionSpeedBonus:P0}\n" +
                    $"Регенерация   {state.Stats.Regeneration:0.##}/с   ·   Защита   {1 - state.Stats.IncomingDamageMultiplier:P0}";
            }
            _pauseCharacter.text = $"{_characterName}\n{_healthText}\n{_levelLabel.text}";
            if (_developmentControlsAvailable && state.ExperienceTotals != null)
            {
                var xp = state.ExperienceTotals;
                _experienceObservation.text = $"XP collected {xp.CollectedBase:0.##} → {xp.CollectedAwarded:0.##} awarded\n" +
                    $"Expired {xp.ExpiredBase:0.##} · recovered {xp.RecoveredAwarded:0.##}\n" +
                    $"Intervention {xp.InterventionAwarded:0.##} · total awarded {xp.TotalAwarded:0.##}";
            }
            if (_developmentControlsAvailable && state.Stats != null)
            {
                var stats = state.Stats;
                _statsObservation.text = $"Action speed +{stats.ActionSpeedBonus:P0} · XP radius {stats.PickupRadius:0.##}\n" +
                    $"Knockback resist {stats.KnockbackResistance:P0} · outgoing x{stats.OutgoingKnockbackMultiplier:0.##}\n" +
                    $"Size x{stats.EffectSizeMultiplier:0.##} · range x{stats.EffectRangeMultiplier:0.##}\n" +
                    $"Potion drop x{stats.PotionDropMultiplier:0.##} · low-HP damage x{stats.LowHealthDamageMultiplier:0.##}\nKnockback remaining {stats.KnockbackRemaining:0.##} s";
            }
        }

        private void RenderWave(WaveViewState wave)
        {
            _waveLabel.text = wave.PhaseCount > 0
                ? $"WAVE {wave.PhaseNumber}/{wave.PhaseCount} · {wave.DisplayName.ToUpperInvariant()}"
                : "WAVE —";
            foreach (WavePhaseTag tag in Enum.GetValues(typeof(WavePhaseTag)))
                _waveLabel.EnableInClassList($"hud-wave--{tag.ToString().ToLowerInvariant()}", tag == wave.Tag);
        }

        public void RenderDraft(DraftViewState state)
        {
            SetVisible(_draftOverlay, state.IsVisible);
            if (state.IsVisible) _pause.Close(false);
            _draftHeading.text = state.Heading;
            _draftQueue.text = state.QueueDetail;
            _draftOverlay.EnableInClassList("draft-book", state.Heading == "Книга странника");
            if (!state.IsVisible)
            {
                _draftOptions.Clear();
                _renderedDraftRevision = Guid.Empty;
                return;
            }

            _rerollButton.text = $"Обновить · {state.RemainingRerolls}";
            _rerollButton.SetEnabled(state.CanReroll);
            _banishModeButton.text = state.IsBanishMode ? "Отмена исключения" : "Исключить";
            _banishModeButton.SetEnabled(state.CanBanish);
            _draftControlHint.text = state.IsBanishMode
                ? "Нажми на карточку, чтобы исключить вариант."
                : "Наведи на карточку для просмотра сетов; нажми для выбора.";
            _draftOverlay.EnableInClassList("draft-banish-mode", state.IsBanishMode);
            _banishCount.text = $"Исключений: {state.RemainingBanishes}";
            if (state.Revision != Guid.Empty && _renderedDraftRevision == state.Revision && _renderedBanishMode == state.IsBanishMode) return;
            _renderedDraftRevision = state.Revision;
            _renderedBanishMode = state.IsBanishMode;
            _draftState = state;
            _draftOptions.Clear();
            _recipeInspector.style.visibility = Visibility.Hidden;
            for (var i = 0; i < 3; i++)
            {
                var option = i < state.Options.Count ? state.Options[i] :
                    new DraftOptionViewState(default, "Нет варианта", "", false);
                var index = i;
                var select = new DraftCard(option, () => InspectDraft(index), () =>
                {
                    if (_renderedDraftRevision == state.Revision)
                        DraftOptionSelected?.Invoke(option.Id, state.Revision);
                });
                select.SelectButton.name = GameplayUiElementIds.DraftSelectButton(i);
                _draftOptions.Add(select);
            }
            for (var i = 0; i < state.Options.Count; i++)
                if (state.Options[i].IsEnabled) { InspectDraft(i); break; }
        }

        private void InspectDraft(int index)
        {
            if (index < 0 || index >= _draftState.Options.Count ||
                !_draftState.Options[index].IsEnabled) return;
            for (var i = 0; i < _draftOptions.childCount; i++)
                ((DraftCard)_draftOptions[i]).SetInspected(i == index);
            var recipes = _draftState.Options[index].Recipes;
            _recipeList.Clear();
            _recipeInspector.style.visibility = recipes.Count > 0 ? Visibility.Visible : Visibility.Hidden;
            _recipeTitle.text = _recipeEffect.text = "";
            _draftDetails.Clear();
            for (var i = 0; i < recipes.Count; i++)
            {
                var recipe = recipes[i];
                var button = new Button { name = GameplayUiElementIds.DraftRecipeButton(i) };
                button.AddToClassList("recipe-list-item");
                var icon = new Image { sprite = recipe.Icon, name = GameplayUiElementIds.CardIcon, pickingMode = PickingMode.Ignore };
                icon.AddToClassList("recipe-list-icon");
                button.Add(icon);
                var title = new Label(recipe.Title) { name = GameplayUiElementIds.CardTitle, pickingMode = PickingMode.Ignore };
                title.AddToClassList("recipe-list-name");
                button.Add(title);
                var progress = new Label(recipe.Progress) { name = GameplayUiElementIds.CardStatus, pickingMode = PickingMode.Ignore };
                progress.AddToClassList("recipe-list-progress");
                button.Add(progress);
                button.clicked += () => InspectRecipe(recipe, button);
                _recipeList.Add(button);
            }
            if (recipes.Count > 0) InspectRecipe(recipes[0], (Button)_recipeList[0]);
        }

        private void InspectRecipe(RecipeProjectionViewState recipe, Button selected)
        {
            foreach (var child in _recipeList.Children()) child.EnableInClassList("recipe-selected", child == selected);
            _recipeTitle.text = recipe.Title + " · " + recipe.Status;
            _recipeEffect.text = recipe.Effect;
            _draftDetails.Clear();
            if (recipe.ComponentStates.Count > 0)
            {
                foreach (var component in recipe.ComponentStates)
                {
                    var label = new Label(component.Text + (component.IsLevelMet ? " · Уровень набран" : ""))
                        { pickingMode = PickingMode.Ignore };
                    label.AddToClassList("draft-component");
                    label.EnableInClassList("draft-component-met", component.IsLevelMet);
                    _draftDetails.Add(label);
                }
            }
            else
            {
                foreach (var text in recipe.Components)
                {
                    var label = new Label(text) { pickingMode = PickingMode.Ignore };
                    label.AddToClassList("draft-component");
                    _draftDetails.Add(label);
                }
            }
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            _root.EnableInClassList("ui-compact", evt.newRect.width < 1600);
        }

        public bool ConsumePauseShortcut(bool space) => _pause.ConsumePauseShortcut(space);

        public void RenderBuild(BuildViewState state)
        {
            if (_renderedBuild.HasValue && SameBuild(_renderedBuild.Value, state)) return;
            _renderedBuild = state;
            RenderPauseBuild(state);
            RenderSlots(_activeSlots, state.ActiveSlots, true);
            RenderSlots(_passiveSlots, state.PassiveSlots, false);
            _sets.Clear();
            for (var i = 0; i < state.Sets.Count; i++)
            {
                var label = new Label(state.Sets[i].Icon != null ? "" : "S")
                {
                    name = GameplayUiElementIds.SetEntry(i),
                    pickingMode = PickingMode.Ignore,
                    focusable = false
                };
                label.AddToClassList("build-slot");
                label.AddToClassList("build-slot-set");
                if (state.Sets[i].Icon != null)
                    label.style.backgroundImage = new StyleBackground(state.Sets[i].Icon);
                _sets.Add(label);
            }

            _setRecipeProgress.Clear();
            var missedHeaderShown = false;
            for (var i = 0; i < state.SetRecipeProgress.Count; i++)
            {
                var recipe = state.SetRecipeProgress[i];
                // DECISION-0073: the presenter orders missed sets last; one header separates them.
                if (recipe.IsMissed && !missedHeaderShown)
                {
                    missedHeaderShown = true;
                    var header = new Label("MISSED SETS") { name = GameplayUiElementIds.MissedSetsHeader };
                    header.AddToClassList("set-recipe-missed-header");
                    _setRecipeProgress.Add(header);
                }
                var status = recipe.IsAcquired ? "acquired" : recipe.IsMissed ? "missed" : recipe.IsEligible ? "eligible" : "locked";
                var label = new Label($"{recipe.Title}: {recipe.FulfilledComponents}/{recipe.RequiredComponents} ({status})")
                {
                    name = GameplayUiElementIds.SetRecipeEntry(i)
                };
                label.AddToClassList("set-recipe-progress");
                if (recipe.IsMissed) label.AddToClassList("set-recipe-missed");
                _setRecipeProgress.Add(label);
            }
        }

        public void RenderCharacterSelection(CharacterSelectionViewState state)
        {
            if (_renderedCharacters != null && Same(_renderedCharacters.Characters, state.Characters)) return;
            _renderedCharacters = state;
            _characterSelection.Clear();
            for (var i = 0; i < state.Characters.Count; i++)
            {
                var character = state.Characters[i];
                if (character.IsSelected) _characterName = character.Title;
                var recoveryPercent = MathF.Round(character.DisappearingXpRecovery * 100f);
                var label = new ContentCard(new ContentCardViewState(
                    character.Title,
                    $"Start: {character.StartingSkillId}\n" +
                    $"HP {character.MaxHealth:0.#} · Move {character.MovementSpeed:0.##} · " +
                    $"Damage x{character.ActiveSkillDamageMultiplier:0.##} · Action speed x{1f / character.ActiveSkillCooldownMultiplier:0.##} · " +
                    $"XP recovery {recoveryPercent:0}%", isSelected: character.IsSelected))
                {
                    name = GameplayUiElementIds.CharacterEntry(i)
                };
                label.AddToClassList("character-entry");
                _characterSelection.Add(label);
            }
        }

        private static void RenderSlots(
            VisualElement container,
            System.Collections.Generic.IReadOnlyList<BuildSlotViewState> slots,
            bool active)
        {
            container.Clear();
            for (var i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                var occupiedText = slot.Icon != null
                    ? $"{slot.Level}"
                    : $"{slot.Title.Substring(0, Math.Min(2, slot.Title.Length))}\n{slot.Level}";
                var label = new Label(slot.IsOccupied ? occupiedText : "—")
                {
                    name = active ? GameplayUiElementIds.ActiveSlot(i) : GameplayUiElementIds.PassiveSlot(i),
                    pickingMode = PickingMode.Ignore,
                    focusable = false
                };
                label.AddToClassList("build-slot");
                if (slot.Icon != null) label.style.backgroundImage = new StyleBackground(slot.Icon);
                if (!slot.IsOccupied)
                    label.AddToClassList("build-slot-empty");
                container.Add(label);
            }
        }

        private static bool Same<T>(System.Collections.Generic.IReadOnlyList<T> left,
            System.Collections.Generic.IReadOnlyList<T> right)
        {
            if (left.Count != right.Count) return false;
            var comparer = System.Collections.Generic.EqualityComparer<T>.Default;
            for (var i = 0; i < left.Count; i++)
                if (!comparer.Equals(left[i], right[i])) return false;
            return true;
        }

        private static bool SameBuild(BuildViewState a, BuildViewState b) =>
            Same(a.ActiveSlots, b.ActiveSlots) && Same(a.PassiveSlots, b.PassiveSlots) &&
            Same(a.Sets, b.Sets) && Same(a.SetRecipeProgress, b.SetRecipeProgress);

        private void RenderPauseBuild(BuildViewState state)
        {
            _pause.Render(state);
        }

        public void RenderRunOverlay(RunOverlayViewState state)
        {
            SetVisible(_runOverlay, state.IsVisible);
            _runOverlayTitle.text = state.Title;
            SetVisible(_runOverlayResumeButton, state.CanResume);
            SetVisible(_pauseBuild, state.CanResume);
            SetVisible(_pauseCharacter, state.CanResume);
            SetVisible(_root.Q(className: "pause-columns"), state.CanResume);
            if (!state.IsVisible || !state.CanResume) _pause.Close(false);
            if (!state.IsVisible && _root.panel?.focusController.focusedElement is VisualElement focus && _runOverlay.Contains(focus)) focus.Blur();
        }

        public void RenderSkillObservability(SkillObservabilityViewState state) => _skillObservation.text = state.Summary;

        public void RenderEnemyObservability(EnemyObservabilityViewState state)
        {
            _enemyObservation.text = state.Summary;
        }

        public void RenderWaveObservability(WaveObservabilityViewState state)
        {
            _waveObservation.text = state.Summary;
        }

        public void SetDevelopmentControlsVisible(bool isVisible)
        {
            _developmentControlsAvailable = isVisible;
            _developmentPanelExpanded = false;
            UpdateDevelopmentVisibility();
        }

        private void HandleDevelopmentToggleClicked()
        {
            _developmentPanelExpanded = !_developmentPanelExpanded;
            UpdateDevelopmentVisibility();
        }

        private void HandleDevelopmentCloseClicked()
        {
            _developmentPanelExpanded = false;
            UpdateDevelopmentVisibility();
        }

        private void ShowDevelopmentRunTab() => ShowDevelopmentTab(
            _developmentRunPane,
            _developmentRunTab);

        private void ShowDevelopmentBuildTab() => ShowDevelopmentTab(
            _developmentBuildPane,
            _developmentBuildTab);

        private void ShowDevelopmentPresentationTab() => ShowDevelopmentTab(
            _developmentPresentationPane,
            _developmentPresentationTab);

        private void ShowDevelopmentPlaytestTab() => ShowDevelopmentTab(_developmentPlaytestPane, _developmentPlaytestTab);

        private void ShowDevelopmentTravelersTab() => ShowDevelopmentTab(_developmentTravelersPane, _developmentTravelersTab);

        private void ShowDevelopmentTab(VisualElement activePane, Button activeTab)
        {
            SetVisible(_developmentRunPane, activePane == _developmentRunPane);
            SetVisible(_developmentBuildPane, activePane == _developmentBuildPane);
            SetVisible(_developmentPresentationPane, activePane == _developmentPresentationPane);
            SetVisible(_developmentPlaytestPane, activePane == _developmentPlaytestPane);
            SetVisible(_developmentTravelersPane, activePane == _developmentTravelersPane);
            _developmentTravelersTab.EnableInClassList("development-tab-active", activeTab == _developmentTravelersTab);
            _developmentPlaytestTab.EnableInClassList("development-tab-active", activeTab == _developmentPlaytestTab);
            _developmentRunTab.EnableInClassList("development-tab-active", activeTab == _developmentRunTab);
            _developmentBuildTab.EnableInClassList("development-tab-active", activeTab == _developmentBuildTab);
            _developmentPresentationTab.EnableInClassList(
                "development-tab-active",
                activeTab == _developmentPresentationTab);
        }

        private void UpdateDevelopmentVisibility()
        {
            SetVisible(_developmentToggleButton, _developmentControlsAvailable);
            SetVisible(_developmentPanel, _developmentControlsAvailable && _developmentPanelExpanded);
            _developmentToggleButton.text = _developmentPanelExpanded ? "DEV ×" : "DEV";
        }

        private void HandlePauseClicked() { _pause.MarkShortcutConsumed(); PauseRequested?.Invoke(); }
        private void HandleHalfSpeedClicked() => SpeedRequested?.Invoke(.5f);
        private void HandleNormalSpeedClicked() => SpeedRequested?.Invoke(1);
        private void HandleDoubleSpeedClicked() => SpeedRequested?.Invoke(2);
        private void HandleTripleSpeedClicked() => SpeedRequested?.Invoke(3);
        private void HandleQuintupleSpeedClicked() => SpeedRequested?.Invoke(5);

        private static void RenderSpeedButton(Button button, float multiplier, HudViewState state)
        {
            button.EnableInClassList("speed-button--selected", state.SpeedMultiplier == multiplier);
            button.SetEnabled(state.CanChangeSpeed);
        }
        private void HandleBanishModeClicked() => DraftBanishModeRequested?.Invoke(_renderedDraftRevision);

        private void HandleRerollClicked() => DraftRerollRequested?.Invoke(_renderedDraftRevision);
        private void HandleAddBookClicked() => AddBookRequested?.Invoke();
        private void HandleAddExperienceClicked() => AddExperienceRequested?.Invoke();
        private void HandleAddLargeExperienceClicked() => AddLargeExperienceRequested?.Invoke();
        private void HandleAddRerollsClicked() => AddRerollsRequested?.Invoke();
        private void HandleUnlockAllDraftEntriesClicked() => UnlockAllDraftEntriesRequested?.Invoke();
        private void HandleDamageClicked() => ApplyDamageRequested?.Invoke();
        private void HandleHealingClicked() => ApplyHealingRequested?.Invoke();
        private void HandleHealthLockClicked() => ToggleHealthLockRequested?.Invoke();
        private void HandlePresentationLiveClicked() =>
            PresentationMotionPreviewRequested?.Invoke(SpritePresentationPreviewMotion.Live);
        private void HandlePresentationIdleClicked() =>
            PresentationMotionPreviewRequested?.Invoke(SpritePresentationPreviewMotion.Idle);
        private void HandlePresentationLeftClicked() =>
            PresentationMotionPreviewRequested?.Invoke(SpritePresentationPreviewMotion.Left);
        private void HandlePresentationRightClicked() =>
            PresentationMotionPreviewRequested?.Invoke(SpritePresentationPreviewMotion.Right);
        private void HandlePresentationResetClicked() => PresentationResetRequested?.Invoke();

        private static T Require<T>(VisualElement root, string name) where T : VisualElement
        {
            return root.Q<T>(name) ?? throw new InvalidOperationException($"Gameplay UI element '{name}' is missing.");
        }

        private static void SetVisible(VisualElement element, bool isVisible)
        {
            element.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void Dispose()
        {
            _root.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            _pause.Dispose();
            _speedHalfButton.clicked -= HandleHalfSpeedClicked;
            _speedNormalButton.clicked -= HandleNormalSpeedClicked;
            _speedDoubleButton.clicked -= HandleDoubleSpeedClicked;
            _speedTripleButton.clicked -= HandleTripleSpeedClicked;
            _speedQuintupleButton.clicked -= HandleQuintupleSpeedClicked;
            _runOverlayResumeButton.clicked -= HandlePauseClicked;
            _rerollButton.clicked -= HandleRerollClicked;
            _banishModeButton.clicked -= HandleBanishModeClicked;
            _addExperienceButton.clicked -= HandleAddExperienceClicked;
            _addLargeExperienceButton.clicked -= HandleAddLargeExperienceClicked;
            _addRerollsButton.clicked -= HandleAddRerollsClicked;
            _unlockAllDraftEntriesButton.clicked -= HandleUnlockAllDraftEntriesClicked;
            _addBookButton.clicked -= HandleAddBookClicked;
            _damageButton.clicked -= HandleDamageClicked;
            _healButton.clicked -= HandleHealingClicked;
            _healthLockButton.clicked -= HandleHealthLockClicked;
            _developmentToggleButton.clicked -= HandleDevelopmentToggleClicked;
            _developmentCloseButton.clicked -= HandleDevelopmentCloseClicked;
            _developmentRunTab.clicked -= ShowDevelopmentRunTab;
            _developmentBuildTab.clicked -= ShowDevelopmentBuildTab;
            _developmentPresentationTab.clicked -= ShowDevelopmentPresentationTab;
            _developmentPlaytestTab.clicked -= ShowDevelopmentPlaytestTab;
            _developmentTravelersTab.clicked -= ShowDevelopmentTravelersTab;
            _presentationLiveButton.clicked -= HandlePresentationLiveClicked;
            _presentationIdleButton.clicked -= HandlePresentationIdleClicked;
            _presentationLeftButton.clicked -= HandlePresentationLeftClicked;
            _presentationRightButton.clicked -= HandlePresentationRightClicked;
            _presentationResetButton.clicked -= HandlePresentationResetClicked;
            _draftOptions.Clear();
        }
    }
}
