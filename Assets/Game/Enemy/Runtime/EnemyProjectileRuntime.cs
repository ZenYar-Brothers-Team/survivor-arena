using Game.Character;
using Game.Combat;
using Game.Movement;
using Game.Pooling;
using Game.Run;
using UnityEngine;

namespace Game.Enemy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class EnemyProjectileRuntime : MonoBehaviour
    {
        private Rigidbody2D _body;
        private CircleCollider2D _collider;
        private SpriteRenderer _renderer;
        private EnemyAttackProfile _profile;
        private Vector2 _direction;
        private PlayerCharacterRuntime _target;
        private RunModel _run;
        private TrailRenderer _trail;
        private GameObjectPool<EnemyProjectileRuntime> _pool;
        private EnemyProjectileLifetime _lifetime;
        private bool _initialized;
        private bool _despawned;

        public bool IsActive => _initialized && !_despawned;
        public float RemainingSeconds => _lifetime?.RemainingSeconds ?? 0f;
        public EnemyAttackProfile Profile => _profile;
        public CombatSource Source { get; private set; }

        public void Initialize(
            EnemyAttackProfile profile,
            Vector2 direction,
            PlayerCharacterRuntime target,
            RunController runController,
            GameObjectPool<EnemyProjectileRuntime> pool = null,
            CombatSource source = default)
        {
            if (profile == null) throw new System.ArgumentNullException(nameof(profile));
            if (runController == null || runController.Model == null) throw new System.ArgumentException("Projectile requires an initialized run.", nameof(runController));
            ClearState();
            _profile = profile;
            _run = runController.Model;
            _run.StateChanged += HandleRunState;
            _target = target;
            Source = source;
            _pool = pool;
            _direction = direction.sqrMagnitude <= Mathf.Epsilon ? Vector2.right : direction.normalized;
            _lifetime = new EnemyProjectileLifetime(profile.ProjectileLifetimeSeconds);
            _despawned = false;
            _initialized = true;

            CacheComponents();
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
            _body.rotation = 0f;
            transform.localRotation = Quaternion.identity;
            if (_trail != null) { _trail.Clear(); _trail.emitting = true; }
            _renderer.enabled = true;
            _renderer.flipX = _renderer.flipY = false;
            _body.bodyType = RigidbodyType2D.Kinematic;
            _body.gravityScale = 0f;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _collider.isTrigger = true;
            _collider.radius = 0.5f;
            _collider.enabled = true;
            transform.localScale = Vector3.one * (profile.ProjectileRadius * 2f);
            _renderer.sprite = PlaceholderSprite.Shared;
            _renderer.color = profile.Pattern == EnemyProjectilePattern.Explosive
                ? new Color(1f, 0.25f, 0.05f, 1f)
                : new Color(1f, 0.8f, 0.15f, 1f);
            gameObject.name = $"Enemy Projectile [{profile.Pattern}]";
            HandleRunState(_run.State);
        }

        private void FixedUpdate() => Tick(Time.fixedDeltaTime);

        public void Tick(float deltaTime)
        {
            Game.Content.NumericValidation.ValidateNonNegative(deltaTime, nameof(deltaTime));
            if (!_initialized || _despawned || _run == null)
                return;
            var state = _run.State;
            if (state == RunState.Won || state == RunState.Lost || state == RunState.Stopped)
            {
                Despawn();
                return;
            }
            var running = state == RunState.Running;
            _body.linearVelocity = running ? _direction * _profile.ProjectileSpeed : Vector2.zero;
            if (_lifetime.Tick(deltaTime, running))
            {
                if (_profile.Pattern == EnemyProjectilePattern.Explosive) ApplyImpact(_body.position);
                else Despawn();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_initialized || _despawned || _run?.State != RunState.Running || _target == null || other.gameObject != _target.gameObject)
                return;
            ApplyImpact(_body.position);
        }

        private void ApplyImpact(Vector2 impactPosition)
        {
            if (!IsActive || _run?.State != RunState.Running) return;
            // Return before callbacks. A lethal hit can end the run and immediately reuse a pool slot.
            var target = _target;
            var profile = _profile;
            var source = Source;
            var direction = _direction;
            var canHit = target != null && target.Health != null && !target.Health.IsDead;
            if (canHit && profile.Pattern == EnemyProjectilePattern.Explosive)
            {
                direction = (Vector2)target.transform.position - impactPosition;
                canHit = direction.sqrMagnitude <= profile.ExplosionRadius * profile.ExplosionRadius;
            }
            Despawn();
            if (canHit) target.ApplyDamage(new CombatDamageRequest(source, profile.Damage, profile.Controls, direction.x, direction.y));
        }

        private void HandleRunState(RunState state)
        {
            // A prior run's multicast invocation may still contain this handler after pool reuse.
            if (_run == null || _run.State != state) return;
            if (state == RunState.Won || state == RunState.Lost || state == RunState.Stopped) Despawn();
            else if (state != RunState.Running && _body != null) _body.linearVelocity = Vector2.zero;
        }

        public void Despawn()
        {
            if (!IsActive) return;
            var pool = _pool;
            ClearState();
            if (pool != null) { pool.Return(this); return; }
            if (Application.isPlaying) Destroy(gameObject);
            else DestroyImmediate(gameObject);
        }

        public void Shutdown() => Despawn();
        private void OnDisable() => ClearState();
        private void OnDestroy() => ClearState();

        private void ClearState()
        {
            if (_run != null) _run.StateChanged -= HandleRunState;
            _initialized = false;
            _despawned = true;
            if (_body != null) { _body.linearVelocity = Vector2.zero; _body.angularVelocity = 0f; }
            if (_collider != null) _collider.enabled = false;
            if (_renderer != null) _renderer.enabled = false;
            if (_trail != null) { _trail.emitting = false; _trail.Clear(); }
            Source = default;
            _profile = null;
            _target = null;
            _run = null;
            _pool = null;
            _lifetime = null;
            _direction = Vector2.zero;
        }

        private void CacheComponents()
        {
            if (_body == null)
                _body = GetComponent<Rigidbody2D>();
            if (_collider == null)
                _collider = GetComponent<CircleCollider2D>();
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();
            if (_trail == null) _trail = GetComponent<TrailRenderer>();
        }
    }
}
