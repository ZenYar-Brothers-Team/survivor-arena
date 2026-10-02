using Game.ActiveSkill;
using Game.Combat;
using Game.Enemy;
using Game.Movement;
using Game.Presentation;
using Game.Run;
using UnityEngine;

namespace Game.Bootstrap
{
    /// <summary>Owns the temporary transit lock and composes portal feedback; restores every captured actor state.</summary>
    public sealed class PortalTransitRuntime : MonoBehaviour, IEnemyLifeTransient
    {
        private PortalTransitState _state;
        private RunController _run;
        private Rigidbody2D _body;
        private Collider2D _collider;
        private Health _health;
        private SpritePresentationRuntime _presentation;
        private CameraFollowTarget _camera;
        private MonoBehaviour _actor, _skills;
        private bool _simulated, _colliding, _locked, _actorEnabled, _skillsEnabled, _visualActive, _arrived;
        public bool IsActive => _state != null;

        public bool Begin(Vector2 destination, RunController run, ZoneSealPresentationProfile profile,
            Health health, SpritePresentationRuntime presentation, CameraFollowTarget camera = null)
        {
            if (IsActive) return false;
            _run = run; _health = health; _presentation = presentation; _camera = camera;
            _body = GetComponent<Rigidbody2D>(); _collider = GetComponent<Collider2D>();
            _actor = GetComponent<EnemyRuntime>();
            if (_actor == null) _actor = GetComponent<PlayerMover>();
            _skills = GetComponent<PlayerActiveSkillSetRuntime>();
            _simulated = _body != null && _body.simulated; _colliding = _collider != null && _collider.enabled;
            _locked = health.IsLocked; _actorEnabled = _actor != null && _actor.enabled; _skillsEnabled = _skills != null && _skills.enabled;
            _visualActive = presentation != null && presentation.gameObject.activeSelf;
            _state = new PortalTransitState(transform.position, destination, profile.PortalCollapseSeconds,
                profile.PortalTransitSeconds, profile.PortalExpandSeconds);
            _arrived = false;
            health.IsLocked = true;
            if (_actor != null) _actor.enabled = false;
            if (_skills != null) _skills.enabled = false;
            if (_body != null) { _body.linearVelocity = Vector2.zero; _body.simulated = false; }
            if (_collider != null) _collider.enabled = false;
            Apply(); return true;
        }
        private void Update()
        {
            if (_state == null) return;
            if (_run.Model.State != RunState.Running) return;
            _state.Tick(Time.deltaTime, true); Apply();
        }
        private void Apply()
        {
            if (_state.HasArrived && !_arrived)
            {
                transform.position = _state.To;
                if (_body != null) _body.position = _state.To;
                _arrived = true;
            }
            if (_camera != null)
            {
                _camera.SetTransitFocus(_state.CameraPosition);
                _camera.CenterOnTarget();
            }
            if (_presentation != null)
            {
                _presentation.gameObject.SetActive(_visualActive && _state.Phase != PortalTransitPhase.Traveling);
                _presentation.SetTransitPose(_state.Scale, _state.Scale, _state.Flash);
            }
            if (_state.Phase == PortalTransitPhase.Complete) Shutdown();
        }
        public void Shutdown()
        {
            if (_state == null) return;
            // Completion and interrupted cleanup both finish at the exit, avoiding an immediate re-entry loop.
            transform.position = _state.To;
            if (_body != null) { _body.position = _state.To; _body.simulated = _simulated; _body.linearVelocity = Vector2.zero; }
            if (_collider != null) _collider.enabled = _colliding;
            if (_health != null) _health.IsLocked = _locked;
            if (_actor != null) _actor.enabled = _actorEnabled;
            if (_skills != null) _skills.enabled = _skillsEnabled;
            if (_presentation != null) { _presentation.gameObject.SetActive(_visualActive); _presentation.SetTransitPose(1f, 1f, 0f); }
            if (_camera != null) { _camera.SetTransitFocus(null); _camera.CenterOnTarget(); }
            _state = null; _run = null; _health = null; _presentation = null; _camera = null;
        }
        private void OnDisable() => Shutdown();
        private void OnDestroy() => Shutdown();
    }
}
