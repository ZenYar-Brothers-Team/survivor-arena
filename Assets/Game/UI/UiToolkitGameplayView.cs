using System;
using Game.Content;
using Game.Enemy;
using Game.Presentation;
using UnityEngine.UIElements;

namespace Game.UI
{
    public sealed class UiToolkitGameplayView : IGameplayUiView, IDisposable
    {
        private readonly Label _draftDetails;
        private readonly Label _pauseCharacter;
        private readonly VisualElement _pauseBuild;
        private readonly UiNotification _notification;
        private string _characterName = "";
        private string _characterStats = "";
        private float _previousElapsed;
        private int _previousLevel;
        private BuildViewState? _renderedBuild;
        private CharacterSelectionViewState _renderedCharacters;
        private readonly ProgressBar _healthBar;
        private readonly ProgressBar _experienceBar;
        private readonly Label _levelLabel;
        private readonly Label _timerLabel;
        private readonly Label _waveLabel;
        private readonly Button _pauseButton;
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
        private readonly VisualElement _developmentRunPane;
        private readonly VisualElement _developmentBuildPane;
        private readonly VisualElement _developmentPresentationPane;
        private readonly Button _addExperienceButton;
        private readonly Button _damageButton;
        private readonly Button _healButton;
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
        public event Action AddExperienceRequested;
        public event Action AddBookRequested;
        public event Action ApplyDamageRequested;
        public event Action ApplyHealingRequested;
        public event Action<SpritePresentationPreviewMotion> PresentationMotionPreviewRequested;
        public event Action PresentationResetRequested;

        public UiToolkitGameplayView(VisualElement root)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            _draftDetails = Require<Label>(root, GameplayUiElementIds.DraftDetails);
            _pauseCharacter = Require<Label>(root, GameplayUiElementIds.PauseCharacter);
            _pauseBuild = Require<VisualElement>(root, GameplayUiElementIds.PauseBuild);
            _notification = new UiNotification(Require<Label>(root, GameplayUiElementIds.Notification));
            _healthBar = Require<ProgressBar>(root, GameplayUiElementIds.HealthBar);
            _experienceBar = Require<ProgressBar>(root, GameplayUiElementIds.ExperienceBar);
            _levelLabel = Require<Label>(root, GameplayUiElementIds.LevelLabel);
            _timerLabel = Require<Label>(root, GameplayUiElementIds.TimerLabel);
            _waveLabel = Require<Label>(root, GameplayUiElementIds.WaveLabel);
            _pauseButton = Require<Button>(root, GameplayUiElementIds.PauseButton);
            _activeSlots = Require<VisualElement>(root, GameplayUiElementIds.ActiveSlots);
            _passiveSlots = Require<VisualElement>(root, GameplayUiElementIds.PassiveSlots);
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
            _developmentRunPane = Require<VisualElement>(root, GameplayUiElementIds.DevelopmentRunPane);
            _developmentBuildPane = Require<VisualElement>(root, GameplayUiElementIds.DevelopmentBuildPane);
            _developmentPresentationPane = Require<VisualElement>(root, GameplayUiElementIds.DevelopmentPresentationPane);
            _addExperienceButton = Require<Button>(root, GameplayUiElementIds.AddExperienceButton);
            _damageButton = Require<Button>(root, GameplayUiElementIds.DamageButton);
            _healButton = Require<Button>(root, GameplayUiElementIds.HealButton);
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

