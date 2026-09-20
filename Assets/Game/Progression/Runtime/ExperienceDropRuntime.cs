using Game.Movement;
using Game.Pooling;
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
        private GameObjectPool<ExperienceDropRuntime> _pool;
        private bool _consumed;

        public float Amount { get; private set; }
        public float Lifetime => _timer != null ? _timer.Lifetime : 0f;
        public bool IsConsumed => _consumed;

        private void Awake()
        {
            CacheComponents();
            _collider.isTrigger = true;

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
            RunController runController,
            float pickupRadius,
            GameObjectPool<ExperienceDropRuntime> pool = null)
        {
            if (amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount))
                throw new System.ArgumentOutOfRangeException(nameof(amount));
            if (pickupRadius <= 0f || float.IsNaN(pickupRadius) || float.IsInfinity(pickupRadius))
                throw new System.ArgumentOutOfRangeException(nameof(pickupRadius));

            Amount = amount;
            _target = target != null ? target : throw new System.ArgumentNullException(nameof(target));
            _runController = runController != null ? runController : throw new System.ArgumentNullException(nameof(runController));
            _timer = new ExperienceDropTimer(lifetime);
            _pool = pool;
            _consumed = false;
            CacheComponents();
            _collider.radius = pickupRadius;
            gameObject.name = "Experience Drop";
        }

        private void CacheComponents()
        {
            if (_collider == null)
                _collider = GetComponent<CircleCollider2D>();
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
            if (_pool != null)
            {
                _pool.Return(this);
                return;
            }

            if (Application.isPlaying)
                Destroy(gameObject);
            else
                DestroyImmediate(gameObject);
        }
    }
}
