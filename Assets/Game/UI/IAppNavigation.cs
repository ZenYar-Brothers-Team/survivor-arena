using System;
namespace Game.UI
{
    public interface IAppNavigation
    {
        event Action NavigationChanged;
        bool AtMainMenu { get; }
        bool AtCharacterSelection { get; }
        bool AtManualPause { get; }
        bool CanPlay { get; }
        NotificationMessage Notification { get; }
        string MovementBindings { get; }
        /// <summary>Editor/Development build only (DECISION-0005).</summary>
        bool DevelopmentTools { get; }
        void UnlockAllForDevelopment();
        /// <summary>Adds 100 000 meta coins to the profile (Editor/Development build only).</summary>
        void GrantCurrencyForDevelopment();
        void ResetProgressionForDevelopment();
        void Play(); void MainMenu(); void Meta(); void QuitRun(); void Exit();
    }
}
