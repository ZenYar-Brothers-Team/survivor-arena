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
        private RunController _runController;
        private GameObjectPool<EnemyProjectileRuntime> _pool;
        private EnemyProjectileLifetime _lifetime;
        private bool _initialized;
        private bool _despawned;

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
            _profile = profile ?? throw new System.ArgumentNullException(nameof(profile));
            _runController = runController != null ? runController : throw new System.ArgumentNullException(nameof(runController));
            _target = target;
            Source = source;
            _pool = pool;
            _direction = direction.sqrMagnitude <= Mathf.Epsilon ? Vector2.right : direction.normalized;
            _lifetime = new EnemyProjectileLifetime(profile.ProjectileLifetimeSeconds);
            _despawned = false;
            _initialized = true;

            CacheComponents();
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
        }

        private void FixedUpdate()
        {
            if (!_initialized || _despawned || _runController.Model == null)
                return;
            var state = _runController.Model.State;
            if (state == RunState.Won || state == RunState.Lost || state == RunState.Stopped)
            {
                Despawn();
                return;
            }
            var running = state == RunState.Running;
            _body.linearVelocity = running ? _direction * _profile.ProjectileSpeed : Vector2.zero;
            if (_lifetime.Tick(Time.fixedDeltaTime, running))
            {
                if (_profile.Pattern == EnemyProjectilePattern.Explosive)
                    ApplyImpact(_body.position);
                Despawn();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_initialized || _despawned || _runController.Model?.State != RunState.Running || _target == null || other.gameObject != _target.gameObject)
                return;
            ApplyImpact(_body.position);
            Despawn();
        }

        private void ApplyImpact(Vector2 impactPosition)
        {
            if (_target == null || _target.Health == null || _runController.Model == null)
                return;
            if (_runController.Model.State != RunState.Running) return;
            var radial = (Vector2)_target.transform.position - impactPosition;
            if (_profile.Pattern == EnemyProjectilePattern.Explosive && radial.sqrMagnitude > _profile.ExplosionRadius * _profile.ExplosionRadius)
                return;
            var direction = _profile.Pattern == EnemyProjectilePattern.Explosive ? radial : _direction;
            _target.ApplyDamage(new CombatDamageRequest(Source, _profile.Damage, _profile.Controls, direction.x, direction.y));
        }

        public void Despawn()
        {
            if (_despawned)
                return;
            _despawned = true;
            if (_body != null)
                _body.linearVelocity = Vector2.zero;
            if (_collider != null)
                _collider.enabled = false;
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
