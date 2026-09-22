using System;
using System.Threading.Tasks;
using Game.Presentation;
namespace Game.Settings
{
    public interface ISettingsService : IScreenShakePreference
    {
        event Action Changed;
        SettingsSnapshot Current { get; }
        IVideoDevice Video { get; }
        VideoMode Candidate { get; }
        VideoPreviewState PreviewState { get; }
        double SecondsRemaining { get; }
        bool Loaded { get; }
        bool Dirty { get; }
        string Message { get; }
        Task LoadAsync();
        void SetAudio(float master,float music,float sfx);
        void SetShake(bool enabled);
        void SetCandidate(VideoMode mode);
        Task ApplyVideoAsync();
        Task KeepVideoAsync();
        Task RevertVideoAsync();
        Task CloseAsync();
        Task SaveAsync();
        void Tick(double realtime);
    }
}
