using System;
using System.Collections.Generic;
namespace Game.UI
{
    public interface ITravelerView
    {
        event Action SpawnRequested;
        /// <summary>Development panel: the Traveler with this content id was chosen to launch now.</summary>
        event Action<string> SpawnChosen;
        void SetChoices(IReadOnlyList<Game.Traveler.TravelerChoice> choices);
        void Render(IReadOnlyList<TravelerHudItem> travelers, string observation, bool development);
    }
}
