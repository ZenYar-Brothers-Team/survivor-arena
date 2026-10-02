using Game.Content;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>DECISION-0142: shrink, hidden camera flight, appearance; pure paused-clock projection.</summary>
    public sealed class PortalTransitState
    {
        private readonly float _collapse, _travel, _expand;
        public Vector2 From { get; }
        public Vector2 To { get; }
        public float Elapsed { get; private set; }
        public PortalTransitPhase Phase => Elapsed < _collapse ? PortalTransitPhase.Collapsing :
            Elapsed < _collapse + _travel ? PortalTransitPhase.Traveling :
            Elapsed < _collapse + _travel + _expand ? PortalTransitPhase.Appearing : PortalTransitPhase.Complete;
        public bool HasArrived => Elapsed >= _collapse + _travel;
        public float Scale => Phase == PortalTransitPhase.Collapsing ? 1f - Smooth(Elapsed / _collapse) :
            HasArrived ? Smooth((Elapsed - _collapse - _travel) / _expand) : 0f;
        public float Flash => Phase == PortalTransitPhase.Collapsing ? 1f - Scale :
            Phase == PortalTransitPhase.Appearing ? 1f - Scale : 0f;
        public Vector2 CameraPosition => Vector2.Lerp(From, To, Smooth((Elapsed - _collapse) / _travel));
        public PortalTransitState(Vector2 from, Vector2 to, float collapse, float travel, float expand)
        {
            NumericValidation.ValidateFinite(from.x, nameof(from)); NumericValidation.ValidateFinite(from.y, nameof(from));
            NumericValidation.ValidateFinite(to.x, nameof(to)); NumericValidation.ValidateFinite(to.y, nameof(to));
            NumericValidation.ValidatePositive(collapse, nameof(collapse)); NumericValidation.ValidatePositive(travel, nameof(travel));
            NumericValidation.ValidatePositive(expand, nameof(expand));
            From = from; To = to; _collapse = collapse; _travel = travel; _expand = expand;
        }
        public void Tick(float seconds, bool running)
        {
            NumericValidation.ValidateNonNegativeFinite(seconds, nameof(seconds));
            if (running) Elapsed = Mathf.Min(Elapsed + seconds, _collapse + _travel + _expand);
        }
        private static float Smooth(float value) => Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(value));
    }
}
