using System.Collections.Generic;

namespace Game.UI
{
    /// <summary>What the development panel needs from a field's screen events (DECISION-0157): the list, live state and a delayed manual start.</summary>
    public interface IScreenEventDevelopmentControl
    {
        IReadOnlyList<ScreenEventDevelopmentEntry> Entries { get; }
        /// <summary>Seconds between pressing a button and the event starting.</summary>
        float LaunchDelaySeconds { get; }
        /// <summary>An event is running or waiting for its delayed start.</summary>
        bool Busy { get; }
        /// <summary>Name of the running event; null when none.</summary>
        string ActiveName { get; }
        float ActiveRemainingSeconds { get; }
        /// <summary>Name of the queued event; null when none.</summary>
        string QueuedName { get; }
        float QueuedInSeconds { get; }
        float Intensity { get; }
        float SecondsUntilNext { get; }
        int PlayerHitCount { get; }
        int UnfairStartCount { get; }
        bool TryQueue(string eventId);
    }
}
