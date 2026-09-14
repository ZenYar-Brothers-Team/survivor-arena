using Game.Movement;
using Game.Run;
using UnityEngine;

namespace Game.Progression
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ExperienceDropRuntime : MonoBehaviour
    {
        private CircleCollider2D _collider;
        private PlayerExperienceRuntime _target;
        private RunController _runController;
        private ExperienceDropTimer _timer;
        private bool _consumed;

        public float Amount { get; private set; }
        public float Lifetime => _timer != null ? _timer.Lifetime : 0f;
        public bool IsConsumed => _consumed;

        private void Awake()
        {
            _collider = GetComponent<CircleCollider2D>();
            _collider.isTrigger = true;
            _collider.radius = 0.2f;

            var renderer = GetComponent<SpriteRenderer>();
            if (renderer.sprite == null)
                renderer.sprite = PlaceholderSprite.Shared;
            renderer.color = new Color(0.25f, 0.85f, 1f, 1f);
            transform.localScale = Vector3.one * 0.35f;
        }

        public void Initialize(
            float amount,
            float lifetime,
            PlayerExperienceRuntime target,
            RunController runController)
        {
            if (amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount))
                throw new System.ArgumentOutOfRangeException(nameof(amount));

            Amount = amount;
            _target = target != null ? target : throw new System.ArgumentNullException(nameof(target));
            _runController = runController != null ? runController : throw new System.ArgumentNullException(nameof(runController));
            _timer = new ExperienceDropTimer(lifetime);
            gameObject.name = "Experience Drop";
        }

        private void Update()
        {
            if (_consumed || _timer == null)
                return;

            var isRunning = _runController.Model != null && _runController.Model.State == RunState.Running;
            if (_timer.Tick(Time.deltaTime, isRunning))
                Expire();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_target != null && other.gameObject == _target.gameObject)
                TryPickup();
        }

        public bool TryPickup()
        {
            if (_consumed || !IsRunRunning())
                return false;

            _consumed = true;
            _target.AddPickedUpExperience(Amount);
            DestroySelf();
            return true;
        }

        public bool TickForTests(float deltaTime)
        {
            if (_consumed || _timer == null || !_timer.Tick(deltaTime, IsRunRunning()))
                return false;

            Expire();
            return true;
        }

        private void Expire()
        {
            if (_consumed)
                return;

            _consumed = true;
            _target.AddRecoveredExperience(Amount);
            DestroySelf();
        }

        private bool IsRunRunning()
        {
            return _runController != null &&
                   _runController.Model != null &&
                   _runController.Model.State == RunState.Running;
        }

        private void DestroySelf()
        {
            if (Application.isPlaying)
                Destroy(gameObject);
            else
                DestroyImmediate(gameObject);
        }
    }
}
