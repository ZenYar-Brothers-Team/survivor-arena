using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Enemy
{
    // Pure timeline model: given run time it selects the current phase, decides how
    // many enemies are due and which to spawn. It owns no GameObjects — the spawner
    // executes its decisions — so transitions, pause behavior and composition are
    // testable without a scene.
    public sealed class WaveDirector
    {
        private readonly WaveTimelineDefinition _timeline;
        private readonly EnemyDefinition[][] _phaseEnemies;
        private readonly float[] _phaseStarts;
        private readonly Random _random;
        private readonly Random _movementRandom;
        private ContinuousSpawnTimer _spawnTimer;
        private int _nextHookIndex;
        private bool _burstConsumed;
        private readonly Random _geometryRandom;
        public WaveSpawnDecision LastDecision { get; private set; }
        public bool BurstConsumed => _burstConsumed;

        public WaveTimelineDefinition Timeline => _timeline;
        /// <summary>Seed actually driving composition and spawn angles: a per-run seed from the
        /// composition root (DECISION-0057) or the timeline reference seed.</summary>
        public int Seed { get; }
        public int PhaseCount => _timeline.Phases.Count;
        public int CurrentPhaseIndex { get; private set; }
        public WavePhaseDefinition CurrentPhase => _timeline.Phases[CurrentPhaseIndex];
        public float Elapsed { get; private set; }
        public WaveHookDefinition NextHook =>
            _nextHookIndex < _timeline.Hooks.Count ? _timeline.Hooks[_nextHookIndex] : null;

        public event Action<WavePhaseDefinition, int> PhaseChanged;
        public event Action<WaveHookDefinition> HookTriggered;

        public WaveDirector(
            WaveTimelineDefinition timeline,
            IReadOnlyDictionary<ContentId, EnemyDefinition> enemies,
            float runDurationSeconds,
            int? seed = null)
        {
            _timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
            if (enemies == null)
                throw new ArgumentNullException(nameof(enemies));
            NumericValidation.ValidatePositive(runDurationSeconds, nameof(runDurationSeconds));

            for (var i = 0; i < timeline.Hooks.Count; i++)
            {
                if (timeline.Hooks[i].TimeSeconds > runDurationSeconds)
                    throw new InvalidOperationException(
                        $"Wave hook '{timeline.Hooks[i].Kind}' at {timeline.Hooks[i].TimeSeconds}s is after the {runDurationSeconds}s run end.");
            }

            _phaseStarts = new float[timeline.Phases.Count];
            _phaseEnemies = new EnemyDefinition[timeline.Phases.Count][];
            var start = 0f;
            for (var phaseIndex = 0; phaseIndex < timeline.Phases.Count; phaseIndex++)
            {
                var phase = timeline.Phases[phaseIndex];
                _phaseStarts[phaseIndex] = start;
                start += phase.DurationSeconds;

                var scaled = new EnemyDefinition[phase.Composition.Count];
                for (var i = 0; i < scaled.Length; i++)
                {
                    var enemyId = phase.Composition[i].Enemy.Id;
                    if (!enemies.TryGetValue(enemyId, out var enemy))
                        throw new InvalidOperationException(
                            $"Wave phase '{phase.Id}' references unknown enemy '{enemyId}'.");
                    scaled[i] = WaveEnemyScaler.Apply(enemy, phase.Modifiers);
                }
                _phaseEnemies[phaseIndex] = scaled;
            }

            Seed = seed ?? timeline.Seed;
            _random = new Random(Seed);
            _geometryRandom = new Random(Seed);
            _movementRandom = new Random(unchecked(Seed * 486187739 + 104729));
            _spawnTimer = new ContinuousSpawnTimer(CurrentPhase.SpawnIntervalSeconds);
        }

        // elapsedSeconds is run time (already pause-aware); deltaTime drives the
        // spawn clock. Returns how many enemies the spawner should create now.
        public int Advance(float elapsedSeconds, float deltaTime, bool isRunning, int aliveEnemies,
            bool replaceAtCap = false)
        {
            NumericValidation.ValidateNonNegativeFinite(elapsedSeconds, nameof(elapsedSeconds));
            NumericValidation.ValidateNonNegativeFinite(deltaTime, nameof(deltaTime));
            if (aliveEnemies < 0)
                throw new ArgumentOutOfRangeException(nameof(aliveEnemies));
            LastDecision = default;
            if (!isRunning)
                return 0;
            if (elapsedSeconds < Elapsed)
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds), "Restart requires a new director.");

            Elapsed = elapsedSeconds;
            var changed = false;
            var expired = 0;
            while (CurrentPhaseIndex < PhaseCount - 1 && Elapsed >= _phaseStarts[CurrentPhaseIndex + 1])
            {
                if (CurrentPhase.Burst != null && !_burstConsumed)
                    expired = checked(expired + CurrentPhase.Burst.Count);
                CurrentPhaseIndex++;
                _burstConsumed = false;
                changed = true;
            }
            if (changed)
            {
                _spawnTimer = new ContinuousSpawnTimer(CurrentPhase.SpawnIntervalSeconds);
                PhaseChanged?.Invoke(CurrentPhase, CurrentPhaseIndex);
            }

            while (_nextHookIndex < _timeline.Hooks.Count && Elapsed >= _timeline.Hooks[_nextHookIndex].TimeSeconds)
            {
                var hook = _timeline.Hooks[_nextHookIndex];
                _nextHookIndex++;
                HookTriggered?.Invoke(hook);
            }

            // Bursts are attempted once and never retried. The shared ordinary-enemy
            // ceiling is a technical safeguard, so it also limits burst spawns.
            if (CurrentPhase.SpawnMode == WaveSpawnMode.Burst)
            {
                var burst = CurrentPhase.Burst;
                var local = Elapsed - _phaseStarts[CurrentPhaseIndex];
                var count = 0;
                if (!_burstConsumed && local >= burst.OffsetSeconds)
                {
                    _burstConsumed = true;
                    if (local < burst.OffsetSeconds + burst.WindowSeconds)
                        count = burst.Count;
                    else
                        expired = checked(expired + burst.Count);
                }
                var burstCapacity = replaceAtCap ? _timeline.MaxAliveEnemies :
                    Math.Max(0, _timeline.MaxAliveEnemies - aliveEnemies);
                var burstAllowed = Math.Min(count, burstCapacity);
                LastDecision = new WaveSpawnDecision(count, burstAllowed, expired);
                return burstAllowed;
            }

            // A skip must not charge time spent in old phases to the new cadence.
            var phaseDelta = changed ? Math.Min(deltaTime, Elapsed - _phaseStarts[CurrentPhaseIndex]) : deltaTime;
            var openingIntensity = _timeline.OpeningIntensity;
            if (openingIntensity != null && phaseDelta > 0f)
            {
                // Charge only the part of this tick inside the opening window at its reduced rate.
                // Splitting at the boundary preserves accumulated progress without an extra timer reset.
                var tickStart = Elapsed - phaseDelta;
                var openingSeconds = Math.Max(0f, Math.Min(Elapsed, openingIntensity.DurationSeconds) -
                    Math.Max(tickStart, 0f));
                phaseDelta -= openingSeconds * (1f - openingIntensity.RateMultiplier);
            }
            var due = _spawnTimer.Tick(phaseDelta, true);
            var capacity = replaceAtCap ? _timeline.MaxAliveEnemies :
                Math.Max(0, _timeline.MaxAliveEnemies - aliveEnemies);
            var allowed = Math.Min(due, capacity);
            LastDecision = new WaveSpawnDecision(due, allowed, expired);
            return allowed;
        }

        /// <summary>True while ordinary spawns use the screen-edge opening placement (DECISION-0057).</summary>
        public bool IsOpeningSpawnActive =>
            _timeline.OpeningSpawn != null && Elapsed < _timeline.OpeningSpawn.DurationSeconds;

        /// <summary>Uniform circle angle in radians; the geometry stream is independent of composition.</summary>
        public double SelectSpawnAngle() => _geometryRandom.NextDouble() * Math.PI * 2d;

        /// <summary>DECISION-0099: squeeze one uniform angle toward the direction opposite the
        /// living ordinary enemy centroid. u - k sin(u) covers the entire ring for k in [0,1].</summary>
        public double SelectSpawnAngle(double oppositeAngle)
        {
            if (double.IsNaN(oppositeAngle) || double.IsInfinity(oppositeAngle))
                throw new ArgumentOutOfRangeException(nameof(oppositeAngle));
            var bias = _timeline.SpawnOppositeBias;
            if (bias <= 0f) return SelectSpawnAngle();
            var uniform = _geometryRandom.NextDouble() * Math.PI * 2d - Math.PI;
            var angle = oppositeAngle + uniform - bias * Math.Sin(uniform);
            angle %= Math.PI * 2d;
            return angle < 0d ? angle + Math.PI * 2d : angle;
        }

        public EnemyDefinition SelectEnemy()
        {
            var composition = CurrentPhase.Composition;
            var totalWeight = 0f;
            for (var i = 0; i < composition.Count; i++)
                totalWeight += composition[i].Weight;

            var roll = (float)(_random.NextDouble() * totalWeight);
            var enemies = _phaseEnemies[CurrentPhaseIndex];
            for (var i = 0; i < composition.Count; i++)
            {
                roll -= composition[i].Weight;
                if (roll < 0f)
                    return enemies[i];
            }
            return enemies[enemies.Length - 1];
        }

        /// <summary>Resolves a per-life movement variant on a stream independent from composition and geometry.</summary>
        public EnemyMovementProfile SelectMovement(EnemyDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            return definition.SelectMovement((float)_movementRandom.NextDouble());
        }

        /// <summary>Seeds per-life movement state such as a personal offset or initial retarget interval.</summary>
        public int SelectMovementSeed() => _movementRandom.Next();
    }
}
