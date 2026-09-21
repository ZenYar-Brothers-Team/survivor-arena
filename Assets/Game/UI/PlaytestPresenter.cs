using System;
using Game.Telemetry;

namespace Game.UI
{
    /// <summary>Routes optional diagnostic intents and gates them in release/no-recorder mode.</summary>
    public sealed class PlaytestPresenter : IDisposable
    {
        private readonly IPlaytestSession _session;
        private readonly IPlaytestView _view;
        private string _lastSummary;
        private bool? _lastEnabled;
        public PlaytestPresenter(IPlaytestSession session, IPlaytestView view)
        {
            _session = session ?? new DisabledPlaytestSession();
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _view.MarkerRequested += Mark; _view.ExportRequested += Export;
            Refresh();
        }
        private void Mark(string text) { if (_session.Enabled) _session.AddMarker(text); Refresh(); }
        private void Export() { if (_session.Enabled) _session.Export(); Refresh(); }
        public void Refresh()
        {
            var summary = _session.Summary;
            if (_lastSummary == summary && _lastEnabled == _session.Enabled) return;
            _lastSummary = summary; _lastEnabled = _session.Enabled;
            _view.Render(new PlaytestViewState(_session.Enabled, summary));
        }
        public void Dispose() { _view.MarkerRequested -= Mark; _view.ExportRequested -= Export; }
    }
}
