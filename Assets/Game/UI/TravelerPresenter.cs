using System;
using System.Collections.Generic;
using Game.Traveler;
using UnityEngine;
namespace Game.UI
{
    public sealed class TravelerPresenter : IDisposable
    {
        // DECISION-0109: pointers ride the screen frame close to the edge; values are normalized screen fractions.
        private const float EdgeInsetX = .035f;
        private const float EdgeInsetY = .065f;
        private const float PointerSeparation = .09f;
        private const float HealthBarLift = .012f;
        private readonly ITravelerRuntime _model;
        private readonly ITravelerView _view;
        private readonly Func<Vector2, Vector3> _project;
        private readonly Func<float> _aspect;
        private readonly bool _development;
        private bool _choicesShown;
        public TravelerPresenter(ITravelerRuntime model, ITravelerView view, Func<Vector2, Vector3> project, bool development,
            Func<float> aspect = null)
        {
            _model = model; _view = view; _project = project; _development = development; _aspect = aspect ?? (() => 16f / 9f);
            _view.SpawnRequested += Spawn; _view.SpawnChosen += SpawnChosen; Refresh();
        }
        public void Refresh()
        {
            // The pool is known only once the encounter is initialized, so the list is handed over on first sight.
            if (_development && !_choicesShown && _model != null)
            {
                var choices = _model.DevelopmentChoices;
                if (choices.Count > 0) { _view.SetChoices(choices); _choicesShown = true; }
            }
            var result = new List<TravelerHudItem>();
            if (_model != null)
                foreach (var item in _model.Snapshot)
                {
                    var viewport = _project(item.Position);
                    var behind = viewport.z <= 0;
                    var offscreen = behind || viewport.x < 0 || viewport.x > 1 || viewport.y < 0 || viewport.y > 1;
                    var point = new Vector2(viewport.x, 1 - viewport.y);
                    var direction = point - Vector2.one * .5f;
                    if (behind) direction = -direction;
                    var health = item.MaxHealth > 0 ? item.Health / item.MaxHealth : 0f;
                    if (!offscreen)
                    {
                        // The HP bar sits just above the body top, like the player's and mid-boss bars.
                        var head = _project(item.HeadPosition);
                        var headPoint = new Vector2(head.x, 1 - head.y);
                        headPoint.y = Mathf.Clamp01(headPoint.y - HealthBarLift);
                        result.Add(new TravelerHudItem(item.LifeId, health, headPoint, false, 0f));
                        continue;
                    }
                    point = Separate(OnFrame(direction), result);
                    var aspect = Mathf.Max(.01f, _aspect());
                    var angle = Mathf.Atan2(direction.y, direction.x * aspect) * Mathf.Rad2Deg;
                    result.Add(new TravelerHudItem(item.LifeId, health, point, true, angle));
                }
            _view.Render(result.AsReadOnly(), _model?.DevelopmentObservation ?? "", _development && _model != null);
        }

        /// <summary>Where the ray from the screen centre toward the Traveler meets the inset screen frame.</summary>
        private static Vector2 OnFrame(Vector2 direction)
        {
            var halfWidth = .5f - EdgeInsetX;
            var halfHeight = .5f - EdgeInsetY;
            var factor = Mathf.Min(halfWidth / Mathf.Max(Mathf.Abs(direction.x), .0001f),
                halfHeight / Mathf.Max(Mathf.Abs(direction.y), .0001f));
            return Vector2.one * .5f + direction * factor;
        }

        /// <summary>Several Travelers in one direction get separate pointers slid along the frame.</summary>
        private static Vector2 Separate(Vector2 point, List<TravelerHudItem> placed)
        {
            var onVerticalEdge = Mathf.Abs(point.x - .5f) >= .5f - EdgeInsetX - .001f;
            for (var attempt = 0; attempt < 8; attempt++)
            {
                var overlaps = false;
                foreach (var other in placed)
                    if (other.Offscreen && Mathf.Abs(other.Position.x - point.x) < PointerSeparation &&
                        Mathf.Abs(other.Position.y - point.y) < PointerSeparation) { overlaps = true; break; }
                if (!overlaps) break;
                if (onVerticalEdge) point.y = point.y + PointerSeparation > 1f - EdgeInsetY ? point.y - 2 * PointerSeparation : point.y + PointerSeparation;
                else point.x = point.x + PointerSeparation > 1f - EdgeInsetX ? point.x - 2 * PointerSeparation : point.x + PointerSeparation;
            }
            point.x = Mathf.Clamp(point.x, EdgeInsetX, 1f - EdgeInsetX);
            point.y = Mathf.Clamp(point.y, EdgeInsetY, 1f - EdgeInsetY);
            return point;
        }

        private void Spawn() { if (_development) _model?.SpawnDevelopmentTraveler(); }
        private void SpawnChosen(string id) { if (_development) _model?.SpawnDevelopmentTraveler(id); }
        public void Dispose() { _view.SpawnRequested -= Spawn; _view.SpawnChosen -= SpawnChosen; }
    }
}