            _pauseButton.clicked += HandlePauseClicked;
            _runOverlayResumeButton.clicked += HandlePauseClicked;
            _rerollButton.clicked += HandleRerollClicked;
            _banishModeButton.clicked += HandleBanishModeClicked;
            _addExperienceButton.clicked += HandleAddExperienceClicked;
            _addBookButton.clicked += HandleAddBookClicked;
            _damageButton.clicked += HandleDamageClicked;
            _healButton.clicked += HandleHealingClicked;
            _developmentToggleButton.clicked += HandleDevelopmentToggleClicked;
            _developmentCloseButton.clicked += HandleDevelopmentCloseClicked;
            _developmentRunTab.clicked += ShowDevelopmentRunTab;
            _developmentBuildTab.clicked += ShowDevelopmentBuildTab;
            _developmentPresentationTab.clicked += ShowDevelopmentPresentationTab;
            _developmentPlaytestTab.clicked += ShowDevelopmentPlaytestTab;
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
            _notification.Tick(Math.Max(0f, state.ElapsedSeconds - _previousElapsed));
            _previousElapsed = state.ElapsedSeconds;
            if (_previousLevel > 0 && state.Level > _previousLevel) _notification.Show("LEVEL UP");
            _previousLevel = state.Level;
            _bookCurrency.text = $"Book currency: +{state.BookCurrency}";
            SetVisible(_bookCurrency, state.BookCurrency > 0);
            var health01 = state.MaxHealth > 0f ? state.CurrentHealth / state.MaxHealth : 0f;
            _healthBar.value = health01 * 100f;
            _healthBar.title = $"HP {MathF.Ceiling(state.CurrentHealth)}/{MathF.Ceiling(state.MaxHealth)}";
            _experienceBar.value = state.ExperienceProgress01 * 100f;
            _experienceBar.title = $"XP {MathF.Round(state.ExperienceProgress01 * 100f)}%";
            _levelLabel.text = $"LV {state.Level}";
            var elapsed = Math.Max(0, (int)Math.Floor(state.ElapsedSeconds));
            _timerLabel.text = $"{elapsed / 60:00}:{elapsed % 60:00}";
            RenderWave(state.Wave);
            if (state.Stats != null)
                _characterStats = $"Action speed +{state.Stats.ActionSpeedBonus:P0} · Pickup radius {state.Stats.PickupRadius:0.##}";
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
            _draftHeading.text = state.Heading;
            _draftQueue.text = state.QueueDetail;
            _draftOverlay.EnableInClassList("draft-book", state.Heading == "TRAVELER BOOK");
            if (!state.IsVisible)
            {
                _draftOptions.Clear();
                _renderedDraftRevision = Guid.Empty;
                return;
            }

            _rerollButton.text = $"Reroll ({state.RemainingRerolls})";
            _rerollButton.SetEnabled(state.CanReroll);
            _banishModeButton.text = state.IsBanishMode ? "Cancel banish" : "Banish";
            _banishModeButton.SetEnabled(state.CanBanish);
            _draftControlHint.text = state.ControlHint;
            _draftOverlay.EnableInClassList("draft-banish-mode", state.IsBanishMode);
            _banishCount.text = $"Banish: {state.RemainingBanishes}";
            if (state.Revision != Guid.Empty && _renderedDraftRevision == state.Revision && _renderedBanishMode == state.IsBanishMode) return;
            _renderedDraftRevision = state.Revision;
            _renderedBanishMode = state.IsBanishMode;
            _draftOptions.Clear();
            _draftDetails.text = "Hover or focus a card for details. Select a card to continue.";
            for (var i = 0; i < 3; i++)
            {
                var option = i < state.Options.Count ? state.Options[i] :
                    new DraftOptionViewState(default, "No available option", "", false);
                var select = new DraftCard(option, () => DraftOptionSelected?.Invoke(option.Id, state.Revision))
                {
                    name = GameplayUiElementIds.DraftSelectButton(i)
                };
                select.RegisterCallback<MouseEnterEvent>(_ => _draftDetails.text = select.Details);
                select.RegisterCallback<FocusInEvent>(_ => _draftDetails.text = select.Details);
                _draftOptions.Add(select);
            }
        }

