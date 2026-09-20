using System;
using Game.Content;
using Game.Enemy;
using Game.Presentation;
using UnityEngine.UIElements;

namespace Game.UI
{
    public sealed class UiToolkitGameplayView : IGameplayUiView, IDisposable
    {
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
        private readonly VisualElement _developmentRunPane;
        private readonly VisualElement _developmentBuildPane;
        private readonly VisualElement _developmentPresentationPane;
        private readonly Button _addExperienceButton;
        private readonly Button _damageButton;
        private readonly Button _healButton;
        private readonly Label _enemyObservation;
        private readonly Label _waveObservation;
        private readonly Button _presentationLiveButton;
        private readonly Button _presentationIdleButton;
        private readonly Button _presentationLeftButton;
        private readonly Button _presentationRightButton;
        private readonly Button _presentationResetButton;
        private readonly VisualElement _characterSelection;
        private bool _developmentControlsAvailable;
        private bool _developmentPanelExpanded;

        public event Action<ContentId> DraftOptionSelected;
        public event Action DraftRerollRequested;
        public event Action<ContentId> DraftBanishRequested;
        public event Action PauseRequested;
        public event Action AddExperienceRequested;
        public event Action ApplyDamageRequested;
        public event Action ApplyHealingRequested;
        public event Action<SpritePresentationPreviewMotion> PresentationMotionPreviewRequested;
        public event Action PresentationResetRequested;

        public UiToolkitGameplayView(VisualElement root)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

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
            _rerollButton = Require<Button>(root, GameplayUiElementIds.DraftRerollButton);
            _banishCount = Require<Label>(root, GameplayUiElementIds.DraftBanishCount);
            _runOverlay = Require<VisualElement>(root, GameplayUiElementIds.RunOverlay);
            _runOverlayTitle = Require<Label>(root, GameplayUiElementIds.RunOverlayTitle);
            _runOverlayResumeButton = Require<Button>(root, GameplayUiElementIds.RunOverlayResumeButton);
            _developmentToggleButton = Require<Button>(root, GameplayUiElementIds.DevelopmentToggleButton);
            _developmentPanel = Require<VisualElement>(root, GameplayUiElementIds.DevelopmentPanel);
            _developmentCloseButton = Require<Button>(root, GameplayUiElementIds.DevelopmentCloseButton);
            _developmentRunTab = Require<Button>(root, GameplayUiElementIds.DevelopmentRunTab);
            _developmentBuildTab = Require<Button>(root, GameplayUiElementIds.DevelopmentBuildTab);
            _developmentPresentationTab = Require<Button>(root, GameplayUiElementIds.DevelopmentPresentationTab);
            _developmentRunPane = Require<VisualElement>(root, GameplayUiElementIds.DevelopmentRunPane);
            _developmentBuildPane = Require<VisualElement>(root, GameplayUiElementIds.DevelopmentBuildPane);
            _developmentPresentationPane = Require<VisualElement>(root, GameplayUiElementIds.DevelopmentPresentationPane);
            _addExperienceButton = Require<Button>(root, GameplayUiElementIds.AddExperienceButton);
            _damageButton = Require<Button>(root, GameplayUiElementIds.DamageButton);
            _healButton = Require<Button>(root, GameplayUiElementIds.HealButton);
            _enemyObservation = Require<Label>(root, GameplayUiElementIds.EnemyObservation);
            _waveObservation = Require<Label>(root, GameplayUiElementIds.WaveObservation);
            _presentationLiveButton = Require<Button>(root, GameplayUiElementIds.PresentationLiveButton);
            _presentationIdleButton = Require<Button>(root, GameplayUiElementIds.PresentationIdleButton);
            _presentationLeftButton = Require<Button>(root, GameplayUiElementIds.PresentationLeftButton);
            _presentationRightButton = Require<Button>(root, GameplayUiElementIds.PresentationRightButton);
            _presentationResetButton = Require<Button>(root, GameplayUiElementIds.PresentationResetButton);
            _characterSelection = Require<VisualElement>(root, GameplayUiElementIds.CharacterSelection);

            _pauseButton.clicked += HandlePauseClicked;
            _runOverlayResumeButton.clicked += HandlePauseClicked;
            _rerollButton.clicked += HandleRerollClicked;
            _addExperienceButton.clicked += HandleAddExperienceClicked;
            _damageButton.clicked += HandleDamageClicked;
            _healButton.clicked += HandleHealingClicked;
            _developmentToggleButton.clicked += HandleDevelopmentToggleClicked;
            _developmentCloseButton.clicked += HandleDevelopmentCloseClicked;
            _developmentRunTab.clicked += ShowDevelopmentRunTab;
            _developmentBuildTab.clicked += ShowDevelopmentBuildTab;
            _developmentPresentationTab.clicked += ShowDevelopmentPresentationTab;
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
            var health01 = state.MaxHealth > 0f ? state.CurrentHealth / state.MaxHealth : 0f;
            _healthBar.value = health01 * 100f;
            _healthBar.title = $"HP {MathF.Ceiling(state.CurrentHealth)}/{MathF.Ceiling(state.MaxHealth)}";
            _experienceBar.value = state.ExperienceProgress01 * 100f;
            _experienceBar.title = $"XP {MathF.Round(state.ExperienceProgress01 * 100f)}%";
            _levelLabel.text = $"LV {state.Level}";
            var remaining = Math.Max(0, (int)Math.Ceiling(state.RemainingSeconds));
            _timerLabel.text = $"{remaining / 60:00}:{remaining % 60:00}";
            RenderWave(state.Wave);
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
            if (!state.IsVisible)
            {
                _draftOptions.Clear();
                return;
            }

