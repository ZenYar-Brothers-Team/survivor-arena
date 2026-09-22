using System;

namespace Game.UI.Tests
{
    internal sealed class FakePlaytestView : IPlaytestView
    {
        public event Action<string> MarkerRequested;
        public event Action ExportRequested;
        public PlaytestViewState State;
        public int Renders;
        public void Render(PlaytestViewState state) { State = state; Renders++; }
        public void Mark(string text) => MarkerRequested?.Invoke(text);
        public void Export() => ExportRequested?.Invoke();
    }
}