        public void RenderBuild(BuildViewState state)
        {
            if (_renderedBuild.HasValue && SameBuild(_renderedBuild.Value, state)) return;
            var previousSetCount = _renderedBuild?.Sets.Count ?? state.Sets.Count;
            _renderedBuild = state;
            if (state.Sets.Count > previousSetCount) _notification.Show("SET ACQUIRED");
            RenderPauseBuild(state);
            RenderSlots(_activeSlots, state.ActiveSlots, true);
            RenderSlots(_passiveSlots, state.PassiveSlots, false);
            _sets.Clear();
            for (var i = 0; i < state.Sets.Count; i++)
            {
                var label = new Label("S")
                {
                    name = GameplayUiElementIds.SetEntry(i),
                    tooltip = state.Sets[i].Title,
                    focusable = false
                };
                label.AddToClassList("build-slot");
                if (state.Sets[i].Icon != null)
                    label.style.backgroundImage = new StyleBackground(state.Sets[i].Icon);
                _sets.Add(label);
            }

            _setRecipeProgress.Clear();
            for (var i = 0; i < state.SetRecipeProgress.Count; i++)
            {
                var recipe = state.SetRecipeProgress[i];
                var status = recipe.IsAcquired ? "acquired" : recipe.IsEligible ? "eligible" : "locked";
                var label = new Label($"{recipe.Title}: {recipe.FulfilledComponents}/{recipe.RequiredComponents} ({status})")
                {
                    name = GameplayUiElementIds.SetRecipeEntry(i)
                };
                label.AddToClassList("set-recipe-progress");
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
                var label = new Label(slot.IsOccupied ? $"{slot.Title.Substring(0, Math.Min(2, slot.Title.Length))}\n{slot.Level}" : "—")
                {
                    name = active ? GameplayUiElementIds.ActiveSlot(i) : GameplayUiElementIds.PassiveSlot(i),
                    tooltip = slot.IsOccupied ? $"{slot.Title} · Lv.{slot.Level}" : "Empty",
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
            _pauseBuild.Clear();
            _pauseBuild.Add(new Label("ACTIVE SKILLS"));
            AddBuildDetails(state.ActiveSlots);
            _pauseBuild.Add(new Label("PASSIVES"));
            AddBuildDetails(state.PassiveSlots);
            _pauseBuild.Add(new Label("ACQUIRED SETS"));
            foreach (var set in state.Sets) _pauseBuild.Add(new ContentCard(new ContentCardViewState(set.Title, set.Detail, icon: set.Icon)));
            _pauseBuild.Add(new Label("SET PROGRESS"));
            foreach (var recipe in state.SetRecipeProgress)
            {
                if (recipe.IsAcquired || !recipe.HasProgress) continue;
                _pauseBuild.Add(new ContentCard(new ContentCardViewState(recipe.Title,
                    $"{recipe.FulfilledComponents}/{recipe.RequiredComponents} · {(recipe.IsEligible ? "Recipe fulfilled · not acquired" : "In progress")}", recipe.Detail)));
            }
        }

        private void AddBuildDetails(System.Collections.Generic.IReadOnlyList<BuildSlotViewState> slots)
        {
            var grid = new VisualElement();
            grid.AddToClassList("pause-build-grid");
            _pauseBuild.Add(grid);
            foreach (var slot in slots)
                grid.Add(new ContentCard(new ContentCardViewState(slot.Title,
                    slot.IsOccupied ? $"Lv.{slot.Level}\n{slot.Detail}" : "Empty", icon: slot.Icon, isEnabled: slot.IsOccupied)));
        }

        public void RenderRunOverlay(RunOverlayViewState state)
        {
            SetVisible(_runOverlay, state.IsVisible);
            _runOverlayTitle.text = state.Title;
            SetVisible(_runOverlayResumeButton, state.CanResume);
            SetVisible(_pauseBuild, state.CanResume);
            SetVisible(_pauseCharacter, state.CanResume);
            if (state.CanResume)
                _pauseCharacter.text = _characterName + " · " + _healthBar.title + " · " + _levelLabel.text + "\n" + _characterStats;

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

        private void ShowDevelopmentTab(VisualElement activePane, Button activeTab)
        {
            SetVisible(_developmentRunPane, activePane == _developmentRunPane);
            SetVisible(_developmentBuildPane, activePane == _developmentBuildPane);
            SetVisible(_developmentPresentationPane, activePane == _developmentPresentationPane);
            SetVisible(_developmentPlaytestPane, activePane == _developmentPlaytestPane);
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

        private void HandlePauseClicked() => PauseRequested?.Invoke();
        private void HandleBanishModeClicked() => DraftBanishModeRequested?.Invoke(_renderedDraftRevision);

        private void HandleRerollClicked() => DraftRerollRequested?.Invoke(_renderedDraftRevision);
        private void HandleAddBookClicked() => AddBookRequested?.Invoke();
        private void HandleAddExperienceClicked() => AddExperienceRequested?.Invoke();
        private void HandleDamageClicked() => ApplyDamageRequested?.Invoke();
        private void HandleHealingClicked() => ApplyHealingRequested?.Invoke();
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
            _pauseButton.clicked -= HandlePauseClicked;
            _runOverlayResumeButton.clicked -= HandlePauseClicked;
            _rerollButton.clicked -= HandleRerollClicked;
            _banishModeButton.clicked -= HandleBanishModeClicked;
            _addExperienceButton.clicked -= HandleAddExperienceClicked;
            _addBookButton.clicked -= HandleAddBookClicked;
            _damageButton.clicked -= HandleDamageClicked;
            _healButton.clicked -= HandleHealingClicked;
            _developmentToggleButton.clicked -= HandleDevelopmentToggleClicked;
            _developmentCloseButton.clicked -= HandleDevelopmentCloseClicked;
            _developmentRunTab.clicked -= ShowDevelopmentRunTab;
            _developmentBuildTab.clicked -= ShowDevelopmentBuildTab;
            _developmentPresentationTab.clicked -= ShowDevelopmentPresentationTab;
            _developmentPlaytestTab.clicked -= ShowDevelopmentPlaytestTab;
            _presentationLiveButton.clicked -= HandlePresentationLiveClicked;
            _presentationIdleButton.clicked -= HandlePresentationIdleClicked;
            _presentationLeftButton.clicked -= HandlePresentationLeftClicked;
            _presentationRightButton.clicked -= HandlePresentationRightClicked;
            _presentationResetButton.clicked -= HandlePresentationResetClicked;
            _draftOptions.Clear();
        }
    }
}
