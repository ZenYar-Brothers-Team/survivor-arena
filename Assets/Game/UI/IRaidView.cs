using System;
using System.Collections.Generic;
using Game.Enemy;

namespace Game.UI
{
    public interface IRaidView
    {
        event Action<RaidTemplateKind> StartRequested;
        /// <summary>One launch button per template; handed over once.</summary>
        void SetTemplates(IReadOnlyList<RaidTemplateKind> templates);
        void Render(string summary, bool launchEnabled, bool development);
    }
}
