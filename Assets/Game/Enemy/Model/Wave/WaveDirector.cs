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
        private ContinuousSpawnTimer _spawnTimer;
        private int _nextHookIndex;
        private bool _burstConsumed;
        private readonly Random _geometryRandom;
        public WaveSpawnDecision LastDecision { get; private set; }
        public bool BurstConsumed => _burstConsumed;

        public WaveTimelineDefinition Timeline => _timeline;
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
            float runDurationSeconds)
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

            _random = new Random(timeline.Seed);
            _geometryRandom = new Random(timeline.Seed);
            _spawnTimer = new ContinuousSpawnTimer(CurrentPhase.SpawnIntervalSeconds);
        }

        // elapsedSeconds is run time (already pause-aware); deltaTime drives the
        // spawn clock. Returns how many enemies the spawner should create now.
        public int Advance(float elapsedSeconds, float deltaTime, bool isRunning, int aliveEnemies)
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

            // W-01: a burst is one whole group, never capped or retried. Last-phase
            // hold does not extend its window. Skipped windows expire without replay.
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
                LastDecision = new WaveSpawnDecision(count, count, expired);
                return count;
            }

            // A skip must not charge time spent in old phases to the new cadence.
            var phaseDelta = changed ? Math.Min(deltaTime, Elapsed - _phaseStarts[CurrentPhaseIndex]) : deltaTime;
            var due = _spawnTimer.Tick(phaseDelta, true);
            var capacity = Math.Max(0, CurrentPhase.MaxAliveEnemies - aliveEnemies);
            var allowed = Math.Min(due, capacity);
            LastDecision = new WaveSpawnDecision(due, allowed, expired);
            return allowed;
        }

        /// <summary>Uniform circle angle in radians; seeded independently of composition.
        /// This reproduces spawn decisions, not the subsequent physics simulation.</summary>
        public double SelectSpawnAngle() => _geometryRandom.NextDouble() * Math.PI * 2d;

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
    }
}
