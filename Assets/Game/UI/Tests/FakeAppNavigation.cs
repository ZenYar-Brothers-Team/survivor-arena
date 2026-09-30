using System;
namespace Game.UI.Tests
{
    public sealed class FakeAppNavigation : IAppNavigation
    {
        public event Action NavigationChanged;
        public bool AtMainMenu {get;set;}=true;
        public bool AtCharacterSelection => false;
        public bool AtManualPause {get;set;}
        public bool CanPlay => true;
        public NotificationMessage Notification => default;
        public string MovementBindings => "WASD";
        public bool DevelopmentTools {get;set;}
        public int Plays,Quits,DevelopmentUnlocks,DevelopmentResets;
        public void UnlockAllForDevelopment(){DevelopmentUnlocks++;}
        public void ResetProgressionForDevelopment(){DevelopmentResets++;}
        public void Play(){Plays++;}public void MainMenu(){AtMainMenu=true;NavigationChanged?.Invoke();}
        public void Meta(){} public void QuitRun(){Quits++;}public void Exit(){}
    }
}
