using System;
using System.Collections.Generic;
using System.Linq;
using Game.Meta;
using Game.Run;

namespace Game.Automation
{
    /// <summary>Route advances only on a real victory; no implicit skips or fallback fields.</summary>
    public static class AutomationRoutePolicy
    {
        public static int NextIndex(int currentIndex, RunCompletionReason outcome, ExperimentConfigData settings,
            IProfileService profile, IReadOnlyCollection<string> playableFields, out string stopReason)
        {
            if (settings == null || profile == null || playableFields == null)
                throw new ArgumentNullException("Route policy inputs required.");
            if (currentIndex < 0 || currentIndex >= settings.FieldRoute.Count)
                throw new ArgumentOutOfRangeException(nameof(currentIndex));
            stopReason = null;
            if (outcome != RunCompletionReason.Victory) return currentIndex;
            if (currentIndex == settings.FieldRoute.Count - 1)
            {
                if (settings.StopAfterRouteClear.Value) stopReason = "routeCleared";
                return currentIndex;
            }
            var next = settings.FieldRoute[currentIndex + 1];
            if (!profile.IsUnlocked(next)) { stopReason = "routeBlocked:locked:" + next; return currentIndex; }
            if (!playableFields.Contains(next)) { stopReason = "routeBlocked:unplayable:" + next; return currentIndex; }
            return currentIndex + 1;
        }
    }
}
