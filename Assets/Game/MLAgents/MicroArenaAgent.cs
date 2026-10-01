using System;
using Game.Movement;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.MLAgents
{
    /// <summary>
    /// Self-contained seek-XP/avoid-threat diagnostic agent. Rebuilt from the research snapshot
    /// (docs/research/2026-10-01-ml-agents-training-snapshot.md); not a byte-identical reproduction of the
    /// original uncommitted experiment, since that code never left its local worktree. Geometry, reward and
    /// outcome rules match the documented formulas; EnvironmentParameters drive curriculum stages from Python.
    /// </summary>
    [RequireComponent(typeof(BehaviorParameters))]
    [RequireComponent(typeof(DecisionRequester))]
    public sealed class MicroArenaAgent : Agent
    {
        [SerializeField]
        private Transform threatVisual;

        [SerializeField]
        private Transform xpVisual;

        private const int MaxSpawnAttempts = 20;

        /// <summary>Wires the threat/XP visuals this agent moves each step; the agent's own transform is the player visual.</summary>
        public void Configure(Transform threatTransform, Transform xpTransform)
        {
            threatVisual = threatTransform != null ? threatTransform : throw new ArgumentNullException(nameof(threatTransform));
            xpVisual = xpTransform != null ? xpTransform : throw new ArgumentNullException(nameof(xpTransform));
        }

        private MicroArenaConfig _config;
        private int _episodeCount;
        private System.Random _rng;
        private Vector2 _playerPosition;
        private Vector2 _threatPosition;
        private Vector2 _threatVelocity;
        private Vector2 _xpPosition;
        private float _threatBaseY;
        private float _threatPhase;
        private float _elapsedSeconds;
        private readonly float[] _observationBuffer = new float[MicroArenaObservationEncoder.ObservationCount];

        public override void Initialize()
        {
            _config = ResolveConfig();
        }

        public override void OnEpisodeBegin()
        {
            _episodeCount++;
            _rng = new System.Random(_config.Seed + _episodeCount - 1);
            _elapsedSeconds = 0f;

            SpawnGeometry();
            ApplyVisualPositions();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            MicroArenaObservationEncoder.Encode(
                _observationBuffer, _playerPosition, _xpPosition, _threatPosition, _threatVelocity, _config);
            sensor.AddObservation(_observationBuffer);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            var deltaTime = Time.fixedDeltaTime;
            var rawAction = new Vector2(actions.ContinuousActions[0], actions.ContinuousActions[1]);
            var velocity = MovementVelocityCalculator.Calculate(rawAction, _config.PlayerSpeed, isRunning: true);

            var previousDistanceToXp = Vector2.Distance(_playerPosition, _xpPosition);
            _playerPosition = ClampToArena(_playerPosition + (velocity * deltaTime));
            AdvanceThreat(deltaTime);
            _elapsedSeconds += deltaTime;
            ApplyVisualPositions();

            var currentDistanceToXp = Vector2.Distance(_playerPosition, _xpPosition);
            AddReward(MicroArenaRewardCalculator.StepReward(previousDistanceToXp, currentDistanceToXp));

            var outcome = MicroArenaRewardCalculator.DetermineOutcome(
                _playerPosition, _xpPosition, _threatPosition, _config, _elapsedSeconds);
            if (outcome == MicroArenaOutcome.None)
                return;

            AddReward(MicroArenaRewardCalculator.TerminalReward(outcome));
            EndEpisode();
        }

        /// <summary>WASD/arrow manual control for a quick sanity check in Play mode without a Python trainer attached.</summary>
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuous = actionsOut.ContinuousActions;
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                continuous[0] = 0f;
                continuous[1] = 0f;
                return;
            }

            var x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f) -
                    (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
            var y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f) -
                    (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
            continuous[0] = x;
            continuous[1] = y;
        }

        private MicroArenaConfig ResolveConfig()
        {
            var parameters = Academy.Instance.EnvironmentParameters;
            var fallback = MicroArenaConfig.Default(seed: 1234);
            return new MicroArenaConfig(
                arenaHalfSize: parameters.GetWithDefault("arena_half_size", fallback.ArenaHalfSize),
                playerRadius: parameters.GetWithDefault("player_radius", fallback.PlayerRadius),
                threatRadius: parameters.GetWithDefault("threat_radius", fallback.ThreatRadius),
                xpRadius: parameters.GetWithDefault("xp_radius", fallback.XpRadius),
                playerSpeed: parameters.GetWithDefault("player_speed", fallback.PlayerSpeed),
                startXOffset: parameters.GetWithDefault("start_x_offset", fallback.StartXOffset),
                startJitter: parameters.GetWithDefault("start_jitter", fallback.StartJitter),
                threatAmplitude: parameters.GetWithDefault("threat_amplitude", fallback.ThreatAmplitude),
                threatFrequencyHz: parameters.GetWithDefault("threat_frequency_hz", fallback.ThreatFrequencyHz),
                timeLimitSeconds: parameters.GetWithDefault("time_limit_seconds", fallback.TimeLimitSeconds),
                seed: (int)parameters.GetWithDefault("seed", fallback.Seed));
        }

        private void SpawnGeometry()
        {
            var side = NextDouble() < 0.5 ? -1f : 1f;
            var minGap = _config.PlayerRadius + _config.ThreatRadius + 0.05f;
            var minXpGap = _config.XpRadius + _config.ThreatRadius + 0.05f;

            var placed = false;
            for (var attempt = 0; attempt < MaxSpawnAttempts && !placed; attempt++)
            {
                var jitterX = NextJitter();
                var jitterY = NextJitter();
                var player = new Vector2((-side * _config.StartXOffset) + jitterX, jitterY);
                var xp = new Vector2((side * _config.StartXOffset) + jitterX, jitterY);
                var threat = Vector2.zero;

                if (Vector2.Distance(player, threat) < minGap || Vector2.Distance(xp, threat) < minXpGap)
                    continue;

                _playerPosition = ClampToArena(player);
                _xpPosition = ClampToArena(xp);
                _threatPosition = threat;
                placed = true;
            }

            if (!placed)
            {
                // Defensive fallback: every jittered attempt above was rejected; fall back to the zero-jitter layout.
                _playerPosition = ClampToArena(new Vector2(-side * _config.StartXOffset, 0f));
                _xpPosition = ClampToArena(new Vector2(side * _config.StartXOffset, 0f));
                _threatPosition = Vector2.zero;
            }

            _threatBaseY = _threatPosition.y;
            _threatPhase = (float)(NextDouble() * Math.PI * 2.0);
            _threatVelocity = Vector2.zero;
        }

        private void AdvanceThreat(float deltaTime)
        {
            if (_config.ThreatAmplitude <= 0f)
            {
                _threatVelocity = Vector2.zero;
                return;
            }

            var omega = 2f * Mathf.PI * _config.ThreatFrequencyHz;
            var angle = _threatPhase + (omega * _elapsedSeconds);
            _threatPosition = new Vector2(_threatPosition.x, _threatBaseY + (_config.ThreatAmplitude * Mathf.Sin(angle)));
            _threatVelocity = new Vector2(0f, omega * _config.ThreatAmplitude * Mathf.Cos(angle));
        }

        private Vector2 ClampToArena(Vector2 position) => new Vector2(
            Mathf.Clamp(position.x, -_config.ArenaHalfSize, _config.ArenaHalfSize),
            Mathf.Clamp(position.y, -_config.ArenaHalfSize, _config.ArenaHalfSize));

        private float NextJitter() => _config.StartJitter <= 0f ? 0f : (float)((NextDouble() * 2.0 - 1.0) * _config.StartJitter);

        private double NextDouble() => _rng.NextDouble();

        private void ApplyVisualPositions()
        {
            transform.localPosition = new Vector3(_playerPosition.x, _playerPosition.y, transform.localPosition.z);
            if (threatVisual != null)
                threatVisual.localPosition = new Vector3(_threatPosition.x, _threatPosition.y, threatVisual.localPosition.z);
            if (xpVisual != null)
                xpVisual.localPosition = new Vector3(_xpPosition.x, _xpPosition.y, xpVisual.localPosition.z);
        }
    }
}
