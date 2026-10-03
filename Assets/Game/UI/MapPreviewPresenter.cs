using System;

namespace Game.UI
{
    /// <summary>Development-only map overlay: toggles a scaled-down view of the arena, its roads, obstacles and the camera frame.</summary>
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
                _view.Render(new MapPreviewViewState(true, false, default, null, null, default, "Карта скрыта"));
                return;
            }
            var arena = _source.Arena;
            var obstacles = _source.Obstacles;
            var roads = _source.Roads;
            var altars = _source.Altars;
            var traps = _source.Traps;
            var summary = $"Арена {arena.width:0}×{arena.height:0}, препятствий: {obstacles.Count}";
            if (roads.Count > 0) summary += $", участков дорог: {roads.Count}";
            if (altars.Count > 0) summary += $", алтарей: {altars.Count}";
            if (traps != null && traps.Count > 0)
            {
                var barrels = 0;
                foreach (var trap in traps) if (trap.Barrel) barrels++;
                summary += $", ловушек: {traps.Count - barrels}, бочек: {barrels}";
            }
            _view.Render(new MapPreviewViewState(true, true, arena, obstacles, roads, _source.View, summary, altars, traps));
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
