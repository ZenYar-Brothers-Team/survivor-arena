using System;
using System.Collections.Generic;
using Game.Content;
using Game.Progression;
using Game.Run;

namespace Game.UI
{
    public readonly struct HudViewState
    {
        public float CurrentHealth { get; }
        public float MaxHealth { get; }
        public float ExperienceProgress01 { get; }
        public int Level { get; }
        public float RemainingSeconds { get; }

        public HudViewState(float currentHealth, float maxHealth, float experienceProgress01, int level, float remainingSeconds)
        {
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            ExperienceProgress01 = experienceProgress01;
            Level = level;
            RemainingSeconds = remainingSeconds;
        }
    }

    public readonly struct DraftOptionViewState
    {
        public ContentId Id { get; }
        public string Title { get; }
        public string Detail { get; }

        public DraftOptionViewState(ContentId id, string title, string detail)
        {
            Id = id;
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Detail = detail ?? throw new ArgumentNullException(nameof(detail));
        }
    }

    public readonly struct DraftViewState
    {
        public bool IsVisible { get; }
        public int RemainingRerolls { get; }
        public int RemainingBanishes { get; }
        public IReadOnlyList<DraftOptionViewState> Options { get; }

        public DraftViewState(
            bool isVisible,
            int remainingRerolls,
            int remainingBanishes,
            IReadOnlyList<DraftOptionViewState> options)
        {
            IsVisible = isVisible;
            RemainingRerolls = remainingRerolls;
            RemainingBanishes = remainingBanishes;
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }
    }

    public readonly struct RunOverlayViewState
    {
        public bool IsVisible { get; }
        public string Title { get; }
        public bool CanResume { get; }

        public RunOverlayViewState(bool isVisible, string title, bool canResume)
        {
            IsVisible = isVisible;
            Title = title ?? string.Empty;
            CanResume = canResume;
        }
    }

    public readonly struct BuildSlotViewState
    {
        public string Title { get; }
        public int Level { get; }
        public bool IsOccupied { get; }

        public BuildSlotViewState(string title, int level, bool isOccupied)
        {
            Title = title ?? string.Empty;
            Level = level;
            IsOccupied = isOccupied;
        }
    }

    public readonly struct BuildViewState
    {
        public IReadOnlyList<BuildSlotViewState> ActiveSlots { get; }
        public IReadOnlyList<BuildSlotViewState> PassiveSlots { get; }

        public BuildViewState(
            IReadOnlyList<BuildSlotViewState> activeSlots,
            IReadOnlyList<BuildSlotViewState> passiveSlots)
        {
            ActiveSlots = activeSlots ?? throw new ArgumentNullException(nameof(activeSlots));
            PassiveSlots = passiveSlots ?? throw new ArgumentNullException(nameof(passiveSlots));
        }
    }

    public interface IGameplayUiModel
    {
        event Action Changed;

        float CurrentHealth { get; }
        float MaxHealth { get; }
        float ExperienceProgress01 { get; }
        int Level { get; }
        float RemainingSeconds { get; }
        RunState RunState { get; }
        bool IsDraftOpen { get; }
        int RemainingRerolls { get; }
        int RemainingBanishes { get; }
        IReadOnlyList<DraftOption> DraftOptions { get; }
        IReadOnlyList<BuildEntry> BuildEntries { get; }
        bool DevelopmentCommandsEnabled { get; }

        bool SelectDraftOption(ContentId id);
        bool RerollDraft();
        bool BanishDraftOption(ContentId id);
        void TogglePause();
        void AddFixtureExperience();
        void ApplyFixtureDamage();
        void ApplyFixtureHealing();
    }

    public interface IGameplayUiView
    {
        event Action<ContentId> DraftOptionSelected;
        event Action DraftRerollRequested;
        event Action<ContentId> DraftBanishRequested;
        event Action PauseRequested;
        event Action AddExperienceRequested;
        event Action ApplyDamageRequested;
        event Action ApplyHealingRequested;

        void RenderHud(HudViewState state);
        void RenderDraft(DraftViewState state);
        void RenderRunOverlay(RunOverlayViewState state);
        void RenderBuild(BuildViewState state);
        void SetDevelopmentControlsVisible(bool isVisible);
    }
}
