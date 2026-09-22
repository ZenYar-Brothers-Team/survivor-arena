using System;
using System.Threading.Tasks;
namespace Game.Settings
{
    /// <summary>Main-thread settings policy. Async IO is serialized; pending video is never saved.</summary>
    public sealed class SettingsService : ISettingsService
    {
        private readonly SettingsConfig _config;
        private readonly ISettingsStore _store;
        private Task _saveTask;
        private Task _videoTask, _loadTask;
        private long _revision, _savedRevision;
        private bool _preserveBeforeWrite;
        private double _now, _deadline;
        public event Action Changed;
        public SettingsSnapshot Current { get; private set; }
        public IVideoDevice Video { get; }
        public VideoMode Candidate { get; private set; }
        public VideoPreviewState PreviewState { get; private set; }
        public double SecondsRemaining => Math.Max(0,_deadline-_now);
        public bool Loaded { get; private set; }
        public bool Dirty => _revision!=_savedRevision;
        public string Message { get; private set; }
        public bool ScreenShakeEnabled => Current.Shake;
        public SettingsService(SettingsConfig config,ISettingsStore store,IVideoDevice video)
        {
            _config=config??throw new ArgumentNullException(nameof(config));_store=store??throw new ArgumentNullException(nameof(store));Video=video??throw new ArgumentNullException(nameof(video));
            Current=config.Defaults(video.Desktop);Candidate=Current.Video;
        }
        private void Notify() => Changed?.Invoke();
        private void Update(SettingsSnapshot snapshot) { Current=snapshot;_revision++;Notify(); }
        public Task LoadAsync() => _loadTask ?? (_loadTask = LoadCoreAsync());
        private async Task LoadCoreAsync()
        {
            if(Loaded)return;
            string message=null;
            try
            {
                var text=await _store.ReadAsync();
                if(text!=null)
                {
                    try { Current=SettingsCodec.Decode(text); }
                    catch { _preserveBeforeWrite=true;message="Settings reset to defaults; the original file will be preserved."; }
                }
                else _revision++;
            }
            catch(Exception error) { _preserveBeforeWrite=true;message="Settings could not be read: "+error.Message; }
            if(_preserveBeforeWrite)_revision++;
            try
            {
                if(!Video.Supports(Current.Video)||!await Video.TryApplyAsync(Current.Video))
                { await ApplyFallbackAsync();message="Unavailable video mode; using a safe window."; }
            }
            catch(Exception error) { message="Video settings unavailable: "+error.Message;Current=Current.WithVideo(Video.Current);_revision++; }
            Candidate=Current.Video;Loaded=true;Message=message;Notify();
            if(Dirty) { await SaveAsync(); if(Message==null) { Message=message;Notify(); } }
        }
        private async Task ApplyFallbackAsync()
        {
            if(!await Video.TryApplyAsync(Video.SafeWindow)) throw new InvalidOperationException("Safe window could not be applied.");
            Current=Current.WithVideo(Video.Current);_revision++;
        }
        public void SetAudio(float master,float music,float sfx) => Update(new SettingsSnapshot(master,music,sfx,Current.Shake,Current.Video));
        public void SetShake(bool enabled) => Update(new SettingsSnapshot(Current.Master,Current.Music,Current.Sfx,enabled,Current.Video));
        public void SetCandidate(VideoMode mode)
        { if(PreviewState!=VideoPreviewState.Idle)return;Candidate=mode??throw new ArgumentNullException(nameof(mode));Notify(); }
        public Task ApplyVideoAsync()
        {
            if (_videoTask != null && !_videoTask.IsCompleted) return _videoTask;
            return _videoTask = ApplyVideoCoreAsync();
        }
        private async Task ApplyVideoCoreAsync()
        {
            if(!Loaded||PreviewState!=VideoPreviewState.Idle)return;
            if(!Video.Supports(Candidate)) { Message="Unsupported video mode.";Notify();return; }
            PreviewState=VideoPreviewState.Applying;Video.SetPreviewBackground(true);Notify();
            try
            {
                if(!await Video.TryApplyAsync(Candidate)) { await RevertVideoCoreAsync();Message="Video mode could not be applied.";Notify();return; }
                _deadline=_now+_config.ConfirmSeconds;PreviewState=VideoPreviewState.Confirming;Message=null;Notify();
            }
            catch(Exception error) { await RevertVideoCoreAsync();Message="Video mode failed: "+error.Message;Notify(); }
        }
        public async Task KeepVideoAsync()
        {
            if(PreviewState!=VideoPreviewState.Confirming)return;
            if(SecondsRemaining<=0) { await RevertVideoAsync();return; }
            PreviewState=VideoPreviewState.Idle;Video.SetPreviewBackground(false);
            Candidate=Video.Current;Update(Current.WithVideo(Candidate));await SaveAsync();
        }
        public Task RevertVideoAsync()
        {
            if (_videoTask != null && !_videoTask.IsCompleted) return _videoTask;
            return _videoTask = RevertVideoCoreAsync();
        }
        private async Task RevertVideoCoreAsync()
        {
            if(PreviewState==VideoPreviewState.Idle||PreviewState==VideoPreviewState.Reverting)return;
            PreviewState=VideoPreviewState.Reverting;Notify();
            try { if(!Video.Supports(Current.Video)||!await Video.TryApplyAsync(Current.Video))await ApplyFallbackAsync(); }
            catch(Exception error) { Current=Current.WithVideo(Video.Current);_revision++;Message="Video rollback failed: "+error.Message; }
            finally { PreviewState=VideoPreviewState.Idle;Candidate=Current.Video;Video.SetPreviewBackground(false);Notify(); }
        }
        public async Task CloseAsync()
        {
            if (_videoTask != null) await _videoTask;
            await RevertVideoAsync();Candidate=Current.Video;await SaveAsync();Notify();
        }
        public Task SaveAsync()
        {
            if(!Loaded)return Task.CompletedTask;
            if(_saveTask!=null&&!_saveTask.IsCompleted)return _saveTask;
            return _saveTask=SaveCoreAsync();
        }
        private async Task SaveCoreAsync()
        {
            try
            {
                if(_preserveBeforeWrite) { await _store.PreserveAsync();_preserveBeforeWrite=false; }
                while(Dirty)
                {
                    var revision=_revision;var text=SettingsCodec.Encode(Current);
                    await _store.WriteAsync(text);_savedRevision=revision;
                }
                Message=null;Notify();
            }
            catch(Exception error) { Message="Settings not saved: "+error.Message;Notify(); }
        }
        public void Tick(double realtime)
        {
            if(double.IsNaN(realtime)||double.IsInfinity(realtime)||realtime<_now)throw new ArgumentOutOfRangeException(nameof(realtime));
            var previous=(int)SecondsRemaining;_now=realtime;
            if(PreviewState!=VideoPreviewState.Confirming)return;
            if(SecondsRemaining<=0) { _ = RevertVideoAsync();return; }
            if((int)SecondsRemaining!=previous)Notify();
        }
    }
}
