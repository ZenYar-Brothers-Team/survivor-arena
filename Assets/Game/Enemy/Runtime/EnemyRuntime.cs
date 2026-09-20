using System;
using Game.Character;
using Game.Combat;
using Game.Movement;
using Game.Pooling;
using Game.Progression;
using Game.Run;
using UnityEngine;

namespace Game.Enemy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(LineRenderer))]
    public sealed class EnemyRuntime : MonoBehaviour, IEnemyDamageReceiver
    {
        private Rigidbody2D _body;
        private CircleCollider2D _collider;
        private SpriteRenderer _renderer;
        private LineRenderer _telegraph;
        private Transform _target;
        private RunController _runController;
        private PlayerCharacterRuntime _contactTarget;
        private PlayerCharacterRuntime _projectileTarget;
        private ContinuousContactTimer _contactTimer;
        private PlayerExperienceRuntime _experienceTarget;
        private GameObjectPool<EnemyRuntime> _pool;
        private GameObjectPool<EnemyProjectileRuntime> _projectilePool;
        private EnemyMovementController _movementController;
        private EnemyAttackController _attackController;
        private bool _initialized;
        private bool _despawned;

        public EnemyDefinition Definition { get; private set; }
        public Health Health { get; private set; }
        public bool IsAlive => _initialized && !_despawned && Health != null && !Health.IsDead;
        public Vector2 Position => transform.position;
        public EnemyMovementPhase MovementPhase { get; private set; }
        public EnemyProjectilePattern? AttackPattern => Definition?.Attack?.Pattern;

        public event Action<EnemyRuntime> Died;
        public event Action<EnemyRuntime> Despawned;

        private void Awake()
        {
            CacheComponents();
            _renderer.color = new Color(0.85f, 0.2f, 0.2f, 1f);
        }

        public void Initialize(
            EnemyDefinition definition,
            Transform target,
            RunController runController,
            PlayerExperienceRuntime experienceTarget = null,
            Sprite visual = null,
            GameObjectPool<EnemyRuntime> pool = null,
            GameObjectPool<EnemyProjectileRuntime> projectilePool = null)
        {
            if (_initialized)
            {
                // Reused from a pool: tear down the previous life before rebuilding,
                // instead of the single-use "throw if already initialized" guard.
                if (Health != null)
                {
                    Health.Died -= HandleDeath;
                    Health.Dispose();
                }
                _despawned = false;
            }

            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _target = target != null ? target : throw new ArgumentNullException(nameof(target));
            // Resolved once per life: it is needed for every projectile this enemy fires.
            _projectileTarget = target.GetComponent<PlayerCharacterRuntime>();
            _runController = runController != null ? runController : throw new ArgumentNullException(nameof(runController));
            _experienceTarget = experienceTarget;
            _pool = pool;
            _projectilePool = projectilePool;

            CacheComponents();
            _collider.enabled = true;
            _body.gravityScale = 0f;
            _body.constraints |= RigidbodyConstraints2D.FreezeRotation;
            _collider.radius = 0.5f;
            transform.localScale = Vector3.one * definition.CollisionSize;
            gameObject.name = $"Enemy [{definition.Id}]";
            // definition.Visual (when set) is resolved by the caller ahead of time and
            // handed in as a plain Sprite, so this class never needs to know about
            // ContentRegistry/ContentRef at all.
            _renderer.sprite = visual != null ? visual : PlaceholderSprite.Shared;

            _movementController = new EnemyMovementController(definition.Movement);
            _attackController = definition.Attack == null ? null : new EnemyAttackController(definition.Attack);
            MovementPhase = EnemyMovementPhase.Seeking;
            ConfigureTelegraph();

            Health = new Health(new FixedHealthProfile(definition.MaxHealth));
            Health.Died += HandleDeath;
            _contactTimer = new ContinuousContactTimer(definition.ContactDamageInterval);
            _initialized = true;
            EnemyRegistry.Register(this);
        }

        private void FixedUpdate()
        {
            if (!_initialized || _despawned)
                return;

            var isSimulating = _runController.Model != null &&
                               _runController.Model.State == RunState.Running &&
                               !Health.IsDead;
            var movement = _movementController.Tick(
                _body.position,
                _target.position,
                Definition.MovementSpeed,
                Time.fixedDeltaTime,
                isSimulating);
            _body.linearVelocity = movement.Velocity;
            MovementPhase = movement.Phase;
            RenderTelegraph(movement);

            if (!isSimulating || _attackController == null)
                return;
            var shots = _attackController.Tick(
                Time.fixedDeltaTime,
                isSimulating,
                (Vector2)_target.position - _body.position);
            for (var i = 0; i < shots.Length; i++)
            {
                EnemyProjectileFactory.Spawn(
                    Definition.Attack,
                    _body.position,
                    shots[i].Direction,
                    _projectileTarget,
                    _runController,
                    transform.parent,
                    _projectilePool);
            }
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

        public float ApplyDamage(EnemyDamageRequest request)
        {
            if (!_initialized)
                throw new InvalidOperationException("Enemy runtime must be initialized before receiving damage.");

            return Health.TakeDamage(request.Amount);
        }

        public void Despawn()
        {
            if (_despawned)
                return;

            _despawned = true;
            if (_body != null)
                _body.linearVelocity = Vector2.zero;
            if (_telegraph != null)
                _telegraph.enabled = false;
            if (Health != null)
                Health.Died -= HandleDeath;
            _contactTimer?.EndContact();
            _contactTarget = null;
            EnemyRegistry.Unregister(this);

            Despawned?.Invoke(this);

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

        private void HandleDeath()
        {
            _body.linearVelocity = Vector2.zero;
            _collider.enabled = false;
            if (Definition.ExperienceReward > 0f && _experienceTarget != null)
            {
                ExperienceDropFactory.Spawn(
                    Definition.ExperienceReward,
                    transform.position,
                    _experienceTarget.DropLifetimeSeconds,
                    _experienceTarget,
                    _runController,
                    pool: _experienceTarget.DropPool);
            }
            Died?.Invoke(this);
            Despawn();
        }

        private void OnDestroy()
        {
            EnemyRegistry.Unregister(this);
            if (Health != null)
            {
                Health.Died -= HandleDeath;
                Health.Dispose();
            }

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
            if (_telegraph == null)
                _telegraph = GetComponent<LineRenderer>();
        }

        private void ConfigureTelegraph()
        {
            _telegraph.enabled = false;
            _telegraph.useWorldSpace = true;
            _telegraph.positionCount = 2;
            _telegraph.startWidth = 0.08f;
            _telegraph.endWidth = 0.025f;
            _telegraph.startColor = new Color(1f, 0.25f, 0.1f, 0.9f);
            _telegraph.endColor = new Color(1f, 0.75f, 0.1f, 0.25f);
            _telegraph.sortingOrder = 5;
        }

        private void RenderTelegraph(EnemyMovementFrame movement)
        {
            _telegraph.enabled = movement.IsTelegraphing;
            if (!movement.IsTelegraphing)
                return;
            var start = (Vector3)_body.position;
            var length = Mathf.Max(2f, Definition.MovementSpeed * Definition.Movement.DashSpeedMultiplier *
                Definition.Movement.DashDurationSeconds);
            _telegraph.SetPosition(0, start);
            _telegraph.SetPosition(1, start + (Vector3)(movement.TelegraphDirection * length));
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
