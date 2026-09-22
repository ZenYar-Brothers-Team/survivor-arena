using System;
using System.Collections.Generic;
namespace Game.UI
{
    public interface ITravelerView
    {
        event Action SpawnRequested;
        void Render(IReadOnlyList<TravelerHudItem> travelers, string observation, bool development);
    }
}
