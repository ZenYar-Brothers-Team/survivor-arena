using System;
using System.Globalization;
using Game.Enemy;

namespace Game.UI
{
    /// <summary>Development-only roundup panel (DECISION-0155): template launch buttons plus live counters; no player-facing surface.</summary>
    public sealed class RaidPresenter : IDisposable
    {
        private readonly IRaidDevelopmentControl _model;
        private readonly IRaidView _view;
        private readonly bool _development;
        private bool _templatesShown;

        public RaidPresenter(IRaidDevelopmentControl model, IRaidView view, bool development)
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
            // The profile is known only once the spawner is initialized, so templates are handed over on first sight.
            if (!_templatesShown && _model.Templates.Count > 0)
            {
                _view.SetTemplates(_model.Templates);
                _templatesShown = true;
            }
            _view.Render(Summary(), !_model.RaidActive && _model.Templates.Count > 0, true);
        }

        private string Summary()
        {
            var ci = CultureInfo.InvariantCulture;
            var state = _model.RaidActive
                ? string.Format(ci, "Облава: ДА · {0} · до выключения {1:0.0} с", _model.ActiveTemplate, _model.RaidRemainingSeconds)
                : "Облава: нет";
            var next = _model.Templates.Count == 0
                ? "Профиль облавы не загружен"
                : _model.RaidActive
                    ? "Следующая: отсчёт после окончания"
                    : string.Format(ci, "До следующей: {0:0} с · таймер {1}", _model.NextRaidSeconds,
                        _model.CountdownRunning ? "идёт" : "стоит (нет скопления)");
            return string.Format(ci, "{0}\n{1}\nВрагов на карте: {2}\nВрагов в радиусе экрана: {3} (порог: от {4})",
                state, next, _model.TotalEnemyCount, _model.NearbyEnemyCount, _model.TriggerEnemyCount);
        }

        private void Start(RaidTemplateKind kind)
        {
            if (!_development || _model == null) return;
            _model.TryStartRaid(kind);
            Refresh();
        }

        public void Dispose() => _view.StartRequested -= Start;
    }
}
