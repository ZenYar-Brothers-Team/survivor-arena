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
        public AppShellViewState(bool menu, bool characterBack, bool pauseActions, bool settings, bool canPlay,
            bool busy, bool confirming, SettingsSnapshot values, VideoMode candidate, VideoMode desktop,
            IReadOnlyList<VideoMode> modes, string message, string bindings, string videoStatus, VideoMode safeWindow, string notification)
        {
            SafeWindow=safeWindow;Notification=notification;
            Menu=menu;CharacterBack=characterBack;PauseActions=pauseActions;Settings=settings;CanPlay=canPlay;
            Busy=busy;Confirming=confirming;Values=values;Candidate=candidate;Desktop=desktop;
            Modes=new List<VideoMode>(modes).AsReadOnly();Message=message;Bindings=bindings;VideoStatus=videoStatus;
        }
    }
}
