using System;
using System.Globalization;

namespace Game.UI
{
    /// <summary>
    /// Development-only panel of a field's screen events (DECISION-0157): one button per event (it starts after the control's launch delay)
    /// plus live state. Hidden on fields without screen events and in non-development builds; no player-facing surface.
    /// </summary>
    public sealed class ScreenEventDevelopmentPresenter : IDisposable
    {
        private readonly IScreenEventDevelopmentControl _model;
        private readonly IScreenEventDevelopmentView _view;
        private readonly bool _development;
        private bool _entriesShown;

        public ScreenEventDevelopmentPresenter(IScreenEventDevelopmentControl model, IScreenEventDevelopmentView view, bool development)
        {
            _model = model;
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _development = development;
            _view.StartRequested += Start;
            Refresh();
        }

        public void Refresh()
        {
            if (!_development || _model == null)
            {
                _view.Render("", false, false);
                return;
            }
            if (!_entriesShown && _model.Entries.Count > 0)
            {
                _view.SetEntries(_model.Entries);
                _entriesShown = true;
            }
            _view.Render(Summary(), !_model.Busy && _model.Entries.Count > 0, true);
        }

        private string Summary()
        {
            var ci = CultureInfo.InvariantCulture;
            string state;
            if (_model.QueuedName != null) state = string.Format(ci, "Запуск через {0:0.0} с: {1}", _model.QueuedInSeconds, _model.QueuedName);
            else if (_model.ActiveName != null) state = string.Format(ci, "Событие идёт: {0} · ещё {1:0.0} с", _model.ActiveName, _model.ActiveRemainingSeconds);
            else state = string.Format(ci, "События нет · до следующего по расписанию {0:0} с", _model.SecondsUntilNext);
            return string.Format(ci, "{0}\nИнтенсивность волны: {1:0.00}\nПопаданий по игроку: {2}\nНечестных стартов: {3}",
                state, _model.Intensity, _model.PlayerHitCount, _model.UnfairStartCount);
        }

        private void Start(string eventId)
        {
            if (!_development || _model == null) return;
            _model.TryQueue(eventId);
            Refresh();
        }

        public void Dispose() => _view.StartRequested -= Start;
    }
}
