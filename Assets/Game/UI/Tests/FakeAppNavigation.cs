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
        public string Notification => "";
        public string MovementBindings => "WASD";
        public int Plays,Quits;
        public void Play(){Plays++;}public void MainMenu(){AtMainMenu=true;NavigationChanged?.Invoke();}
        public void Meta(){} public void QuitRun(){Quits++;}public void Exit(){}
    }
}
