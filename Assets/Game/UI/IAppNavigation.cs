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
        string Notification { get; }
        string MovementBindings { get; }
        void Play(); void MainMenu(); void Meta(); void QuitRun(); void Exit();
    }
}
