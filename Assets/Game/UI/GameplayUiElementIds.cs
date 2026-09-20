namespace Game.UI
{
    public static class GameplayUiElementIds
    {
        public const string Root = "gameplay-ui-root";
        public const string HealthBar = "hud-health-bar";
        public const string ExperienceBar = "hud-xp-bar";
        public const string LevelLabel = "hud-level";
        public const string TimerLabel = "hud-timer";
        public const string WaveLabel = "hud-wave";
        public const string PauseButton = "hud-pause";
        public const string BuildPanel = "build-panel";
        public const string ActiveSlots = "build-active-slots";
        public const string PassiveSlots = "build-passive-slots";
        public const string Sets = "build-sets";
        public const string SetRecipeProgress = "development-set-recipes";
        public const string DraftOverlay = "draft-overlay";
        public const string DraftOptions = "draft-options";
        public const string DraftRerollButton = "draft-reroll";
        public const string DraftBanishCount = "draft-banish-count";
        public const string RunOverlay = "run-overlay";
        public const string RunOverlayTitle = "run-overlay-title";
        public const string RunOverlayResumeButton = "run-overlay-resume";
        public const string DevelopmentToggleButton = "development-toggle";
        public const string DevelopmentPanel = "development-panel";
        public const string DevelopmentCloseButton = "development-close";
        public const string DevelopmentRunTab = "development-tab-run";
        public const string DevelopmentBuildTab = "development-tab-build";
        public const string DevelopmentPresentationTab = "development-tab-presentation";
        public const string DevelopmentRunPane = "development-pane-run";
        public const string DevelopmentBuildPane = "development-pane-build";
        public const string DevelopmentPresentationPane = "development-pane-presentation";
        public const string AddExperienceButton = "development-add-xp";
        public const string DamageButton = "development-damage";
        public const string HealButton = "development-heal";
        public const string EnemyObservation = "development-enemy-observation";
        public const string WaveObservation = "development-wave-observation";
        public const string StatsObservation = "development-stats-observation";
        public const string PresentationLiveButton = "development-presentation-live";
        public const string PresentationIdleButton = "development-presentation-idle";
        public const string PresentationLeftButton = "development-presentation-left";
        public const string PresentationRightButton = "development-presentation-right";
        public const string PresentationResetButton = "development-presentation-reset";
        public const string CharacterSelection = "development-character-selection";

        public static string DraftSelectButton(int index) => $"draft-option-{index}-select";
        public static string DraftBanishButton(int index) => $"draft-option-{index}-banish";
        public static string ActiveSlot(int index) => $"build-active-slot-{index}";
        public static string PassiveSlot(int index) => $"build-passive-slot-{index}";
        public static string SetEntry(int index) => $"build-set-{index}";
        public static string SetRecipeEntry(int index) => $"development-set-recipe-{index}";
        public static string CharacterEntry(int index) => $"development-character-{index}";
    }
}
