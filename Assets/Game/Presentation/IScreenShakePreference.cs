namespace Game.Presentation
{
    // IP-26 owns persistence and the camera service; presentation never moves the camera itself.
    public interface IScreenShakePreference
    {
        bool ScreenShakeEnabled { get; }
    }
}
