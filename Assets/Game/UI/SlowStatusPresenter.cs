using System;
using Game.Enemy;
using Game.Presentation;

namespace Game.UI
{
    /// <summary>Development-only slow-status look switcher (DECISION-0108); no player-facing surface.</summary>
    public sealed class SlowStatusPresenter : IDisposable
    {
        private readonly ISlowStatusPreview _model;
        private readonly ISlowStatusView _view;
        private readonly bool _development;
        private string _lastAction = "";

        public SlowStatusPresenter(ISlowStatusPreview model, ISlowStatusView view, bool development)
        {
            _model = model; _view = view ?? throw new ArgumentNullException(nameof(view)); _development = development;
            if (_model != null) _model.Changed += Refresh;
            _view.StyleRequested += Style; _view.SlowAllRequested += SlowAll;
            Refresh();
        }

        public void Refresh() =>
            _view.Render(_model?.Style ?? SlowStatusStyle.Off, _development && _model != null, _lastAction);

        private void Style(SlowStatusStyle style)
        {
            if (!_development || _model == null) return;
            _model.SetStyle(style);
            Refresh();
        }

        private void SlowAll()
        {
            if (!_development || _model == null) return;
            var count = _model.SlowAllForPreview();
            _lastAction = $"Замедлено врагов: {count}";
            Refresh();
        }

        public void Dispose()
        {
            if (_model != null) _model.Changed -= Refresh;
            _view.StyleRequested -= Style; _view.SlowAllRequested -= SlowAll;
        }
    }
}
