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
    public sealed class ContinuousFixtureEnemySpawner : MonoBehaviour, IEnemyLifecycleSink, IRunOutcomeContributor, IRaidDevelopmentControl
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
        private RaidProfile _raidProfile;
        private RaidScheduler _raidScheduler;
        // A player that moved less than this over the sampling window, or slower than this on average, counts as standing still.
        private const float MinHeadingDistance = 1f;
        private const float MinHeadingSpeed = .3f;
        private readonly RingFormationPlanner _raidPlanner = new RingFormationPlanner();
        private readonly LineFormationPlanner _raidLinePlanner = new LineFormationPlanner();
        private readonly List<EnemyRuntime> _raidParticipants = new List<EnemyRuntime>();
        private System.Random _raidPicker;
        private RaidTemplateKind _raidKind;
        private float _raidElapsed;
        private int _spawnCounter;
        private int _raidSpawnMark;
        private bool _raidJoined;
        private Vector2 _raidForward;
        private Vector2 _raidStartPlayerPosition;
        private Vector2 _playerHeading;
        private Vector2 _lastPlayerPosition;
        private bool _hasLastPlayerPosition;
        private readonly List<EnemyRuntime> _raidEnemies = new List<EnemyRuntime>();
        private readonly List<Vector2> _raidPositions = new List<Vector2>();
        private readonly List<Vector2> _raidOffsets = new List<Vector2>();
        private readonly List<bool> _raidAssigned = new List<bool>();
        private static readonly RaidTemplateKind[] RaidTemplates =
            (RaidTemplateKind[])System.Enum.GetValues(typeof(RaidTemplateKind));
        private float _raidCountRemaining;
        private float _raidRetargetRemaining;
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
        /// <summary>Forwarded from every living ordinary enemy: a wind-up, volley or dash started (audio cues).</summary>
        public event System.Action<EnemyActionKind, Vector2> ActionStarted;
        public event System.Action<WaveSpawnOutcome> SpawnResolved;
        public WaveSpawnOutcome LastSpawnOutcome { get; private set; }
        public EnemyLifeEvent LastLifeEvent { get; private set; }

        public int AliveCount => _aliveEnemies.Count;
        public int LastBlobBreakupCount { get; private set; }
        public WaveDirector Director => _director;
        public IReadOnlyList<RaidTemplateKind> Templates => _raidScheduler != null ? RaidTemplates : System.Array.Empty<RaidTemplateKind>();
        public int TotalEnemyCount => _aliveEnemies.Count;
        public int NearbyEnemyCount { get; private set; }
        public int TriggerEnemyCount => _raidProfile?.TriggerEnemyCount ?? 0;
        public bool RaidActive => _raidScheduler != null && _raidScheduler.IsActive;
        public RaidTemplateKind ActiveTemplate => _raidScheduler?.ActiveTemplate ?? RaidTemplateKind.Ring;
        public float RaidRemainingSeconds => _raidScheduler?.ActiveRemainingSeconds ?? 0f;
        public float NextRaidSeconds => _raidScheduler?.WaitRemainingSeconds ?? 0f;
        public bool CountdownRunning => _raidScheduler != null && !_raidScheduler.IsActive && _raidScheduler.CrowdPresent;
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
            Camera viewCamera = null,
            RaidProfile raidProfile = null)
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
            _raidProfile = raidProfile;
            _raidScheduler = raidProfile == null ? null : new RaidScheduler(raidProfile, new System.Random(director.Seed ^ 0x2A1D));
            _raidPicker = new System.Random(director.Seed ^ 0x51F7);
            _raidCountRemaining = 0f;
            _raidRetargetRemaining = 0f;
            _raidJoined = false;
            _raidParticipants.Clear();
            _playerHeading = Vector2.zero;
            _hasLastPlayerPosition = false;
            NearbyEnemyCount = 0;
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
            var arcBurst = spawnCount > 0 && _director.CurrentPhase.SpawnMode == WaveSpawnMode.Burst &&
                           _director.CurrentPhase.Burst.ArcDegrees > 0f;
            // DECISION-0146: an arc burst surrounds the player on the side away from the crowd (any side if empty).
            var arcCenter = arcBurst ? (hasCenter ? oppositeAngle : _director.SelectSpawnAngle()) : 0d;
            for (var i = 0; i < spawnCount; i++)
            {
                if (wasRunning && runController.Model.State != RunState.Running) break;
                double? forced = arcBurst ? _director.SelectBurstArcAngle(i, spawnCount, arcCenter) : (double?)null;
                if (SpawnEnemy(hasCenter, oppositeAngle, forced)) actual++;
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
            TickRaid(deltaTime, isRunning);
            return actual;
        }

        // DECISION-0155: roundup. The crowd count is sampled a few times per second (one pass over the alive list),
        // the scheduler counts down only while a crowd exists, and an active template is re-planned on its own cadence.
        private void TickRaid(float deltaTime, bool isRunning)
        {
            if (_raidScheduler == null || target == null || _viewCamera == null || !_viewCamera.orthographic) return;
            using var guard = PerfGuard.Measure("ContinuousFixtureEnemySpawner.Raid", 2f);
            if (!isRunning) return;
            TrackPlayerHeading(deltaTime);
            var wasActive = _raidScheduler.IsActive;
            _raidCountRemaining -= deltaTime;
            if (_raidCountRemaining <= 0f)
            {
                _raidCountRemaining = _raidProfile.CountCheckSeconds;
                NearbyEnemyCount = CountNearbyOrdinary();
            }
            if (_raidScheduler.Tick(deltaTime, true, NearbyEnemyCount) && !_raidScheduler.IsActive)
                TryStartRaid(RaidTemplates[_raidPicker.Next(RaidTemplates.Length)]);
            if (wasActive && !_raidScheduler.IsActive)
            {
                EndRaidForAll();
                return;
            }
            if (!_raidScheduler.IsActive) return;
            _raidElapsed += deltaTime;
            if (!_raidJoined)
            {
                if (_raidElapsed >= _raidProfile.SampleSecondsOf(_raidKind)) JoinRaid();
                return;
            }
            _raidRetargetRemaining -= deltaTime;
            if (_raidRetargetRemaining <= 0f) ReplanRaid();
        }

        public bool TryStartRaid(RaidTemplateKind kind)
        {
            if (_raidScheduler == null || target == null || _viewCamera == null || !_viewCamera.orthographic ||
                !_raidScheduler.Start(kind)) return false;
            var player = (Vector2)target.position;
            var innerSqr = _raidProfile.InnerExemptRadius * _raidProfile.InnerExemptRadius;
            _raidKind = kind;
            _raidElapsed = 0f;
            _raidJoined = false;
            _raidStartPlayerPosition = player;
            _raidSpawnMark = _spawnCounter + 1;
            _raidParticipants.Clear();
            for (var i = 0; i < _aliveEnemies.Count; i++)
            {
                var enemy = _aliveEnemies[i];
                // Enemies already inside the exempt radius at launch keep their own movement for the whole roundup.
                if (enemy == null || !enemy.IsAlive || enemy.Category != EnemyCategory.Ordinary ||
                    enemy.BlobBreakupExempt || (enemy.Position - player).sqrMagnitude <= innerSqr) continue;
                _raidParticipants.Add(enemy);
            }
            if (_raidProfile.SampleSecondsOf(kind) <= 0f) JoinRaid();
            return true;
        }

        // Fixes the facing of the formation, hands the first slots out and from then on keeps re-planning them.
        private void JoinRaid()
        {
            var player = (Vector2)target.position;
            var innerSqr = _raidProfile.InnerExemptRadius * _raidProfile.InnerExemptRadius;
            _raidEnemies.Clear();
            _raidPositions.Clear();
            var centroid = Vector2.zero;
            for (var i = 0; i < _raidParticipants.Count; i++)
            {
                var enemy = _raidParticipants[i];
                if (enemy == null || !enemy.IsAlive || (enemy.Position - player).sqrMagnitude <= innerSqr) continue;
                _raidEnemies.Add(enemy);
                _raidPositions.Add(enemy.Position);
                centroid += enemy.Position;
            }
            _raidParticipants.Clear();
            _raidJoined = true;
            _raidRetargetRemaining = _raidProfile.RetargetSeconds;
            if (_raidEnemies.Count == 0) return;
            _raidForward = ChooseForward(player, centroid / _raidEnemies.Count - player);
            PlanFormation(player, 0f);
            for (var i = 0; i < _raidEnemies.Count; i++)
                if (_raidAssigned[i])
                    _raidEnemies[i].TryStartRaid(_raidOffsets[i], _raidProfile.InnerExemptRadius,
                        RandomInCircle(_raidProfile.SlotJitter), _raidProfile.FormationSpeedBonus, _raidProfile.ArrivalSlowDistance);
        }

        private void ReplanRaid()
        {
            _raidRetargetRemaining = _raidProfile.RetargetSeconds;
            var player = (Vector2)target.position;
            var innerSqr = _raidProfile.InnerExemptRadius * _raidProfile.InnerExemptRadius;
            _raidEnemies.Clear();
            _raidPositions.Clear();
            for (var i = 0; i < _aliveEnemies.Count; i++)
            {
                var enemy = _aliveEnemies[i];
                if (enemy == null || !enemy.IsAlive || !(enemy.RaidActive || CanJoinLate(enemy, player, innerSqr))) continue;
                _raidEnemies.Add(enemy);
                _raidPositions.Add(enemy.Position);
            }
            if (_raidEnemies.Count == 0) return;
            var sample = _raidProfile.SampleSecondsOf(_raidKind);
            var formation = _raidProfile.DurationOf(_raidKind) - sample;
            PlanFormation(player, Mathf.Clamp01((_raidElapsed - sample) / formation));
            for (var i = 0; i < _raidEnemies.Count; i++)
            {
                if (!_raidAssigned[i]) continue;
                if (_raidEnemies[i].RaidActive) _raidEnemies[i].SetRaidSlot(_raidOffsets[i]);
                else
                    _raidEnemies[i].TryStartRaid(_raidOffsets[i], _raidProfile.InnerExemptRadius,
                        RandomInCircle(_raidProfile.SlotJitter), _raidProfile.FormationSpeedBonus, _raidProfile.ArrivalSlowDistance);
            }
        }

        // Every template takes in enemies that spawned after the roundup began; they must still be outside the exempt
        // radius and must not have left this roundup by reaching the player.
        private bool CanJoinLate(EnemyRuntime enemy, Vector2 player, float innerSqr) =>
            enemy.SpawnOrder >= _raidSpawnMark && !enemy.RaidExited &&
            enemy.Category == EnemyCategory.Ordinary && !enemy.BlobBreakupExempt &&
            (enemy.Position - player).sqrMagnitude > innerSqr;

        // progress runs 0..1 over the formation time; only the contracting templates read it.
        private void PlanFormation(Vector2 player, float progress)
        {
            var width = 2f * _viewCamera.orthographicSize * _viewCamera.aspect;
            var spacing = _raidProfile.SlotSpacing;
            var max = _raidProfile.MaxParticipants;
            switch (_raidKind)
            {
                case RaidTemplateKind.Ring:
                    _raidPlanner.Plan(player, _raidPositions, _raidProfile.Ring.RadiusScreenWidths * width, spacing, max,
                        _raidOffsets, _raidAssigned);
                    break;
                case RaidTemplateKind.Contraction:
                    var c = _raidProfile.Contraction;
                    _raidPlanner.Plan(player, _raidPositions,
                        Mathf.Lerp(c.StartRadiusScreenWidths, c.EndRadiusScreenWidths, progress) * width, spacing, max,
                        _raidOffsets, _raidAssigned);
                    break;
                case RaidTemplateKind.Wall:
                    var w = _raidProfile.Wall;
                    _raidLinePlanner.PlanWall(player, _raidPositions, _raidForward, w.DistanceScreenWidths * width,
                        w.WidthScreenWidths * width, spacing, max, _raidOffsets, _raidAssigned);
                    break;
                case RaidTemplateKind.Pincer:
                    var p = _raidProfile.Pincer;
                    _raidLinePlanner.PlanPincer(player, _raidPositions, _raidForward, p.LengthScreenWidths * width,
                        Mathf.Lerp(p.StartGapScreenWidths, p.EndGapScreenWidths, progress) * width, spacing, max,
                        _raidOffsets, _raidAssigned);
                    break;
            }
        }

        // Wall: the player's heading over the sampling window. Pincer: the smoothed heading. A player standing still falls back to the direction of the crowd.
        private Vector2 ChooseForward(Vector2 player, Vector2 towardCrowd)
        {
            var crowd = towardCrowd.sqrMagnitude > .0001f ? towardCrowd.normalized : Vector2.right;
            switch (_raidKind)
            {
                case RaidTemplateKind.Wall:
                    var moved = player - _raidStartPlayerPosition;
                    return moved.magnitude >= MinHeadingDistance ? moved.normalized : crowd;
                default:
                    return _playerHeading.magnitude >= MinHeadingSpeed ? _playerHeading.normalized : crowd;
            }
        }

        // Exponentially smoothed player velocity; the time constant is the wall's sampling window.
        private void TrackPlayerHeading(float deltaTime)
        {
            var player = (Vector2)target.position;
            if (_hasLastPlayerPosition && deltaTime > 0f)
            {
                var velocity = (player - _lastPlayerPosition) / deltaTime;
                var blend = 1f - Mathf.Exp(-deltaTime / _raidProfile.Wall.SampleSeconds);
                _playerHeading = Vector2.Lerp(_playerHeading, velocity, blend);
            }
            _lastPlayerPosition = player;
            _hasLastPlayerPosition = true;
        }

        private Vector2 RandomInCircle(float radius)
        {
            if (radius <= 0f) return Vector2.zero;
            var angle = (float)(_raidPicker.NextDouble() * Mathf.PI * 2d);
            var length = radius * Mathf.Sqrt((float)_raidPicker.NextDouble());
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * length;
        }

        private void EndRaidForAll()
        {
            _raidParticipants.Clear();
            _raidJoined = false;
            for (var i = 0; i < _aliveEnemies.Count; i++)
                _aliveEnemies[i]?.EndRaid();
        }

        private int CountNearbyOrdinary()
        {
            var player = (Vector2)target.position;
            var radius = _raidProfile.TriggerRadiusScreenWidths * 2f * _viewCamera.orthographicSize * _viewCamera.aspect;
            var radiusSqr = radius * radius;
            var count = 0;
            for (var i = 0; i < _aliveEnemies.Count; i++)
            {
                var enemy = _aliveEnemies[i];
                if (enemy != null && enemy.IsAlive && enemy.Category == EnemyCategory.Ordinary &&
                    (enemy.Position - player).sqrMagnitude <= radiusSqr) count++;
            }
            return count;
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
                _blobEligible.Add(!enemy.BlobBreakupActive && !enemy.BlobBreakupExempt && !enemy.RaidActive && settings.Includes(enemy.ContentId));
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

        private bool SpawnEnemy(bool hasCenter, double oppositeAngle, double? forcedAngle = null)
        {
            if (target == null || runController == null)
                return false;

            var angle = forcedAngle ?? (hasCenter ? _director.SelectSpawnAngle(oppositeAngle) : _director.SelectSpawnAngle());
            var direction = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle));

            var definition = _director.SelectEnemy();
            var movement = _director.SelectMovement(definition);
            // DECISION-0146: an arc burst walks straight at the player, so its formation is not shuffled by
            // sidesteps/arcs; dash enemies keep their own straight-line dash.
            if (forcedAngle.HasValue && definition.Movement.Kind != EnemyMovementKind.TelegraphedDash)
                movement = EnemyMovementProfile.Seek;
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
            enemy.BlobBreakupExempt = forcedAngle.HasValue;
            enemy.SpawnOrder = ++_spawnCounter;
            enemy.Despawned += HandleEnemyDespawned;
            enemy.CombatResolved += ForwardCombat;
            enemy.ActionStarted += ForwardAction;
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
        private void ForwardAction(EnemyActionKind kind, Vector2 position) => ActionStarted?.Invoke(kind, position);

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
            enemy.ActionStarted -= ForwardAction;
            enemy.EraseSilently();
        }

        private void HandleEnemyDespawned(EnemyRuntime enemy)
        {
            enemy.Despawned -= HandleEnemyDespawned;
            enemy.CombatResolved -= ForwardCombat;
            enemy.ActionStarted -= ForwardAction;
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
                enemy.ActionStarted -= ForwardAction;
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
            _raidProfile = null;
            _raidScheduler = null;
            _raidPicker = null;
            _raidParticipants.Clear();
            _raidEnemies.Clear();
            _raidPositions.Clear();
            _raidOffsets.Clear();
            _raidAssigned.Clear();
            NearbyEnemyCount = 0;
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
            ActionStarted = null;
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