            _rerollButton.text = $"Reroll ({state.RemainingRerolls})";
            _rerollButton.SetEnabled(state.RemainingRerolls > 0);
            _banishCount.text = $"Banish: {state.RemainingBanishes}";
            _draftOptions.Clear();
            for (var i = 0; i < state.Options.Count; i++)
            {
                var option = state.Options[i];
                var row = new VisualElement();
                row.AddToClassList("draft-option-row");

                var select = new Button(() => DraftOptionSelected?.Invoke(option.Id))
                {
                    name = GameplayUiElementIds.DraftSelectButton(i),
                    text = $"{option.Title}\n{option.Detail}"
                };
                select.AddToClassList("draft-option-select");

                var banish = new Button(() => DraftBanishRequested?.Invoke(option.Id))
                {
                    name = GameplayUiElementIds.DraftBanishButton(i),
                    text = "Banish"
                };
                banish.AddToClassList("draft-option-banish");
                banish.SetEnabled(state.RemainingBanishes > 0);

                row.Add(select);
                row.Add(banish);
                _draftOptions.Add(row);
            }
        }

        public void RenderBuild(BuildViewState state)
        {
            RenderSlots(_activeSlots, state.ActiveSlots, true);
            RenderSlots(_passiveSlots, state.PassiveSlots, false);
            _sets.Clear();
            for (var i = 0; i < state.Sets.Count; i++)
            {
                var label = new Label(state.Sets[i].Title)
                {
                    name = GameplayUiElementIds.SetEntry(i)
                };
                label.AddToClassList("build-slot");
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
            _characterSelection.Clear();
            for (var i = 0; i < state.Characters.Count; i++)
            {
                var character = state.Characters[i];
                var selected = character.IsSelected ? " [SELECTED]" : string.Empty;
                var recoveryPercent = MathF.Round(character.DisappearingXpRecovery * 100f);
                var label = new Label(
                    $"{character.Title}{selected}\n" +
                    $"Start: {character.StartingSkillId}\n" +
                    $"HP {character.MaxHealth:0.#} · Move {character.MovementSpeed:0.##} · " +
                    $"Damage x{character.ActiveSkillDamageMultiplier:0.##} · Cooldown x{character.ActiveSkillCooldownMultiplier:0.##} · " +
                    $"XP recovery {recoveryPercent:0}%")
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
                var label = new Label(slot.IsOccupied ? $"{slot.Title}  Lv.{slot.Level}" : "—")
                {
                    name = active ? GameplayUiElementIds.ActiveSlot(i) : GameplayUiElementIds.PassiveSlot(i)
                };
                label.AddToClassList("build-slot");
                if (!slot.IsOccupied)
                    label.AddToClassList("build-slot-empty");
                container.Add(label);
            }
        }

        public void RenderRunOverlay(RunOverlayViewState state)
        {
            SetVisible(_runOverlay, state.IsVisible);
            _runOverlayTitle.text = state.Title;
            SetVisible(_runOverlayResumeButton, state.CanResume);
        }

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

        private void ShowDevelopmentTab(VisualElement activePane, Button activeTab)
        {
            SetVisible(_developmentRunPane, activePane == _developmentRunPane);
            SetVisible(_developmentBuildPane, activePane == _developmentBuildPane);
            SetVisible(_developmentPresentationPane, activePane == _developmentPresentationPane);
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
        private void HandleRerollClicked() => DraftRerollRequested?.Invoke();
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
            _addExperienceButton.clicked -= HandleAddExperienceClicked;
            _damageButton.clicked -= HandleDamageClicked;
            _healButton.clicked -= HandleHealingClicked;
            _developmentToggleButton.clicked -= HandleDevelopmentToggleClicked;
            _developmentCloseButton.clicked -= HandleDevelopmentCloseClicked;
            _developmentRunTab.clicked -= ShowDevelopmentRunTab;
            _developmentBuildTab.clicked -= ShowDevelopmentBuildTab;
            _developmentPresentationTab.clicked -= ShowDevelopmentPresentationTab;
            _presentationLiveButton.clicked -= HandlePresentationLiveClicked;
            _presentationIdleButton.clicked -= HandlePresentationIdleClicked;
            _presentationLeftButton.clicked -= HandlePresentationLeftClicked;
            _presentationRightButton.clicked -= HandlePresentationRightClicked;
            _presentationResetButton.clicked -= HandlePresentationResetClicked;
            _draftOptions.Clear();
        }
    }
}
