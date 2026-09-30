using System.Collections.Generic;
using Game.Content;
using Game.Diagnostics;
using Game.Pooling;
using Game.Run;
using Game.Presentation;
using UnityEngine;

namespace Game.Enemy
{
    // Executes the WaveDirector's spawn decisions: owns the enemy/projectile pools and
    // the alive list, while the director owns timing, composition and phase state.
    [DisallowMultipleComponent]
    public sealed class ContinuousFixtureEnemySpawner : MonoBehaviour, IEnemyLifecycleSink, IRunOutcomeContributor
    {
        // Spawning instantiates/rents and initializes several enemies in one tick at
        // high phase rates; warn if that starts costing real time (DECISION-0008).
        private const float TickWarningMilliseconds = 2f;

        [SerializeField]
        private RunController runController;

        [SerializeField]
        private Transform target;

        private readonly List<EnemyRuntime> _aliveEnemies = new List<EnemyRuntime>();
        private readonly List<EnemyRuntime> _blobEnemies = new List<EnemyRuntime>();
        private readonly List<Vector2> _blobPositions = new List<Vector2>();
        private readonly List<bool> _blobEligible = new List<bool>();
        private readonly List<int> _blobSelected = new List<int>();
        private readonly List<Vector2> _blobWaypoints = new List<Vector2>();
        private readonly List<float> _blobDelays = new List<float>();
        private readonly BlobBreakupPlanner.Cell[] _blobGrid = new BlobBreakupPlanner.Cell[BlobBreakupPlanner.GridLength];
        private System.Random _blobRandom;
        private float _nextBlobCheckTime;
        private int _lastBlobPhaseIndex;
        private WaveDirector _director;
        private IReadOnlyDictionary<ContentId, Sprite> _visuals;
        private IReadOnlyDictionary<ContentId, SpriteMotionProfile> _motions;
        private IReadOnlyDictionary<ContentId, SpriteContactProfile> _contacts;
        private EnemyDeathPresentationProfile _deathPresentation;
        private GroundShadowPresentationProfile _groundShadowPresentation;
        private ContentRegistry _contentRegistry;
        private Camera _viewCamera;
        private GameObjectPool<EnemyRuntime> _pool;
        private GameObjectPool<EnemyProjectileRuntime> _projectilePool;
        private bool _initialized;
        private IEnemyLifecycleSink _lifecycleSink;
        private RunModel _outcomeOwner;
        private int _kills;

        public string Key => "ordinary-enemy-kills";
        /// <summary>Feature-owned facts; optional observers do not participate in spawn/reward decisions.</summary>
        public event System.Action<EnemyLifeEvent> LifeEvent;
        public event System.Action<Game.Combat.CombatResult> CombatResolved;
        public event System.Action<WaveSpawnOutcome> SpawnResolved;
        public WaveSpawnOutcome LastSpawnOutcome { get; private set; }
        public EnemyLifeEvent LastLifeEvent { get; private set; }

        public int AliveCount => _aliveEnemies.Count;
        public int LastBlobBreakupCount { get; private set; }
        public WaveDirector Director => _director;
        public string DevelopmentObservation
        {
            get
            {
                if (_aliveEnemies.Count > 0 && _aliveEnemies[0] != null)
                {
                    var enemy = _aliveEnemies[0];
                    return $"{enemy.Definition.Id} · life {enemy.LifeId:N} · {enemy.CurrentMovement.Kind}/{enemy.MovementPhase} · {enemy.AttackPattern?.ToString() ?? "Melee"} · {enemy.AttackPhase?.ToString() ?? "No attack"} {enemy.AttackPhaseRemaining:0.##} s · burst left {enemy.BurstShotsRemaining} · shot {enemy.LastProjectileSource.ContentId?.ToString() ?? "none"}/{enemy.LastProjectileSource.Owner.LifeId:N} · slow sources {enemy.Controls.SlowSourceCount} · movement x{enemy.Controls.MovementMultiplier:0.##} · knockback {enemy.Controls.KnockbackRemaining:0.##} s";
                }
                if (LastLifeEvent != null)
                    return $"{LastLifeEvent.ContentId} · life {LastLifeEvent.LifeId:N} · {LastLifeEvent.Reason}";
                return _initialized ? "No live enemies" : "Enemy fixtures unavailable";
            }
        }

