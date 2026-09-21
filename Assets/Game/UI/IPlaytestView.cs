using System;

namespace Game.UI
{
    public interface IPlaytestView
    {
        event Action<string> MarkerRequested;
        event Action ExportRequested;
        void Render(PlaytestViewState state);
    }
}
