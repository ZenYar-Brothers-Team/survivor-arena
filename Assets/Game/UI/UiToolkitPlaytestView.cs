using System;
using UnityEngine.UIElements;

namespace Game.UI
{
    /// <summary>Bounded Playtest drawer pane; sends intents only.</summary>
    public sealed class UiToolkitPlaytestView : IPlaytestView, IDisposable
    {
        private readonly Label _summary;
        private readonly TextField _note;
        private readonly Button _marker, _export;
        public event Action<string> MarkerRequested;
        public event Action ExportRequested;
        public UiToolkitPlaytestView(VisualElement root)
        {
            _summary = root.Q<Label>(GameplayUiElementIds.PlaytestSummary);
            _note = root.Q<TextField>(GameplayUiElementIds.PlaytestNote);
            _marker = root.Q<Button>(GameplayUiElementIds.PlaytestMarker);
            _export = root.Q<Button>(GameplayUiElementIds.PlaytestExport);
            _note.maxLength = 512;
            _marker.clicked += Mark; _export.clicked += Export;
        }
        private void Mark() { MarkerRequested?.Invoke(_note.value); }
        private void Export() { ExportRequested?.Invoke(); }
        public void Render(PlaytestViewState state)
        {
            if (_summary.text != state.Summary) _summary.text = state.Summary;
            _marker.SetEnabled(state.Enabled); _export.SetEnabled(state.Enabled); _note.SetEnabled(state.Enabled);
        }
        public void Dispose() { _marker.clicked -= Mark; _export.clicked -= Export; }
    }
}
