using System;
using Game.Content;
using Game.Movement;
using Game.Pooling;
using Game.Run;
using UnityEngine;
using Game.Presentation;

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
        private PickupSpritePresentation _presentation;

        public float Amount { get; private set; }
        public ExperienceDropIdentity Identity { get; private set; }
        public float PickupRadius => _target != null && _target.IsInitialized ? _target.PickupRadius : 0f;
        public float Lifetime => _timer != null ? _timer.Lifetime : 0f;
        public bool IsConsumed => _consumed;

        private void Awake()
        {
            CacheComponents();
            _collider.isTrigger = true;

            var renderer = GetComponent<SpriteRenderer>();
            renderer.enabled = false;
            EnsurePresentation();
        }

        public void Initialize(
            float amount,
            float lifetime,
            PlayerExperienceRuntime target,
            RunController runController,
            float pickupRadius,
            GameObjectPool<ExperienceDropRuntime> pool = null,
            Guid? sourceLifeId = null,
            ContentId? sourceContentId = null)
        {
            NumericValidation.ValidatePositive(amount, nameof(amount));
            NumericValidation.ValidatePositive(pickupRadius, nameof(pickupRadius));
            var timer = new ExperienceDropTimer(lifetime);
            if (target == null || !target.IsInitialized) throw new ArgumentException("XP target must be initialized.", nameof(target));
            if (runController == null || runController.Model == null) throw new ArgumentException("XP run must be initialized.", nameof(runController));
            _target?.UnregisterDrop(this);
            Identity = new ExperienceDropIdentity(Guid.NewGuid(), runController.Model.RunId, sourceLifeId, sourceContentId);
            Amount = amount;
            _target = target != null ? target : throw new System.ArgumentNullException(nameof(target));
            _runController = runController != null ? runController : throw new System.ArgumentNullException(nameof(runController));
            _timer = timer;
            _pool = pool;
            _consumed = false;
            CacheComponents();
            EnsurePresentation();
            var visual = target.DropVisual;
            _presentation.Initialize(visual != null ? visual.Sprite : PlaceholderSprite.Shared,
                visual != null ? target.DropVisualScale : 0.35f,
                visual != null ? Color.white : new Color(0.25f, 0.85f, 1f, 1f),
                Mathf.Repeat(transform.position.x * 1.7f + transform.position.y * 2.3f, Mathf.PI * 2f));
            _collider.enabled = true;
            RefreshColliderRadius();
            _target.RegisterDrop(this);
            gameObject.name = "Experience Drop";
        }

        private void CacheComponents()
        {
            if (_collider == null)
                _collider = GetComponent<CircleCollider2D>();
        }

        private void EnsurePresentation()
        {
            _presentation ??= new PickupSpritePresentation(transform, 5);
        }

        private void Update() => Tick(Time.deltaTime);

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_target != null && other.gameObject == _target.gameObject) TryPickupWithinRadius();
        }

        public bool TryPickupWithinRadius()
        {
            if (_consumed || !IsRunRunning()) return false;
            var radius = PickupRadius;
            if (((Vector2)transform.position - (Vector2)_target.transform.position).sqrMagnitude > radius * radius) return false;
            return TryPickup();
        }

        public bool TryPickup()
        {
            if (_consumed || !IsRunRunning()) return false;
            Consume(ExperienceEventKind.Collected);
            return true;
        }

        /// <summary>Expiry is checked first, preserving the previous timer-before-pickup contract.</summary>
        public bool Tick(float deltaTime)
        {
            if (_consumed || _timer == null) return false;
            RefreshColliderRadius();
            if (IsRunRunning()) _presentation.Tick(deltaTime);
            if (_timer.Tick(deltaTime, IsRunRunning()))
            {
                Consume(ExperienceEventKind.Expired);
                return true;
            }
            return TryPickupWithinRadius();
        }

        public bool TickForTests(float deltaTime) => Tick(deltaTime);

        private void Consume(ExperienceEventKind kind)
        {
            _consumed = true;
            _collider.enabled = false;
            _target.UnregisterDrop(this);
            // Claims the drop before level-up callbacks can re-enter; return it even if an observer throws.
            try
            {
                if (kind == ExperienceEventKind.Collected) _target.AddPickedUpExperience(Amount, Identity);
                else _target.AddRecoveredExperience(Amount, Identity);
            }
            finally { DestroySelf(); }
        }

        private void RefreshColliderRadius()
        {
            // The sprite scale must not shrink the gameplay radius. Distance is the authoritative
            // test so the player's own collider cannot silently extend this radius either.
            var scale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
            var localRadius = PickupRadius / Mathf.Max(scale, Mathf.Epsilon);
            if (_collider.radius != localRadius) _collider.radius = localRadius;
        }

        public void Shutdown()
        {
            if (_consumed) return;
            _consumed = true;
            _target?.UnregisterDrop(this);
            if (_collider != null) _collider.enabled = false;
            _presentation?.Shutdown();
            DestroySelf();
        }

        private void OnDestroy() => _target?.UnregisterDrop(this);

        private bool IsRunRunning()
        {
            return _target != null && _target.IsInitialized && _runController != null &&
                   _runController.Model != null &&
                   _runController.Model.State == RunState.Running && _runController.Model.RunId == Identity.RunId;
        }

        private void DestroySelf()
        {
            _presentation?.Shutdown();
            if (_pool != null && _target != null && _target.IsInitialized)
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
