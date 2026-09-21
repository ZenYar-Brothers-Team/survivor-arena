using System;
using Game.Character;
using Game.Combat;
using Game.Content;
using Game.Diagnostics;
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
    [RequireComponent(typeof(LineRenderer))]
    public sealed class EnemyRuntime : MonoBehaviour, IEnemyLifeTarget
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
        private IEnemyLifecycleSink _lifecycleSink;
        private ContentId? _damageSource;
        private Guid? _runId;
        private bool _deathPublished;
        private bool _dispatchingLifecycle;
        private GameObjectPool<EnemyRuntime> _pool;
        private GameObjectPool<EnemyProjectileRuntime> _projectilePool;
        private EnemyMovementController _movementController;
        private EnemyAttackController _attackController;
        private bool _initialized;
        private bool _despawned;

        public EnemyDefinition Definition { get; private set; }
        public Guid LifeId { get; private set; }
        public ContentId ContentId => Definition.Id;
        public EnemyCategory Category { get; private set; }
        public EnemyLifeEvent LastLifeEvent { get; private set; }
        public Health Health { get; private set; }
        public CombatControlState Controls { get; } = new CombatControlState();
        public CombatIdentity Identity => new CombatIdentity(LifeId, _runId, ContentId, Category switch
        {
            EnemyCategory.Boss => CombatEntityCategory.Boss,
            EnemyCategory.Traveler => CombatEntityCategory.Traveler,
            _ => CombatEntityCategory.OrdinaryEnemy
        });
        public event Action<CombatResult> CombatResolved;
        public bool IsAlive => _initialized && !_despawned && Health != null && !Health.IsDead;
        public Vector2 Position => transform.position;
        public EnemyMovementPhase MovementPhase { get; private set; }
        public EnemyAttackPhase? AttackPhase => _attackController?.Phase;
        public float AttackPhaseRemaining => _attackController?.PhaseRemaining ?? 0f;
        public int BurstShotsRemaining => _attackController?.BurstShotsRemaining ?? 0;
        public CombatSource LastProjectileSource { get; private set; }
        public CombatControlProfile CurrentContactControls => MovementPhase == EnemyMovementPhase.Dashing
            ? Definition.DashContactControls : Definition.ContactControls;
        public EnemyProjectilePattern? AttackPattern => Definition?.Attack?.Pattern;

        public event Action<EnemyRuntime> Died;
        public event Action<EnemyRuntime> Despawned;
        public event Action<EnemyLifeEvent> LifeEvent;

        private void Awake()
        {
            CacheComponents();
            _renderer.color = new Color(0.85f, 0.2f, 0.2f, 1f);
        }

        public void Initialize(
            EnemyDefinition definition,
            Transform target,
            RunController runController,
            IEnemyLifecycleSink lifecycleSink = null,
            Sprite visual = null,
            GameObjectPool<EnemyRuntime> pool = null,
            GameObjectPool<EnemyProjectileRuntime> projectilePool = null,
            EnemyCategory category = EnemyCategory.Ordinary)
        {
            if (_dispatchingLifecycle) throw new InvalidOperationException("Cannot reuse an enemy during lifecycle callbacks.");
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (runController == null) throw new ArgumentNullException(nameof(runController));
            if (!Enum.IsDefined(typeof(EnemyCategory), category)) throw new ArgumentOutOfRangeException(nameof(category));
            var reused = _initialized;
            if (_initialized)
            {
                if (!_despawned) EndLife(EnemyLifeReason.Reinitialized, releaseObject: false);
                if (Health != null)
                {
                    Health.Died -= HandleDeath;
                    Health.Dispose();
                }
            }
            Controls.Reset();
            LastProjectileSource = default;
            _despawned = false;
            _deathPublished = false;
            _damageSource = null;
            LifeId = Guid.NewGuid();
            Category = category;

            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _target = target != null ? target : throw new ArgumentNullException(nameof(target));
            // Resolved once per life: it is needed for every projectile this enemy fires.
            _projectileTarget = target.GetComponent<PlayerCharacterRuntime>();
            _runController = runController != null ? runController : throw new ArgumentNullException(nameof(runController));
            _runId = runController.Model?.RunId;
            _lifecycleSink = lifecycleSink;
            _pool = pool;
            _projectilePool = projectilePool;

            CacheComponents();
            _collider.enabled = true;
            _body.gravityScale = 0f;
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
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
            _dispatchingLifecycle = true;
            try { Publish(EnemyLifeEventKind.Spawned, reused ? EnemyLifeReason.PoolReuse : EnemyLifeReason.Spawn); }
            finally { _dispatchingLifecycle = false; }
        }

        private void FixedUpdate()
        {
            if (!_initialized || _despawned)
                return;

            using var guard = PerfGuard.Measure("EnemyRuntime.Tick", 2f);
            var isSimulating = _runController.Model != null &&
                               _runController.Model.State == RunState.Running &&
                               !Health.IsDead;
            var control = Controls.Tick(Time.fixedDeltaTime, isSimulating);
            var movement = _movementController.Tick(
                _body.position,
                _target.position,
                Definition.MovementSpeed * control.MovementMultiplier,
                Time.fixedDeltaTime,
                isSimulating);
            _body.linearVelocity = movement.Velocity + new Vector2(control.KnockbackX, control.KnockbackY);
            MovementPhase = movement.Phase;
            if (!isSimulating || _attackController == null)
            {
                RenderTelegraph(movement);
                return;
            }
            var shots = _attackController.Tick(
                Time.fixedDeltaTime,
                isSimulating,
                (Vector2)_target.position - _body.position);
            RenderTelegraph(movement);
            for (var i = 0; i < shots.Length; i++)
            {
                LastProjectileSource = new CombatSource(Identity, Definition.Id, CombatSourceOrigin.EnemyProjectile);
                EnemyProjectileFactory.Spawn(
                    Definition.Attack,
                    _body.position,
                    shots[i].Direction,
                    _projectileTarget,
                    _runController,
                    transform.parent,
                    _projectilePool,
                    LastProjectileSource);
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
            return ResolveDamage(new CombatDamageRequest(default, amount)).Health.Actual;
        }

        public float ApplyDamage(EnemyDamageRequest request)
        {
            return ResolveDamage(request.Combat).Health.Actual;
        }

        public CombatResult ResolveDamage(CombatDamageRequest request)
        {
            if (!_initialized)
                throw new InvalidOperationException("Enemy runtime must be initialized before receiving damage.");
            var identity = Identity;
            if (!IsAlive || _dispatchingLifecycle)
                return new CombatResult(request.Source, identity, new HealthChange(request.Amount, 0f, 0f, false));
            // Death may dispose this component and clear its events before TakeDamageMeasured returns.
            var notify = CombatResolved;
            var previousSource = _damageSource;
            _damageSource = request.Source.ContentId;
            try
            {
                var distance = IsRunRunning() ? Controls.Apply(request, Definition.KnockbackResistance, acceptsSlow: true) : 0f;
                var result = new CombatResult(request.Source, identity, Health.TakeDamageMeasured(request.Amount), distance);
                notify?.Invoke(result);
                return result;
            }
            finally { _damageSource = previousSource; }
        }

        public void Despawn(EnemyLifeReason reason = EnemyLifeReason.Cleanup)
        {
            if (reason != EnemyLifeReason.Cleanup && reason != EnemyLifeReason.Escaped)
                throw new ArgumentOutOfRangeException(nameof(reason), "Only cleanup or escape may be requested externally.");
            // Death notification owns its final cleanup; callbacks cannot return/re-rent this life halfway through it.
            if (_dispatchingLifecycle) return;
            EndLife(reason, releaseObject: true);
        }

        public void Shutdown() => Despawn();

        private void EndLife(EnemyLifeReason reason, bool releaseObject)
        {
            if (_despawned || !_initialized)
                return;

            _despawned = true;
            Controls.Reset();
            if (_body != null)
                _body.linearVelocity = Vector2.zero;
            if (_telegraph != null)
                _telegraph.enabled = false;
            if (_collider != null) _collider.enabled = false;
            if (Health != null)
            {
                Health.Died -= HandleDeath;
                Health.Dispose();
            }
            _contactTimer?.EndContact();
            _contactTarget = null;
            EnemyRegistry.Unregister(this);

            _dispatchingLifecycle = true;
            try
            {
                Publish(EnemyLifeEventKind.Despawned, reason);
                Despawned?.Invoke(this);
            }
            finally
            {
                _dispatchingLifecycle = false;
                Died = null;
                Despawned = null;
                LifeEvent = null;
                CombatResolved = null;
                _lifecycleSink = null;
                if (releaseObject) ReleaseObject();
            }
        }

        private void ReleaseObject()
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

        private void HandleDeath()
        {
            if (_deathPublished || _despawned) return;
            _deathPublished = true;
            _body.linearVelocity = Vector2.zero;
            _collider.enabled = false;
            _dispatchingLifecycle = true;
            try
            {
                Publish(EnemyLifeEventKind.Died, EnemyLifeReason.Killed);
                Died?.Invoke(this);
            }
            finally
            {
                _dispatchingLifecycle = false;
                EndLife(EnemyLifeReason.Killed, releaseObject: true);
            }
        }

        private void OnDestroy()
        {
            EndLife(EnemyLifeReason.Destroyed, releaseObject: false);
        }

        private void Publish(EnemyLifeEventKind kind, EnemyLifeReason reason)
        {
            var snapshot = new EnemyLifeEvent(LifeId, _runId,
                Definition, Category, kind, reason, Position, _damageSource);
            LastLifeEvent = snapshot;
            _lifecycleSink?.OnEnemyLifeEvent(snapshot);
            LifeEvent?.Invoke(snapshot);
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
            _telegraph.sharedMaterial = _renderer.sharedMaterial;
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
            var attackTelegraph = _attackController?.Phase == EnemyAttackPhase.Telegraphing;
            _telegraph.enabled = movement.IsTelegraphing || attackTelegraph;
            if (!_telegraph.enabled) return;
            var start = (Vector3)_body.position;
            var length = Mathf.Max(2f, Definition.MovementSpeed * Definition.Movement.DashSpeedMultiplier *
                Definition.Movement.DashDurationSeconds);
            var direction = movement.IsTelegraphing ? movement.TelegraphDirection : _attackController.AimDirection;
            if (!movement.IsTelegraphing) length = Definition.Attack.ProjectileSpeed * Definition.Attack.TelegraphSeconds;
            _telegraph.SetPosition(0, start);
            _telegraph.SetPosition(1, start + (Vector3)(direction * length));
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
                if (!IsRunRunning()) break;
                var direction = (Vector2)_contactTarget.transform.position - Position;
                _contactTarget.ApplyDamage(new CombatDamageRequest(
                    new CombatSource(Identity, Definition.Id, CombatSourceOrigin.EnemyContact),
                    Definition.ContactDamage, CurrentContactControls, direction.x, direction.y));
            }
        }
    }
}
