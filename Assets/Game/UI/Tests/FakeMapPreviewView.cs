using System;

namespace Game.UI.Tests
{
    internal sealed class FakeMapPreviewView : IMapPreviewView
    {
        public event Action ToggleRequested;
        public MapPreviewViewState State;
        public int Renders;
        public void Render(MapPreviewViewState state) { State = state; Renders++; }
        public void Toggle() => ToggleRequested?.Invoke();
    }
}
