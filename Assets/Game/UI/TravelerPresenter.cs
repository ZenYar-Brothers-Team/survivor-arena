using System;
using System.Collections.Generic;
using Game.Traveler;
using UnityEngine;
namespace Game.UI
{
    public sealed class TravelerPresenter : IDisposable
    {
        private readonly ITravelerRuntime _model;
        private readonly ITravelerView _view;
        private readonly Func<Vector2, Vector3> _project;
        private readonly bool _development;
        public TravelerPresenter(ITravelerRuntime model, ITravelerView view, Func<Vector2, Vector3> project, bool development)
        { _model = model; _view = view; _project = project; _development = development; _view.SpawnRequested += Spawn; Refresh(); }
        public void Refresh()
        {
            var result = new List<TravelerHudItem>();
            if (_model != null)
                foreach (var item in _model.Snapshot)
                {
                    var viewport = _project(item.Position);
                    var offscreen = viewport.z <= 0 || viewport.x < 0 || viewport.x > 1 || viewport.y < 0 || viewport.y > 1;
                    var point = new Vector2(viewport.x, 1 - viewport.y);
                    var direction = point - Vector2.one * .5f;
                    if (viewport.z <= 0) direction = -direction;
                    if (offscreen)
                    {
                        // Insets reserve the top HUD and bottom skill slots. Separate inward lanes prevent simultaneous pointers overlapping.
                        var lane = result.FindAll(value => value.Offscreen).Count;
                        var halfWidth = .44f - lane * .07f;
                        var halfHeight = .30f - lane * .065f;
                        var factor = Mathf.Min(halfWidth / Mathf.Max(Mathf.Abs(direction.x), .0001f), halfHeight / Mathf.Max(Mathf.Abs(direction.y), .0001f));
                        point = Vector2.one * .5f + direction * factor;
                    }
                    else point.y = Mathf.Clamp01(point.y - .04f);
                    result.Add(new TravelerHudItem(item.LifeId, item.Marker, item.Health / item.MaxHealth,
                        point, offscreen, Arrow(direction)));
                }
            _view.Render(result.AsReadOnly(), _model?.DevelopmentObservation ?? "", _development && _model != null);
        }
        private static string Arrow(Vector2 direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y) * 2) return direction.x > 0 ? "→" : "←";
            if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x) * 2) return direction.y > 0 ? "↓" : "↑";
            return direction.x > 0 ? (direction.y > 0 ? "↘" : "↗") : (direction.y > 0 ? "↙" : "↖");
        }
        private void Spawn() { if (_development) _model?.SpawnDevelopmentTraveler(); }
        public void Dispose() => _view.SpawnRequested -= Spawn;
    }
}
