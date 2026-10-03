using System;
using System.Collections.Generic;

namespace Game.UI
{
    public interface IScreenEventDevelopmentView
    {
        event Action<string> StartRequested;
        /// <summary>One launch button per event; handed over once.</summary>
        void SetEntries(IReadOnlyList<ScreenEventDevelopmentEntry> entries);
        void Render(string summary, bool launchEnabled, bool visible);
    }
}