        private void Start()
        {
            if (_initialized)
                return;
            Debug.LogError("Enemy spawner must be initialized by the gameplay composition root.", this);
            enabled = false;
        }

        public void Initialize(WaveDirector director, IReadOnlyDictionary<ContentId, Sprite> visuals = null,
            IEnemyLifecycleSink lifecycleSink = null,
            IReadOnlyDictionary<ContentId, SpriteMotionProfile> motions = null,
            IReadOnlyDictionary<ContentId, SpriteContactProfile> contacts = null,
            EnemyDeathPresentationProfile deathPresentation = null,
            GroundShadowPresentationProfile groundShadowPresentation = null,
            ContentRegistry contentRegistry = null,
            Camera viewCamera = null)
        {
            if (_initialized)
                throw new System.InvalidOperationException("Enemy spawner is already initialized.");

            _director = director ?? throw new System.ArgumentNullException(nameof(director));
            _lifecycleSink = lifecycleSink;
            _kills = 0;
            LastLifeEvent = null;
            LastSpawnOutcome = default;
            LastBlobBreakupCount = 0;
            _blobRandom = new System.Random(director.Seed ^ 0x4B10B);
            _nextBlobCheckTime = director.CurrentPhase.BlobBreakup?.CheckIntervalSeconds ?? 0f;
            _lastBlobPhaseIndex = director.CurrentPhaseIndex;
            _visuals = visuals;
            _motions = motions;
            _contacts = contacts;
            _deathPresentation = deathPresentation;
            _groundShadowPresentation = groundShadowPresentation;
            _contentRegistry = contentRegistry;
            _viewCamera = viewCamera;
            _pool ??= new GameObjectPool<EnemyRuntime>(EnemyFactory.CreateInstance, transform);
            _projectilePool ??= new GameObjectPool<EnemyProjectileRuntime>(EnemyProjectileFactory.CreateInstance, transform);
            _outcomeOwner = runController != null ? runController.Model : null;
            _outcomeOwner?.RegisterOutcomeContributor(this);
            _initialized = true;
        }

        private void Update()
        {
            var hasRun = runController != null && runController.Model != null;
            var isRunning = hasRun && runController.Model.State == RunState.Running;
            Tick(hasRun ? runController.Model.Elapsed : 0f, Time.deltaTime, isRunning);
        }

        // Spawns whatever the director says is due; returns how many enemies were created.
        public int Tick(float elapsedSeconds, float deltaTime, bool isRunning)
        {
            if (!_initialized)
                return 0;

            using var guard = PerfGuard.Measure("ContinuousFixtureEnemySpawner.Tick", TickWarningMilliseconds);
            var wasRunning = runController != null && runController.Model != null && runController.Model.State == RunState.Running;
            var spawnCount = _director.Advance(elapsedSeconds, deltaTime, isRunning, _aliveEnemies.Count,
                replaceAtCap: true);
            if (_lastBlobPhaseIndex != _director.CurrentPhaseIndex)
            {
                _lastBlobPhaseIndex = _director.CurrentPhaseIndex;
                if (_director.CurrentPhase.BlobBreakup == null)
                {
                    for (var i = 0; i < _aliveEnemies.Count; i++)
                        _aliveEnemies[i]?.CancelBlobBreakup();
                    _nextBlobCheckTime = 0f;
                }
                else if (_nextBlobCheckTime <= 0f)
                    _nextBlobCheckTime = elapsedSeconds + _director.CurrentPhase.BlobBreakup.CheckIntervalSeconds;
            }
            if (spawnCount > 0 && target != null)
                while (_aliveEnemies.Count + spawnCount > _director.Timeline.MaxAliveEnemies && _aliveEnemies.Count > 0)
                    EraseFarthestOrdinary();
            double oppositeAngle = 0d;
            var hasCenter = spawnCount > 0 && TryGetOppositeMassAngle(out oppositeAngle);
            var actual = 0;
            for (var i = 0; i < spawnCount; i++)
            {
                if (wasRunning && runController.Model.State != RunState.Running) break;
                if (SpawnEnemy(hasCenter, oppositeAngle)) actual++;
            }
            var decision = _director.LastDecision;
            if (decision.Requested > 0 || decision.Expired > 0)
            {
                LastSpawnOutcome = new WaveSpawnOutcome(_director.CurrentPhase.Id, _director.Elapsed,
                    _director.CurrentPhase.SpawnMode, decision, actual, AliveCount, _director.Timeline.MaxAliveEnemies);
                SpawnResolved?.Invoke(LastSpawnOutcome);
            }
            if (isRunning && target != null && _director.CurrentPhase.BlobBreakup != null &&
                elapsedSeconds >= _nextBlobCheckTime)
            {
                var settings = _director.CurrentPhase.BlobBreakup;
                _nextBlobCheckTime = elapsedSeconds + settings.CheckIntervalSeconds;
                TryBreakBlob(settings);
            }
            return actual;
        }

