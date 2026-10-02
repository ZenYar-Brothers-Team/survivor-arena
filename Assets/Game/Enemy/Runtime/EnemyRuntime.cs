using System;
using Game.Character;
using Game.Combat;
using Game.Content;
using Game.Diagnostics;
using Game.Movement;
using Game.Pooling;
using Game.Run;
using Game.Presentation;
using UnityEngine;

namespace Game.Enemy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(LineRenderer))]
    public sealed class EnemyRuntime : MonoBehaviour, IEnemyLifeTarget, IEnemyControlReceiver
    {
        private Rigidbody2D _body;
        private CircleCollider2D _collider;
        private SpriteRenderer _renderer;
        private SpritePresentationRuntime _presentation;
        private SpritePresentationRig _presentationRig;
        private EnemyDeathPresentationRuntime _deathPresentation;
        private EnemyDeathPresentationProfile _deathProfile;
        private GroundShadowRuntime _groundShadow;
        private GroundShadowPresentationProfile _groundShadowProfile;
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
        private bool _dying;
        private bool _dispatchingLifecycle;
        private GameObjectPool<EnemyRuntime> _pool;
        private GameObjectPool<EnemyProjectileRuntime> _projectilePool;
        private EnemyMovementController _movementController;
        private EnemyAttackController _attackController;
        private EnemyDashVolleyController _dashVolley;
        // Body movement, or the active boss phase's dash override (DECISION-0066, E3).
        private EnemyMovementProfile _movementProfile;
        private EnemyMovementProfile _spawnMovementProfile;
        private Vector2 _blobWaypoint;
        private float _blobDelay;
        private float _blobRemaining;
        private bool _holdAttacksDuringDash;
        // Shoving dash (DECISION-0118): pass-through state and per-dash hit bookkeeping.
        private const int DashShoveBufferSize = 48;
        private Collider2D[] _dashShoveHits;
        private readonly System.Collections.Generic.HashSet<EnemyRuntime> _dashShoved = new System.Collections.Generic.HashSet<EnemyRuntime>();
        private bool _dashPassThrough;
        private bool _dashHitPlayer;
        private LayerMask _dashSavedExclude;
        public BossCombatController BossCombat { get; private set; }
        private EnemyAttackProfile CurrentAttack => BossCombat != null ? BossCombat.AttackDefinition?.Attack : Definition?.Attack;
        private bool _initialized;
        private bool _despawned;
        private IEnemyMovementDriver _movementDriver;
        private System.Random _movementRandom;
        private Func<bool> _damageAllowed;
        private ContentRegistry _contentRegistry;
        public EnemyProtection Protection { get; } = new EnemyProtection();

        public void ConfigureEncounter(IEnemyMovementDriver movement, Func<bool> damageAllowed)
        { _movementDriver = movement; _damageAllowed = damageAllowed; }

        public EnemyDefinition Definition { get; private set; }
        public Guid LifeId { get; private set; }
        public ContentId ContentId => Definition.Id;
        public EnemyCategory Category { get; private set; }
        public EnemyLifeEvent LastLifeEvent { get; private set; }
        public Health Health { get; private set; }
        public CombatControlState Controls { get; } = new CombatControlState();
        /// <summary>Area-only slow, replaced each zone tick; it has no timed status or unit presentation.</summary>
        public float AreaSlowFraction { get; private set; }

        public void SetAreaSlowFraction(float fraction)
        {
            Game.Content.NumericValidation.ValidateRange(fraction, 0f, .95f, nameof(fraction));
            AreaSlowFraction = fraction;
        }
        /// <summary>Body presentation when production art is bound; null for fixture bodies (DECISION-0108 status overlays).</summary>
        public SpritePresentationRuntime BodyPresentation =>
            _presentation != null && _presentation.IsInitialized && _presentationRig.gameObject.activeSelf ? _presentation : null;
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
        public EnemyProjectilePattern? AttackPattern => CurrentAttack?.Pattern;

        public event Action<EnemyRuntime> Died;
        public event Action<EnemyRuntime> Despawned;
        /// <summary>A boss step or dash end started a zone, beam or summon (DECISION-0066); the encounter owner runs it.</summary>
        public event Action<EnemyRuntime, BossSpecialRequest> SpecialRequested;
        /// <summary>Movement profile in effect (body movement or the boss phase override).</summary>
        public EnemyMovementProfile CurrentMovement => _movementProfile ?? Definition?.Movement;
        public bool BlobBreakupActive => _blobDelay > 0f || _blobRemaining > 0f;
        public event Action<EnemyLifeEvent> LifeEvent;
        /// <summary>Arc-burst enemies (DECISION-0146) keep their formation: never picked for blob breakup. Reset per life.</summary>
        public bool BlobBreakupExempt { get; set; }

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
            EnemyCategory category = EnemyCategory.Ordinary,
            SpriteMotionProfile motionProfile = null,
            SpriteContactProfile contact = null,
            EnemyDeathPresentationProfile deathPresentation = null,
            GroundShadowPresentationProfile groundShadowPresentation = null,
            ContentRegistry contentRegistry = null,
            EnemyMovementProfile movement = null,
            int? movementSeed = null)
        {
            if (_dispatchingLifecycle) throw new InvalidOperationException("Cannot reuse an enemy during lifecycle callbacks.");
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (runController == null) throw new ArgumentNullException(nameof(runController));
            if (!Enum.IsDefined(typeof(EnemyCategory), category)) throw new ArgumentOutOfRangeException(nameof(category));
            if (motionProfile != null && (visual == null || runController.Model == null))
                throw new ArgumentException("Enemy body motion requires a sprite and initialized run.");
            if (contact != null && motionProfile == null)
                throw new ArgumentException("Fitted contact requires the matching animated body.");
            _presentation?.Shutdown();
            _deathPresentation?.ResetPresentation();
            if (_presentationRig != null) _presentationRig.gameObject.SetActive(false);
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
            AreaSlowFraction = 0f;
            Protection.Reset(); _movementDriver = null; _damageAllowed = null;
            BossCombat = null;
            LastProjectileSource = default;
            _despawned = false;
            _deathPublished = false;
            _dying = false;
            _deathProfile = deathPresentation;
            _groundShadowProfile = groundShadowPresentation;
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
            _contentRegistry = contentRegistry;

            CacheComponents();
            // Only the root carries the collider; skill area damage queries this layer only (DECISION-0056).
            gameObject.layer = EnemyPhysicsLayer.Index;
            _collider.enabled = true;
            EndDashPassThrough();
            _body.gravityScale = 0f;
            _body.simulated = true;
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
            _body.constraints |= RigidbodyConstraints2D.FreezeRotation;
            _collider.radius = 0.5f;
            _collider.offset = Vector2.zero;
            contact?.Apply(_collider, definition.CollisionSize);
            transform.localScale = Vector3.one * definition.CollisionSize;
            gameObject.name = $"Enemy [{definition.Id}]";
            // definition.Visual (when set) is resolved by the caller ahead of time and
            // handed in as a plain Sprite, so this class never needs to know about
            // ContentRegistry/ContentRef at all.
            _renderer.sprite = visual != null ? visual : PlaceholderSprite.Shared;
            _renderer.enabled = true;
            _renderer.flipX = false;
            _renderer.color = visual != null ? Color.white : new Color(0.85f, 0.2f, 0.2f, 1f);

            _movementRandom = new System.Random(movementSeed ?? LifeId.GetHashCode());
            _spawnMovementProfile = movement ?? definition.SelectMovement((float)_movementRandom.NextDouble());
            _movementProfile = _spawnMovementProfile;
            _holdAttacksDuringDash = false;
            _movementController = new EnemyMovementController(_movementProfile, _movementRandom);
            _blobDelay = 0f;
            _blobRemaining = 0f;
            _dashVolley = definition.DashVolley == null ? null : new EnemyDashVolleyController(definition.DashVolley);
            // Aim deviation is per life, so neighbouring archers do not fire identical patterns.
            _attackController = definition.Attack == null ? null
                : new EnemyAttackController(definition.Attack, random: new System.Random(LifeId.GetHashCode()));
            BlobBreakupExempt = false;
            MovementPhase = EnemyMovementPhase.Seeking;
            ConfigureTelegraph();

            Health = new Health(new FixedHealthProfile(definition.MaxHealth));
            Health.Died += HandleDeath;
            if (motionProfile != null)
                InitializePresentation(visual, motionProfile, contact);
            InitializeGroundShadow(contact);
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

            if (_damageAllowed != null && !_damageAllowed()) return;

            using var guard = PerfGuard.Measure("EnemyRuntime.Tick", 2f);
            var isSimulating = _runController.Model != null &&
                               _runController.Model.State == RunState.Running &&
                               !Health.IsDead;
            var control = Controls.Tick(Time.fixedDeltaTime, isSimulating);
            Protection.Tick(_runController.Model?.Elapsed ?? 0f);
            // MIDBOSS-009 slows down while winding up (DECISION-0066, E6).
            var windup = _attackController?.Phase == EnemyAttackPhase.Telegraphing && CurrentAttack != null
                ? CurrentAttack.WindupMovementMultiplier : 1f;
            var speed = Definition.MovementSpeed * Mathf.Min(control.MovementMultiplier, 1f - AreaSlowFraction) * windup * Protection.SpeedMultiplier;
            var movement = _movementDriver != null
                ? _movementDriver.Tick(_body.position, _target.position, speed, Time.fixedDeltaTime, isSimulating)
                : _movementController.Tick(_body.position, _target.position, speed, Time.fixedDeltaTime, isSimulating);
            var steering = ApplyBlobBreakup(movement, speed, Time.fixedDeltaTime, isSimulating);
            _body.linearVelocity = steering + new Vector2(control.KnockbackX, control.KnockbackY);
            MovementPhase = movement.Phase;
            UpdateDashShove(movement, isSimulating);
            if (_dashVolley != null) FireDashVolley(movement, isSimulating);
            if (!isSimulating || (_attackController == null && BossCombat == null))
            {
                RenderTelegraph(movement);
                return;
            }
            var aim = (Vector2)_target.position - _body.position;
            var dashing = movement.Phase == EnemyMovementPhase.Dashing || movement.Phase == EnemyMovementPhase.TelegraphingDash;
            // DECISION-0066 E5: the sequence waits, as in pause, while the boss telegraphs or performs a dash.
            var attacking = !(_holdAttacksDuringDash && dashing);
            EnemyShotCommand[] shots;
            if (BossCombat != null)
            {
                shots = BossCombat.Tick(Time.fixedDeltaTime, attacking, Health.CurrentHealth / Health.MaxHealth, aim);
                _attackController = BossCombat.Attack;
                if (BossCombat.TriggeredSpecial != null)
                    SpecialRequested?.Invoke(this, BossCombat.TriggeredSpecial.ToRequest(BossCombat.TriggeredSpecialId));
                ApplyPhaseMovement(dashing);
            }
            else shots = _attackController.Tick(Time.fixedDeltaTime, true, aim);
            RenderTelegraph(movement);
            for (var i = 0; i < shots.Length; i++)
            {
                LastProjectileSource = new CombatSource(Identity, BossCombat?.AttackDefinition?.Id ?? Definition.Id, CombatSourceOrigin.EnemyProjectile);
                EnemyProjectileFactory.Spawn(
                    CurrentAttack,
                    _body.position,
                    shots[i].Direction,
                    _projectileTarget,
                    _runController,
                    transform.parent,
                    _projectilePool,
                    LastProjectileSource,
                    ResolveProjectileVisual(CurrentAttack));
            }
        }

        /// <summary>Assigns one temporary fan waypoint to an ordinary enemy; never changes its base profile.</summary>
        public bool TryStartBlobBreakup(Vector2 waypoint, float delaySeconds, float maneuverSeconds)
        {
            if (!IsAlive || Category != EnemyCategory.Ordinary || BlobBreakupActive || BlobBreakupExempt ||
                float.IsNaN(waypoint.x) || float.IsNaN(waypoint.y) ||
                float.IsInfinity(waypoint.x) || float.IsInfinity(waypoint.y) ||
                delaySeconds < 0f || maneuverSeconds <= 0f)
                return false;
            _blobWaypoint = waypoint;
            _blobDelay = delaySeconds;
            _blobRemaining = maneuverSeconds;
            return true;
        }

        public void CancelBlobBreakup()
        {
            if (_blobDelay > 0f) _blobRemaining = 0f;
            _blobDelay = 0f;
            _blobRemaining = Mathf.Min(_blobRemaining, .3f);
        }

        private Vector2 ApplyBlobBreakup(EnemyMovementFrame movement, float speed, float deltaTime, bool isSimulating)
        {
            if (!isSimulating || !BlobBreakupActive) return movement.Velocity;
            if (_blobDelay > 0f)
            {
                _blobDelay = Mathf.Max(0f, _blobDelay - deltaTime);
                return movement.Velocity;
            }
            _blobRemaining = Mathf.Max(0f, _blobRemaining - deltaTime);
            if (movement.Phase == EnemyMovementPhase.Dashing ||
                movement.Phase == EnemyMovementPhase.TelegraphingDash || _movementDriver != null)
                return movement.Velocity;
            var offset = _blobWaypoint - _body.position;
            if (offset.sqrMagnitude < .04f) _blobRemaining = 0f;
            if (_blobRemaining <= 0f) return movement.Velocity;
            var weight = Mathf.Clamp01(_blobRemaining / .4f);
            return Vector2.Lerp(movement.Velocity, offset.normalized * speed, weight);
        }

        // Swap the dash series only between dashes, so a running dash is never cut short (DECISION-0066, E3).
        private void ApplyPhaseMovement(bool dashing)
        {
            var wanted = BossCombat.Phase.MovementOverride ?? _spawnMovementProfile;
            if (dashing || ReferenceEquals(wanted, _movementProfile)) return;
            _movementProfile = wanted;
            _movementController = new EnemyMovementController(wanted, _movementRandom);
        }

        // DECISION-0063/0066: dash-end volleys leave the moment the dash series stops; the dash telegraph is their warning.
        private void FireDashVolley(EnemyMovementFrame movement, bool isSimulating)
        {
            var shots = _dashVolley.Tick(movement.Phase, Health.CurrentHealth / Health.MaxHealth, Time.fixedDeltaTime, isSimulating,
                (Vector2)_target.position - _body.position, movement.TelegraphDirection);
            foreach (var zone in _dashVolley.TriggeredZones)
                SpecialRequested?.Invoke(this, BossSpecialRequest.ForZone(zone, Definition.Id));
            if (shots.Length == 0) return;
            LastProjectileSource = new CombatSource(Identity, Definition.Id, CombatSourceOrigin.EnemyProjectile);
            for (var i = 0; i < shots.Length; i++)
            {
                var attack = shots[i].Attack ?? Definition.DashVolley.Attack;
                EnemyProjectileFactory.Spawn(attack, _body.position, shots[i].Direction, _projectileTarget, _runController,
                    transform.parent, _projectilePool, LastProjectileSource, ResolveProjectileVisual(attack));
            }
        }

        private SpriteDefinition ResolveProjectileVisual(EnemyAttackProfile attack)
        {
            if (_contentRegistry == null || attack == null || !attack.ProjectileVisual.Id.IsValid) return null;
            var visual = attack.ProjectileVisual.Resolve(_contentRegistry);
            visual.RequireRole(SpriteRole.Projectile);
            return visual;
        }

        private void Update()
        {
            if (!_dying || _deathPresentation == null || _runController?.Model?.State != RunState.Running)
                return;
            if (_deathPresentation.Tick(Time.deltaTime))
                EndLife(EnemyLifeReason.Killed, releaseObject: true);
        }

        public void ConfigureBoss(BossEncounterDefinition encounter)
        {
            if (!IsAlive || Category != EnemyCategory.Boss || encounter == null || encounter.Id != Definition.Id)
                throw new InvalidOperationException("Boss combat requires the matching active boss life.");
            BossCombat = new BossCombatController(encounter);
            _attackController = BossCombat.Attack;
            _holdAttacksDuringDash = encounter.HoldAttacksDuringDash;
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

        /// <summary>Applies only the request's movement controls (e.g. SET-010 orbit slow); no damage, no hit event.</summary>
        public void ApplyControl(CombatDamageRequest request)
        {
            if (!_initialized || !IsAlive || _dispatchingLifecycle || !IsRunRunning()) return;
            Controls.Apply(request.WithAmount(0f).WithDirection(0f, 0f), Mathf.Min(1, Definition.KnockbackResistance + Protection.ResistanceBonus), acceptsSlow: true);
        }

        public CombatResult ResolveDamage(CombatDamageRequest request)
        {
            if (!_initialized)
                throw new InvalidOperationException("Enemy runtime must be initialized before receiving damage.");
            var identity = Identity;
            if (!IsAlive || _dispatchingLifecycle || (_damageAllowed != null && !_damageAllowed()))
                return new CombatResult(request.Source, identity, new HealthChange(request.Amount, 0f, 0f, false));
            // Death may dispose this component and clear its events before TakeDamageMeasured returns.
            var notify = CombatResolved;
            var previousSource = _damageSource;
            _damageSource = request.Source.ContentId;
            try
            {
                Protection.Tick(_runController.Model?.Elapsed ?? 0f);
                // "Already slowed" is decided before this hit applies its own slow (DECISION-0053).
                if (Controls.MovementMultiplier < 1f || AreaSlowFraction > 0f) request = request.ResolveForSlowedTarget();
                // Vulnerability mark (SET-010, DECISION-0139) is decided before this hit applies its own controls.
                if (Controls.DamageTakenMultiplier > 1f) request = request.WithAmount(request.Amount * Controls.DamageTakenMultiplier);
                var distance = IsRunRunning() ? Controls.Apply(request, Mathf.Min(1, Definition.KnockbackResistance + Protection.ResistanceBonus), acceptsSlow: true) : 0f;
                var measured = Health.TakeDamageMeasured(Protection.Absorb(request.Amount, _runController.Model?.Elapsed ?? 0f));
                var result = new CombatResult(request.Source, identity, new HealthChange(request.Amount, measured.AfterMitigation, measured.Actual, false), distance);
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

        /// <summary>Removes an ordinary enemy for cap replacement without lifecycle, death or reward callbacks.</summary>
        public void EraseSilently()
        {
            if (Category != EnemyCategory.Ordinary)
                throw new InvalidOperationException("Only ordinary enemies may be erased for cap replacement.");
            if (_dispatchingLifecycle) return;
            EndLife(EnemyLifeReason.Cleanup, releaseObject: true, publishLifecycle: false);
        }

        public void Shutdown() => Despawn();

        private void EndLife(EnemyLifeReason reason, bool releaseObject, bool publishLifecycle = true)
        {
            if (_despawned || !_initialized)
                return;

            _despawned = true;
            _blobDelay = 0f;
            _blobRemaining = 0f;
            _dying = false;
            _deathPresentation?.ResetPresentation();
            _presentation?.Shutdown();
            if (_presentationRig != null) _presentationRig.gameObject.SetActive(false);
            _groundShadow?.Shutdown();
            Controls.Reset();
            AreaSlowFraction = 0f;
            Protection.Reset(); _movementDriver = null; _damageAllowed = null;
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
                if (publishLifecycle)
                {
                    Publish(EnemyLifeEventKind.Despawned, reason);
                    Despawned?.Invoke(this);
                }
            }
            finally
            {
                _dispatchingLifecycle = false;
                Died = null;
                Despawned = null;
                LifeEvent = null;
                CombatResolved = null;
                SpecialRequested = null;
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
            _dying = true;
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
            _body.simulated = false;
            _collider.enabled = false;
            _telegraph.enabled = false;
            _contactTimer?.EndContact();
            _contactTarget = null;
            EnemyRegistry.Unregister(this);
            _dispatchingLifecycle = true;
            try
            {
                Publish(EnemyLifeEventKind.Died, EnemyLifeReason.Killed);
                Died?.Invoke(this);
            }
            finally
            {
                _dispatchingLifecycle = false;
                BeginDeathPresentationOrRelease();
            }
        }

        private void BeginDeathPresentationOrRelease()
        {
            if (_deathProfile == null)
            {
                EndLife(EnemyLifeReason.Killed, releaseObject: true);
                return;
            }
            var sourceRenderer = _presentationRig != null && _presentationRig.gameObject.activeSelf
                ? _presentationRig.BodyRenderer : _renderer;
            var sourceTransform = sourceRenderer.transform;
            if (_deathPresentation == null)
                _deathPresentation = gameObject.AddComponent<EnemyDeathPresentationRuntime>();
            _deathPresentation.Begin(_deathProfile, sourceTransform, sourceRenderer);
            _presentation?.Shutdown();
            if (_presentationRig != null) _presentationRig.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            EndLife(EnemyLifeReason.Destroyed, releaseObject: false);
        }

        private void OnDisable()
        {
            _presentation?.Shutdown();
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

        // Art is independent of collision-size scaling; only the child pose is animated.
        // ASSET_PIPELINE body/pooling contract; the existing player pose writer is reused.
        private void InitializePresentation(Sprite sprite, SpriteMotionProfile profile, SpriteContactProfile contact)
        {
            if (_presentation == null)
            {
                var visual = new GameObject("VisualRoot");
                visual.transform.SetParent(transform, false);
                var body = new GameObject("BodyRoot");
                body.transform.SetParent(visual.transform, false);
                var renderer = body.AddComponent<SpriteRenderer>();
                renderer.sharedMaterial = _renderer.sharedMaterial;
                renderer.sortingLayerID = _renderer.sortingLayerID;
                renderer.sortingOrder = _renderer.sortingOrder;
                _presentationRig = visual.AddComponent<SpritePresentationRig>();
                _presentationRig.Configure(body.transform, renderer);
                _presentation = visual.AddComponent<SpritePresentationRuntime>();
            }
            _presentationRig.transform.localScale = Vector3.one / Definition.CollisionSize;
            _presentationRig.gameObject.SetActive(true);
            _presentation.Initialize(new SpriteDefinition(Definition.Visual.Id, sprite, SpriteRole.Body, contact),
                profile, Health, _body, _runController);
            _renderer.enabled = false;
        }

        private void InitializeGroundShadow(SpriteContactProfile contact)
        {
            if (_groundShadowProfile == null)
            {
                _groundShadow?.Shutdown();
                return;
            }
            if (_groundShadow == null) _groundShadow = gameObject.AddComponent<GroundShadowRuntime>();
            var usesScaleCompensatedBody = _presentationRig != null && _presentationRig.gameObject.activeSelf;
            var bodyRenderer = usesScaleCompensatedBody ? _presentationRig.BodyRenderer : _renderer;
            _groundShadow.Initialize(_groundShadowProfile, contact,
                usesScaleCompensatedBody ? Definition.CollisionSize : 1f, bodyRenderer);
        }

        /// <summary>
        /// DECISION-0118: while a shoving dash runs the body ignores enemy and player colliders, every ordinary enemy it
        /// sweeps over is pushed sideways once, and the player takes one contact hit per dash. One overlap query per
        /// physics tick, only during the dash; the buffer is reused and nothing is allocated.
        /// </summary>
        private void UpdateDashShove(EnemyMovementFrame movement, bool isSimulating)
        {
            var move = CurrentMovement;
            var active = isSimulating && move.DashShoves && movement.Phase == EnemyMovementPhase.Dashing;
            if (!active)
            {
                if (_dashPassThrough) EndDashPassThrough();
                return;
            }
            using var guard = PerfGuard.Measure("EnemyRuntime.DashShove", 1f);
            if (!_dashPassThrough)
            {
                _dashPassThrough = true;
                _dashSavedExclude = _collider.excludeLayers;
                var mask = 1 << EnemyPhysicsLayer.Index;
                var playerLayer = LayerMask.NameToLayer("Player");
                if (playerLayer >= 0) mask |= 1 << playerLayer;
                _collider.excludeLayers = _dashSavedExclude | mask;
                _dashShoved.Clear();
                _dashHitPlayer = false;
            }
            _dashShoveHits ??= new Collider2D[DashShoveBufferSize];
            var filter = ContactFilter2D.noFilter;
            var queryMask = 1 << EnemyPhysicsLayer.Index;
            var playerQueryLayer = LayerMask.NameToLayer("Player");
            if (playerQueryLayer >= 0) queryMask |= 1 << playerQueryLayer;
            filter.SetLayerMask(queryMask);
            var origin = _body.position;
            var count = Physics2D.OverlapCircle(origin, move.DashShoveRadius, filter, _dashShoveHits);
            var forward = movement.TelegraphDirection;
            for (var i = 0; i < count; i++)
            {
                var hit = _dashShoveHits[i];
                if (hit == null || hit.gameObject == gameObject) continue;
                if (hit.TryGetComponent(out EnemyRuntime other))
                {
                    if (!other.IsAlive || other.Category != EnemyCategory.Ordinary || other._runId != _runId ||
                        !_dashShoved.Add(other)) continue;
                    var relative = other.Position - origin;
                    var cross = forward.x * relative.y - forward.y * relative.x;
                    // Dead-centre hits alternate by life id so a packed line splits to both sides.
                    var side = Mathf.Abs(cross) > 0.05f ? Mathf.Sign(cross) : ((other.LifeId.GetHashCode() & 1) == 0 ? 1f : -1f);
                    var push = new Vector2(-forward.y, forward.x) * side + forward * 0.3f;
                    other.ApplyShove(push, move.DashShoveDistance, move.DashShoveSeconds, Identity, Definition.Id);
                }
                else if (!_dashHitPlayer && hit.TryGetComponent(out PlayerCharacterRuntime player))
                {
                    _dashHitPlayer = true;
                    if (player.Health == null || player.Health.IsDead || Definition.ContactDamage <= 0) continue;
                    var direction = (Vector2)player.transform.position - origin;
                    player.ApplyDamage(new CombatDamageRequest(
                        new CombatSource(Identity, Definition.Id, CombatSourceOrigin.EnemyContact),
                        Definition.ContactDamage, Definition.DashContactControls, direction.x, direction.y));
                }
            }
        }

        private void EndDashPassThrough()
        {
            if (!_dashPassThrough) return;
            _dashPassThrough = false;
            _collider.excludeLayers = _dashSavedExclude;
            _dashShoved.Clear();
        }

        /// <summary>Knockback only (no damage, no hit event, no slow), reduced by this enemy's knockback resistance.</summary>
        public void ApplyShove(Vector2 direction, float distance, float seconds, CombatIdentity source, ContentId sourceId)
        {
            if (!_initialized || !IsAlive || _dispatchingLifecycle || !IsRunRunning() || distance <= 0f || seconds <= 0f) return;
            var request = new CombatDamageRequest(new CombatSource(source, sourceId, CombatSourceOrigin.EnemyContact), 0f,
                new CombatControlProfile(distance, seconds), direction.x, direction.y);
            Controls.Apply(request, Mathf.Min(1, Definition.KnockbackResistance + Protection.ResistanceBonus), acceptsSlow: false);
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
            var move = CurrentMovement;
            var attackTelegraph = _attackController?.Phase == EnemyAttackPhase.Telegraphing && CurrentAttack != null;
            var dashLine = movement.IsTelegraphing && move.ShowDashTelegraphLine;
            _telegraph.enabled = dashLine || attackTelegraph;
            if (!_telegraph.enabled) return;
            var start = (Vector3)_body.position;
            var length = move.DashTelegraphLength > 0f ? move.DashTelegraphLength
                : move.DashDistance > 0f ? move.DashDistance
                : Mathf.Max(2f, Definition.MovementSpeed * move.DashSpeedMultiplier * move.DashDurationSeconds);
            var direction = dashLine ? movement.TelegraphDirection : _attackController.AimDirection;
            if (!dashLine) length = CurrentAttack.ProjectileSpeed * CurrentAttack.TelegraphSeconds;
            // A dash telegraph with its own width keeps a constant band; every other telegraph keeps the thin tapered line.
            var wide = dashLine && move.DashTelegraphWidth > 0f;
            _telegraph.startWidth = wide ? move.DashTelegraphWidth : 0.08f;
            _telegraph.endWidth = wide ? move.DashTelegraphWidth : 0.025f;
            _telegraph.SetPosition(0, start);
            _telegraph.SetPosition(1, start + (Vector3)(direction * length));
        }

        private bool IsRunRunning()
        {
            return _runController.Model != null && _runController.Model.State == RunState.Running;
        }

        private void ApplyContactHits(int hitCount)
        {
            if (_contactTarget == null || _contactTarget.Health == null || _runController.Model == null || Definition.ContactDamage <= 0)
                return;
            if (_damageAllowed != null && !_damageAllowed()) return;

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
