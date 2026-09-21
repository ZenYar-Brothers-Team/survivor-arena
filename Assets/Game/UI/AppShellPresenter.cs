using System;
using Game.Settings;
namespace Game.UI
{
    public sealed class AppShellPresenter : IDisposable
    {
        private readonly IAppNavigation _navigation;
        private readonly ISettingsService _settings;
        private readonly IAudioPreview _audio;
        private readonly IAppShellView _view;
        private bool _disposed;
        public bool SettingsOpen { get; private set; }
        public AppShellPresenter(IAppNavigation navigation, ISettingsService settings, IAudioPreview audio, IAppShellView view)
        {
            _navigation=navigation; _settings=settings; _audio=audio; _view=view;
            navigation.NavigationChanged+=Refresh; settings.Changed+=Refresh;
            view.Play+=Play; view.Meta+=Meta; view.Settings+=Open; view.Exit+=Exit; view.MainMenu+=Menu; view.Quit+=Quit;
            view.Back+=Back; view.Apply+=Apply; view.Keep+=Keep; view.Revert+=Revert; view.Save+=Save;
            view.Audio+=Audio; view.Shake+=Shake; view.Preview+=Preview; view.Video+=Video;
            Refresh();
        }
        private void Play() { if(!SettingsOpen&&_navigation.CanPlay)_navigation.Play(); }
        private void Meta() { if(!SettingsOpen&&_navigation.CanPlay)_navigation.Meta(); }
        private void Exit() => _navigation.Exit();
        private void Menu() { if(!SettingsOpen)_navigation.MainMenu(); }
        private void Quit() { if(!SettingsOpen)_navigation.QuitRun(); }
        private void Open() { if(_navigation.AtMainMenu||_navigation.AtManualPause) { SettingsOpen=true; Refresh(); } }
        public async void Back()
        {
            if(!SettingsOpen||_settings.PreviewState==VideoPreviewState.Applying||_settings.PreviewState==VideoPreviewState.Reverting)return;
            if(_settings.PreviewState==VideoPreviewState.Confirming) { await _settings.RevertVideoAsync(); return; }
            await _settings.CloseAsync(); if (_disposed) return; _audio.StopPreviews(); SettingsOpen=false; Refresh();
        }
        private async void Apply() => await _settings.ApplyVideoAsync();
        private async void Keep() => await _settings.KeepVideoAsync();
        private async void Revert() => await _settings.RevertVideoAsync();
        private async void Save() => await _settings.SaveAsync();
        private void Audio(float master,float music,float sfx) => _settings.SetAudio(master,music,sfx);
        private void Shake(bool enabled) => _settings.SetShake(enabled);
        private void Preview(bool music) => _audio.Preview(music);
        private void Video(VideoMode mode) => _settings.SetCandidate(mode);
        public void Refresh()
        {
            _view.Render(new AppShellViewState(_navigation.AtMainMenu&&!SettingsOpen, _navigation.AtCharacterSelection&&!SettingsOpen,
                _navigation.AtManualPause&&!SettingsOpen, SettingsOpen, _navigation.CanPlay,
                _settings.PreviewState==VideoPreviewState.Applying||_settings.PreviewState==VideoPreviewState.Reverting,
                _settings.PreviewState==VideoPreviewState.Confirming, _settings.Current, _settings.Candidate,
                _settings.Video.Desktop, _settings.Video.WindowModes, _settings.Message??"", _navigation.MovementBindings,
                _settings.Video.Current+(_settings.Video.Current.Borderless?" Borderless":" Windowed")+
                    (_settings.PreviewState==VideoPreviewState.Confirming?" — Revert in "+Math.Ceiling(_settings.SecondsRemaining)+"s":""), _settings.Video.SafeWindow, _navigation.Notification));
        }
        public void Dispose()
        {
            _audio.StopPreviews(); _navigation.NavigationChanged-=Refresh; _settings.Changed-=Refresh;
            _view.Play-=Play; _view.Meta-=Meta; _view.Settings-=Open; _view.Exit-=Exit; _view.MainMenu-=Menu; _view.Quit-=Quit;
            _view.Back-=Back; _view.Apply-=Apply; _view.Keep-=Keep; _view.Revert-=Revert; _view.Save-=Save;
            _view.Audio-=Audio; _view.Shake-=Shake; _view.Preview-=Preview; _view.Video-=Video;
        }
    }
}