        private void TryBreakBlob(BlobBreakupDefinition settings)
        {
            using var guard = PerfGuard.Measure("ContinuousFixtureEnemySpawner.BlobBreakup", 2f);
            _blobEnemies.Clear();
            _blobPositions.Clear();
            _blobEligible.Clear();
            for (var i = 0; i < _aliveEnemies.Count; i++)
            {
                var enemy = _aliveEnemies[i];
                if (enemy == null || !enemy.IsAlive || enemy.Category != EnemyCategory.Ordinary) continue;
                _blobEnemies.Add(enemy);
                _blobPositions.Add(enemy.Position);
                _blobEligible.Add(!enemy.BlobBreakupActive && settings.Includes(enemy.ContentId));
            }
            LastBlobBreakupCount = 0;
            BlobBreakupPlanner.Plan(_blobPositions, target.position, settings, _blobRandom,
                _blobGrid, _blobSelected, _blobWaypoints, _blobDelays, _blobEligible);
            for (var i = 0; i < _blobSelected.Count; i++)
                if (_blobEnemies[_blobSelected[i]].TryStartBlobBreakup(
                    _blobWaypoints[i], _blobDelays[i], settings.ManeuverSeconds))
                    LastBlobBreakupCount++;
        }

        // At most 16 position reads per spawning tick, shared by a whole burst.
        // Precision is deliberately low; the bias is a broad visual correction, not pathfinding.
        private bool TryGetOppositeMassAngle(out double angle)
        {
            angle = 0d;
            var count = _aliveEnemies.Count;
            if (count == 0 || target == null) return false;
            var stride = Mathf.Max(1, (count + 15) / 16);
            var player = (Vector2)target.position;
            var sum = Vector2.zero;
            for (var i = 0; i < count; i += stride)
            {
                var enemy = _aliveEnemies[i];
                if (enemy != null && enemy.IsAlive) sum += enemy.Position - player;
            }
            if (sum.sqrMagnitude <= 0.0001f) return false;
            angle = System.Math.Atan2(-sum.y, -sum.x);
            return true;
        }

        private bool SpawnEnemy(bool hasCenter, double oppositeAngle)
        {
            if (target == null || runController == null)
                return false;

            var angle = hasCenter ? _director.SelectSpawnAngle(oppositeAngle) : _director.SelectSpawnAngle();
            var direction = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle));

