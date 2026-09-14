using System;
using Game.Content;
using UnityEngine.UIElements;

namespace Game.UI
{
    public sealed class UiToolkitGameplayView : IGameplayUiView, IDisposable
    {
        private readonly ProgressBar _healthBar;
        private readonly ProgressBar _experienceBar;
        private readonly Label _levelLabel;
        private readonly Label _timerLabel;
        private readonly Button _pauseButton;
        private readonly VisualElement _activeSlots;
        private readonly VisualElement _passiveSlots;
        private readonly VisualElement _draftOverlay;
        private readonly VisualElement _draftOptions;
        private readonly Button _rerollButton;
        private readonly Label _banishCount;
        private readonly VisualElement _runOverlay;
        private readonly Label _runOverlayTitle;
        private readonly Button _runOverlayResumeButton;
        private readonly VisualElement _developmentPanel;
        private readonly Button _addExperienceButton;
        private readonly Button _damageButton;
        private readonly Button _healButton;

        public event Action<ContentId> DraftOptionSelected;
        public event Action DraftRerollRequested;
        public event Action<ContentId> DraftBanishRequested;
        public event Action PauseRequested;
        public event Action AddExperienceRequested;
        public event Action ApplyDamageRequested;
        public event Action ApplyHealingRequested;

        public UiToolkitGameplayView(VisualElement root)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            _healthBar = Require<ProgressBar>(root, GameplayUiElementIds.HealthBar);
            _experienceBar = Require<ProgressBar>(root, GameplayUiElementIds.ExperienceBar);
            _levelLabel = Require<Label>(root, GameplayUiElementIds.LevelLabel);
            _timerLabel = Require<Label>(root, GameplayUiElementIds.TimerLabel);
            _pauseButton = Require<Button>(root, GameplayUiElementIds.PauseButton);
            _activeSlots = Require<VisualElement>(root, GameplayUiElementIds.ActiveSlots);
            _passiveSlots = Require<VisualElement>(root, GameplayUiElementIds.PassiveSlots);
            _draftOverlay = Require<VisualElement>(root, GameplayUiElementIds.DraftOverlay);
            _draftOptions = Require<VisualElement>(root, GameplayUiElementIds.DraftOptions);
            _rerollButton = Require<Button>(root, GameplayUiElementIds.DraftRerollButton);
            _banishCount = Require<Label>(root, GameplayUiElementIds.DraftBanishCount);
            _runOverlay = Require<VisualElement>(root, GameplayUiElementIds.RunOverlay);
            _runOverlayTitle = Require<Label>(root, GameplayUiElementIds.RunOverlayTitle);
            _runOverlayResumeButton = Require<Button>(root, GameplayUiElementIds.RunOverlayResumeButton);
            _developmentPanel = Require<VisualElement>(root, GameplayUiElementIds.DevelopmentPanel);
            _addExperienceButton = Require<Button>(root, GameplayUiElementIds.AddExperienceButton);
            _damageButton = Require<Button>(root, GameplayUiElementIds.DamageButton);
            _healButton = Require<Button>(root, GameplayUiElementIds.HealButton);

            _pauseButton.clicked += HandlePauseClicked;
            _runOverlayResumeButton.clicked += HandlePauseClicked;
            _rerollButton.clicked += HandleRerollClicked;
            _addExperienceButton.clicked += HandleAddExperienceClicked;
            _damageButton.clicked += HandleDamageClicked;
            _healButton.clicked += HandleHealingClicked;
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

        public void SetDevelopmentControlsVisible(bool isVisible)
        {
            SetVisible(_developmentPanel, isVisible);
        }

        private void HandlePauseClicked() => PauseRequested?.Invoke();
        private void HandleRerollClicked() => DraftRerollRequested?.Invoke();
        private void HandleAddExperienceClicked() => AddExperienceRequested?.Invoke();
        private void HandleDamageClicked() => ApplyDamageRequested?.Invoke();
        private void HandleHealingClicked() => ApplyHealingRequested?.Invoke();

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
            _draftOptions.Clear();
        }
    }
}
