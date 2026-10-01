using System;

namespace Game.UI
{
    /// <summary>Development-only map overlay: toggles a scaled-down view of the arena, its obstacles and the camera frame.</summary>
    public sealed class MapPreviewPresenter : IDisposable
    {
        private readonly IMapPreviewSource _source;
        private readonly IMapPreviewView _view;
        private readonly bool _development;
        private bool _visible;

        public MapPreviewPresenter(IMapPreviewSource source, IMapPreviewView view, bool development)
        {
            _source = source; _view = view ?? throw new ArgumentNullException(nameof(view)); _development = development;
            _view.ToggleRequested += Toggle;
            Refresh();
        }

        /// <summary>Re-renders the overlay; call at the HUD cadence so the camera frame follows the player.</summary>
        public void Refresh()
        {
            if (!_development || _source == null)
            {
                _view.Render(MapPreviewViewState.Hidden);
                return;
            }
            if (!_visible)
            {
                _view.Render(new MapPreviewViewState(true, false, default, null, default, "Карта скрыта"));
                return;
            }
            var arena = _source.Arena;
            var obstacles = _source.Obstacles;
            _view.Render(new MapPreviewViewState(true, true, arena, obstacles, _source.View,
                $"Арена {arena.width:0}×{arena.height:0}, препятствий: {obstacles.Count}"));
        }

        private void Toggle()
        {
            if (!_development || _source == null) return;
            _visible = !_visible;
            Refresh();
        }

        public void Dispose() => _view.ToggleRequested -= Toggle;
    }
}
