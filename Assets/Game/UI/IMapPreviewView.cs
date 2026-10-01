using System;

namespace Game.UI
{
    public interface IMapPreviewView
    {
        event Action ToggleRequested;
        void Render(MapPreviewViewState state);
    }
}
