using System;
using Game.Run;
using UnityEngine;
namespace Game.Settings
{
    /// <summary>Owned endpoints; every source receives master exactly once.</summary>
    public sealed class SettingsAudioRuntime : IAudioPreview, IDisposable
    {
        private readonly GameObject _owner;
        private readonly ISettingsService _settings;
        private readonly AudioSource _musicPreview, _sfxPreview, _music, _sfx;
        private readonly AudioClip _musicClip, _sfxClip;
        private readonly SettingsConfig _config;
        private RunModel _run;
        private float _musicGain = 1, _sfxGain = 1;
        public SettingsAudioRuntime(Transform parent, ISettingsService settings, SettingsConfig config)
        {
            _settings = settings; _config = config;
            _owner = new GameObject("Audio routing"); _owner.transform.SetParent(parent, false);
            _musicPreview = Source(); _sfxPreview = Source(); _music = Source(); _sfx = Source();
            _musicClip = Tone(config.MusicPreviewHz); _sfxClip = Tone(config.SfxPreviewHz);
            _musicPreview.clip = _musicClip; _sfxPreview.clip = _sfxClip;
            settings.Changed += Refresh; Refresh();
        }
        private AudioSource Source() { var s = _owner.AddComponent<AudioSource>(); s.playOnAwake = false; s.spatialBlend = 0; s.ignoreListenerPause = true; return s; }
        private AudioClip Tone(float hz)
        {
            var samples = new float[(int)(_config.PreviewSeconds * _config.PreviewSampleRate)];
            for (var i = 0; i < samples.Length; i++) samples[i] = Mathf.Sin(2 * Mathf.PI * hz * i / _config.PreviewSampleRate) * Mathf.Sin(Mathf.PI * i / samples.Length);
            var clip = AudioClip.Create("Settings preview", samples.Length, 1, _config.PreviewSampleRate, false); clip.SetData(samples, 0); return clip;
        }
        public void Bind(RunModel run) { if (_run != null) _run.StateChanged -= State; _sfx.Stop(); _run = run; if (run != null) { run.StateChanged += State; State(run.State); } }
        private void State(RunState state) { if (state == RunState.Paused) _sfx.Pause(); else if (state == RunState.Running) _sfx.UnPause(); else _sfx.Stop(); }
        public void PlayMusic(AudioClip clip, float sourceGain = 1) { _musicGain = sourceGain; Refresh(); _music.clip = clip; _music.Play(); }
        public void PlayGameplaySfx(AudioClip clip, float sourceGain = 1) { if (_run?.State != RunState.Running) return; _sfxGain = sourceGain; Refresh(); _sfx.clip = clip; _sfx.Play(); }
        public void Preview(bool music) { var source = music ? _musicPreview : _sfxPreview; source.Stop(); source.Play(); }
        public void StopPreviews() { _musicPreview.Stop(); _sfxPreview.Stop(); }
        private void Refresh()
        {
            _music.volume = _settings.Current.Gain(true, _musicGain); _sfx.volume = _settings.Current.Gain(false, _sfxGain);
            _musicPreview.volume = _settings.Current.Gain(true, _config.PreviewGain); _sfxPreview.volume = _settings.Current.Gain(false, _config.PreviewGain);
        }
        public void Dispose() { Bind(null); _settings.Changed -= Refresh; UnityEngine.Object.Destroy(_owner); UnityEngine.Object.Destroy(_musicClip); UnityEngine.Object.Destroy(_sfxClip); }
    }
}
