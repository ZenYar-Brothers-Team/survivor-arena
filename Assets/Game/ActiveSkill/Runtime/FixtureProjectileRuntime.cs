using System;
using System.Collections.Generic;
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
        private readonly HashSet<IEnemyDamageReceiver> _hitThisPass = new HashSet<IEnemyDamageReceiver>();
        private Vector2 _direction;
        private float _elapsed;
        private int _remainingHits;
        private bool _isReturning;

        public Vector2 Direction => _direction;
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
            _direction = projectile.Direction;
            _remainingHits = 1 + projectile.PierceCount;
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

            _elapsed += deltaTime;
            if (_projectile.Returns && !_isReturning && _elapsed >= _projectile.ReturnAfterSeconds)
            {
                _isReturning = true;
                _hitThisPass.Clear();
            }

            if (_isReturning)
            {
                var returnOffset = (Vector2)_projectile.ReturnTarget.position - _body.position;
                var travelDistance = _projectile.Speed * deltaTime;
                if (returnOffset.sqrMagnitude <= travelDistance * travelDistance)
                {
                    Despawn();
                    return;
                }
                if (returnOffset.sqrMagnitude > Mathf.Epsilon)
                    _direction = returnOffset.normalized;
            }

            _body.position += _direction * (_projectile.Speed * deltaTime);
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
            if (!_hitThisPass.Add(receiver))
                return false;

            var damage = _projectile.Damage;
            if (_isReturning && _projectile.ReturnDamageMultiplier != 1f)
            {
                damage = new EnemyDamageRequest(
                    damage.SourceId,
                    damage.Amount * _projectile.ReturnDamageMultiplier);
            }
            EnemyDamageArea.Apply(
                impactPoint,
                _projectile.ImpactAreaRadius,
                damage,
                receiver);
            if (!_projectile.Returns)
            {
                _remainingHits--;
                if (_remainingHits <= 0)
                    Despawn();
            }
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
