using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
namespace Game.Settings
{
    public sealed class UnityVideoDevice : IVideoDevice
    {
        private readonly SettingsConfig _config;
        private bool? _previousBackground;
        public UnityVideoDevice(SettingsConfig config) { _config = config; }
        public VideoMode Current => new VideoMode(Screen.width, Screen.height, Screen.fullScreenMode == FullScreenMode.FullScreenWindow);
        public VideoMode Desktop => new VideoMode(Display.main.systemWidth, Display.main.systemHeight, true);
        public VideoMode SafeWindow => new VideoMode(Math.Min(_config.SafeWidth, Desktop.Width), Math.Min(_config.SafeHeight, Desktop.Height), false);
        public IReadOnlyList<VideoMode> WindowModes => Screen.resolutions.Select(r => new VideoMode(r.width, r.height, false))
            .Concat(new[] { SafeWindow }).Distinct().OrderBy(m => m.Width).ThenBy(m => m.Height).ToArray();
        public bool Supports(VideoMode mode) => mode != null && (mode.Borderless ? mode.Equals(Desktop) : WindowModes.Contains(mode));
        public async Task<bool> TryApplyAsync(VideoMode mode)
        {
            if (!Supports(mode)) return false;
            Screen.SetResolution(mode.Width, mode.Height, mode.Borderless ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            var deadline = Time.realtimeSinceStartupAsDouble + _config.ApplyWaitSeconds;
            do { await Task.Yield(); } while (Time.realtimeSinceStartupAsDouble < deadline && !Current.Equals(mode));
            return Current.Equals(mode);
        }
        public void SetPreviewBackground(bool active)
        {
            if (active) { if (!_previousBackground.HasValue) _previousBackground = Application.runInBackground; Application.runInBackground = true; }
            else if (_previousBackground.HasValue) { Application.runInBackground = _previousBackground.Value; _previousBackground = null; }
        }
    }
}
