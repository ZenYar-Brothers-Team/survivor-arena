using System.Collections.Generic;
using Game.Settings;
namespace Game.UI
{
    public sealed class AppShellViewState
    {
        public bool Menu { get; }
        public bool CharacterBack { get; }
        public bool PauseActions { get; }
        public bool Settings { get; }
        public bool CanPlay { get; }
        public bool Busy { get; }
        public bool Confirming { get; }
        public SettingsSnapshot Values { get; }
        public VideoMode Candidate { get; }
        public VideoMode Desktop { get; }
        public VideoMode SafeWindow { get; }
        public string Notification { get; }
        public IReadOnlyList<VideoMode> Modes { get; }
        public string Message { get; }
        public string Bindings { get; }
        public string VideoStatus { get; }
        public bool DevelopmentUnlock { get; }
        /// <summary>First reset click arms the destructive action; the second performs it.</summary>
        public bool DevelopmentResetArmed { get; }
        public bool SettingsFromPause { get; }
        public bool CanApplyVideo { get; }
        public bool CanRetrySettingsSave { get; }
        public AppShellViewState(bool menu, bool characterBack, bool pauseActions, bool settings, bool canPlay,
            bool busy, bool confirming, SettingsSnapshot values, VideoMode candidate, VideoMode desktop,
            IReadOnlyList<VideoMode> modes, string message, string bindings, string videoStatus, VideoMode safeWindow, string notification, bool developmentUnlock = false, bool developmentResetArmed = false,
            bool settingsFromPause = false, bool canApplyVideo = false, bool canRetrySettingsSave = false)
        {
            SafeWindow=safeWindow;Notification=notification;DevelopmentUnlock=developmentUnlock;DevelopmentResetArmed=developmentResetArmed;
            Menu=menu;CharacterBack=characterBack;PauseActions=pauseActions;Settings=settings;CanPlay=canPlay;
            Busy=busy;Confirming=confirming;Values=values;Candidate=candidate;Desktop=desktop;
            Modes=new List<VideoMode>(modes).AsReadOnly();Message=message;Bindings=bindings;VideoStatus=videoStatus;
            SettingsFromPause=settingsFromPause;CanApplyVideo=canApplyVideo;CanRetrySettingsSave=canRetrySettingsSave;
        }
    }
}
