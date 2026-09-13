using System;
using Game.Enemy;
using Game.Movement;
using Game.Run;
using UnityEngine;

namespace Game.ActiveSkill
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class FixtureProjectileRuntime : MonoBehaviour
    {
        private Rigidbody2D _body;
        private CircleCollider2D _collider;
        private SpriteRenderer _renderer;
        private RunController _runController;
        private ActiveSkillProjectile _projectile;
        private ProjectileLifetime _lifetime;
        private bool _initialized;
        private bool _despawned;

        public Vector2 Direction => _projectile.Direction;
        public Vector2 Position => _body != null ? _body.position : transform.position;
        public bool IsDespawned => _despawned;

        private void Awake()
        {
            CacheComponents();
            if (_renderer.sprite == null)
                _renderer.sprite = PlaceholderSprite.Shared;
            _renderer.color = new Color(1f, 0.85f, 0.15f, 1f);
        }

        public void Initialize(ActiveSkillProjectile projectile, RunController runController)
        {
            if (_initialized)
                throw new InvalidOperationException("Projectile runtime is already initialized.");

            _projectile = projectile;
            _runController = runController != null
                ? runController
                : throw new ArgumentNullException(nameof(runController));
            _lifetime = new ProjectileLifetime(projectile.LifetimeSeconds);

            CacheComponents();
            _body.bodyType = RigidbodyType2D.Kinematic;
            _body.gravityScale = 0f;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _body.constraints |= RigidbodyConstraints2D.FreezeRotation;
            _collider.isTrigger = true;
            _collider.radius = projectile.CollisionRadius;
            transform.position = projectile.Origin;
            _initialized = true;
        }

        private void FixedUpdate()
        {
            Simulate(Time.fixedDeltaTime);
        }

        public void Simulate(float deltaTime)
        {
            if (!_initialized || _despawned || _runController.Model == null)
                return;

            var state = _runController.Model.State;
            if (state == RunState.Won || state == RunState.Lost)
            {
                Despawn();
                return;
            }

            var isRunning = state == RunState.Running;
            if (!isRunning)
                return;

            _body.position += _projectile.Direction * (_projectile.Speed * deltaTime);
            if (_lifetime.Tick(deltaTime, isRunning))
                Despawn();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_initialized || _despawned || !IsRunRunning())
                return;

            var receiver = other.GetComponentInParent<IEnemyDamageReceiver>();
            TryImpact(receiver, other.ClosestPoint(_body.position));
        }

        public bool TryImpact(IEnemyDamageReceiver receiver, Vector2 impactPoint)
        {
            if (!_initialized || _despawned || !IsRunRunning() || receiver == null || !receiver.IsAlive)
                return false;

            EnemyDamageArea.Apply(
                impactPoint,
                _projectile.ImpactAreaRadius,
                _projectile.Damage,
                receiver);
            Despawn();
            return true;
        }

        public void Despawn()
        {
            if (_despawned)
                return;

            _despawned = true;
            if (Application.isPlaying)
                Destroy(gameObject);
            else
                DestroyImmediate(gameObject);
        }

        private bool IsRunRunning()
        {
            return _runController != null &&
                   _runController.Model != null &&
                   _runController.Model.State == RunState.Running;
        }

        private void CacheComponents()
        {
            if (_body == null)
                _body = GetComponent<Rigidbody2D>();
            if (_collider == null)
                _collider = GetComponent<CircleCollider2D>();
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();
        }
    }
}
