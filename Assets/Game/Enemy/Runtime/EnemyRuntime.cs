using System;
using Game.Character;
using Game.Movement;
using Game.Run;
using UnityEngine;

namespace Game.Enemy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class EnemyRuntime : MonoBehaviour
    {
        private Rigidbody2D _body;
        private CircleCollider2D _collider;
        private SpriteRenderer _renderer;
        private Transform _target;
        private RunController _runController;
        private PlayerCharacterRuntime _contactTarget;
        private ContinuousContactTimer _contactTimer;
        private bool _initialized;
        private bool _despawned;

        public EnemyDefinition Definition { get; private set; }
        public EnemyHealth Health { get; private set; }

        public event Action<EnemyRuntime> Died;
        public event Action<EnemyRuntime> Despawned;

        private void Awake()
        {
            CacheComponents();
            if (_renderer.sprite == null)
                _renderer.sprite = PlaceholderSprite.Shared;
            _renderer.color = new Color(0.85f, 0.2f, 0.2f, 1f);
        }

        public void Initialize(EnemyDefinition definition, Transform target, RunController runController)
        {
            if (_initialized)
                throw new InvalidOperationException("Enemy runtime is already initialized.");

            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _target = target != null ? target : throw new ArgumentNullException(nameof(target));
            _runController = runController != null ? runController : throw new ArgumentNullException(nameof(runController));

            CacheComponents();
            _body.gravityScale = 0f;
            _body.constraints |= RigidbodyConstraints2D.FreezeRotation;
            _collider.radius = 0.5f;
            transform.localScale = Vector3.one * definition.CollisionSize;
            gameObject.name = $"Enemy [{definition.Id}]";

            Health = new EnemyHealth(definition.MaxHealth);
            Health.Died += HandleDeath;
            _contactTimer = new ContinuousContactTimer(definition.ContactDamageInterval);
            _initialized = true;
        }

        private void FixedUpdate()
        {
            if (!_initialized || _despawned)
                return;

            var isSimulating = _runController.Model != null &&
                               _runController.Model.State == RunState.Running &&
                               !Health.IsDead;
            _body.linearVelocity = EnemyMovement.CalculateSeekVelocity(
                _body.position,
                _target.position,
                Definition.MovementSpeed,
                isSimulating);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!_initialized || _despawned || _runController.Model == null)
                return;

            var player = collision.gameObject.GetComponent<PlayerCharacterRuntime>();
            if (player == null || player.Health == null)
                return;

            _contactTarget = player;
            ApplyContactHits(_contactTimer.BeginContact(IsRunRunning()));
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!_initialized || _despawned || _contactTarget == null)
                return;
            if (collision.gameObject != _contactTarget.gameObject)
                return;

            ApplyContactHits(_contactTimer.Tick(Time.fixedDeltaTime, IsRunRunning()));
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (_contactTarget == null || collision.gameObject != _contactTarget.gameObject)
                return;

            _contactTimer.EndContact();
            _contactTarget = null;
        }

        public float TakeDamage(float amount)
        {
            if (!_initialized)
                throw new InvalidOperationException("Enemy runtime must be initialized before receiving damage.");

            return Health.TakeDamage(amount);
        }

        public void Despawn()
        {
            if (_despawned)
                return;

            _despawned = true;
            if (_body != null)
                _body.linearVelocity = Vector2.zero;
            if (Health != null)
                Health.Died -= HandleDeath;
            _contactTimer?.EndContact();
            _contactTarget = null;

            Despawned?.Invoke(this);

            if (Application.isPlaying)
                Destroy(gameObject);
            else
                DestroyImmediate(gameObject);
        }

        private void HandleDeath()
        {
            _body.linearVelocity = Vector2.zero;
            _collider.enabled = false;
            Died?.Invoke(this);
            Despawn();
        }

        private void OnDestroy()
        {
            if (Health != null)
                Health.Died -= HandleDeath;

            if (_despawned)
                return;

            _despawned = true;
            Despawned?.Invoke(this);
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

        private bool IsRunRunning()
        {
            return _runController.Model != null && _runController.Model.State == RunState.Running;
        }

        private void ApplyContactHits(int hitCount)
        {
            if (_contactTarget == null || _contactTarget.Health == null || _runController.Model == null)
                return;

            for (var i = 0; i < hitCount && !_contactTarget.Health.IsDead; i++)
            {
                EnemyContactDamage.Apply(
                    Definition.ContactDamage,
                    _contactTarget.Health,
                    _runController.Model.State);
            }
        }
    }
}