            var definition = _director.SelectEnemy();
            var movement = _director.SelectMovement(definition);
            var movementSeed = _director.SelectMovementSeed();
            var visual = _visuals != null && _visuals.TryGetValue(definition.Id, out var sprite) ? sprite : null;
            var motion = _motions != null && _motions.TryGetValue(definition.Id, out var profile) ? profile : null;
            var contact = _contacts != null && _contacts.TryGetValue(definition.Id, out var fitted) ? fitted : null;
            var spawnPosition = (Vector2)target.position + SpawnOffset(angle, direction);
            var enemy = EnemyFactory.Spawn(
                definition,
                spawnPosition,
                target,
                runController,
                transform,
                visual,
                _pool,
                _projectilePool,
                this,
                motionProfile: motion,
                contact: contact,
                deathPresentation: _deathPresentation,
                groundShadowPresentation: _groundShadowPresentation,
                contentRegistry: _contentRegistry,
                movement: movement,
                movementSeed: movementSeed);
            enemy.Despawned += HandleEnemyDespawned;
            enemy.CombatResolved += ForwardCombat;
            _aliveEnemies.Add(enemy);
            return true;
        }

        // DECISION-0057: during the opening window enemies start just outside the camera view
        // (the camera follows the target); otherwise, or without an orthographic camera, on the radius.
        private Vector2 SpawnOffset(double angle, Vector2 direction)
        {
            var opening = _director.Timeline.OpeningSpawn;
            if (!_director.IsOpeningSpawnActive || _viewCamera == null || !_viewCamera.orthographic)
                return direction * _director.Timeline.SpawnRadius;

            var halfHeight = _viewCamera.orthographicSize;
            WaveScreenEdgePlacement.Offset(angle, halfHeight * _viewCamera.aspect, halfHeight, opening.ScreenMargin,
                out var x, out var y);
            return new Vector2(x, y);
        }

        private void ForwardCombat(Game.Combat.CombatResult result) => CombatResolved?.Invoke(result);

        private void EraseFarthestOrdinary()
        {
            var player = (Vector2)target.position;
            var farthestIndex = 0;
            var farthestDistance = float.NegativeInfinity;
            for (var i = 0; i < _aliveEnemies.Count; i++)
            {
                var distance = (_aliveEnemies[i].Position - player).sqrMagnitude;
                if (distance <= farthestDistance) continue;
                farthestDistance = distance;
                farthestIndex = i;
            }
            var enemy = _aliveEnemies[farthestIndex];
            _aliveEnemies.RemoveAt(farthestIndex);
            enemy.Despawned -= HandleEnemyDespawned;
            enemy.CombatResolved -= ForwardCombat;
            enemy.EraseSilently();
        }

        private void HandleEnemyDespawned(EnemyRuntime enemy)
        {
            enemy.Despawned -= HandleEnemyDespawned;
            enemy.CombatResolved -= ForwardCombat;
            _aliveEnemies.Remove(enemy);
        }

        public void OnEnemyLifeEvent(EnemyLifeEvent snapshot)
        {
            LastLifeEvent = snapshot;
            if (snapshot.Kind == EnemyLifeEventKind.Died && snapshot.Category == EnemyCategory.Ordinary) _kills++;
            _lifecycleSink?.OnEnemyLifeEvent(snapshot);
            LifeEvent?.Invoke(snapshot);
        }

        public RunOutcomeContribution Capture() => new RunOutcomeContribution(kills: _kills);

        public void Shutdown()
        {
            if (!_initialized)
                return;

            using var guard = PerfGuard.Measure("ContinuousFixtureEnemySpawner.Shutdown", TickWarningMilliseconds);

            while (_aliveEnemies.Count > 0)
            {
                var lastIndex = _aliveEnemies.Count - 1;
                var enemy = _aliveEnemies[lastIndex];
                _aliveEnemies.RemoveAt(lastIndex);
                if (enemy == null)
                    continue;

                enemy.Despawned -= HandleEnemyDespawned;
                enemy.CombatResolved -= ForwardCombat;
                enemy.Despawn();
            }
            _director = null;
            _blobEnemies.Clear();
            _blobPositions.Clear();
            _blobEligible.Clear();
            _blobSelected.Clear();
            _blobWaypoints.Clear();
            _blobDelays.Clear();
            _blobRandom = null;
            _nextBlobCheckTime = 0f;
            LastBlobBreakupCount = 0;
            _visuals = null;
            _motions = null;
            _contacts = null;
            _deathPresentation = null;
            _contentRegistry = null;
            _viewCamera = null;
            _outcomeOwner?.UnregisterOutcomeContributor(this);
            _outcomeOwner = null;
            _lifecycleSink = null;
            LifeEvent = null;
            CombatResolved = null;
            SpawnResolved = null;
            LastSpawnOutcome = default;
            _initialized = false;
        }

        private void OnDestroy()
        {
            Shutdown();
        }
    }
}
