using System;
using System.Collections.Generic;
using Game.Content;
using Game.Diagnostics;
using Game.Enemy;
using Game.Movement;
using Game.Pooling;
using Game.Run;
using UnityEngine;

namespace Game.ActiveSkill
{
    public sealed class SceneActiveSkillEffectExecutor : IActiveSkillEffectExecutor, IDisposable
    {
        // Scans every mine against every alive enemy; fine at current fixture
        // scale, but warns if it starts costing real time once counts grow.
        private const float TickMinesWarningMilliseconds = 1f;

        // Whole-tick budget: scheduled effects (beams/chains/areas scan every alive enemy
        // or run physics overlaps) plus mines. Generous for fixture-scale content.
        private const float TickWarningMilliseconds = 2f;

        private readonly RunController _runController;
        private readonly IActiveSkillProjectileLauncher _projectileLauncher;
        private readonly List<ScheduledEffect> _scheduled = new List<ScheduledEffect>();
        private readonly List<MineState> _mines = new List<MineState>();
        private readonly List<EnemyRuntime> _enemyBuffer = new List<EnemyRuntime>();
        private readonly HashSet<IEnemyDamageReceiver> _chainHitBuffer = new HashSet<IEnemyDamageReceiver>();
        private readonly Transform _minePoolRoot;
        private readonly GameObjectPool<SpriteRenderer> _minePool;

        public int ScheduledCount => _scheduled.Count;
        public int ActiveMineCount => _mines.Count;

        public SceneActiveSkillEffectExecutor(
            RunController runController,
            IActiveSkillProjectileLauncher projectileLauncher = null)
        {
            _runController = runController != null
                ? runController
                : throw new ArgumentNullException(nameof(runController));
            _projectileLauncher = projectileLauncher ?? new SceneProjectileLauncher(runController);
            _minePoolRoot = new GameObject("Mine Pool").transform;
            _minePool = new GameObjectPool<SpriteRenderer>(CreateMineMarker, _minePoolRoot);
        }

        public void Schedule(ActiveSkillActivation activation)
        {
            var waves = activation.LevelDefinition.Waves;
            for (var waveIndex = 0; waveIndex < waves.Count; waveIndex++)
            {
                var wave = waves[waveIndex];
                for (var effectIndex = 0; effectIndex < wave.Effects.Count; effectIndex++)
                {
                    var effect = wave.Effects[effectIndex];
                    var repeatCount = 1;
                    var repeatInterval = 0f;
                    if (effect is BeamEffect beam)
                    {
                        repeatCount = Math.Max(1, Mathf.CeilToInt(beam.DurationSeconds / beam.TickIntervalSeconds));
                        repeatInterval = beam.TickIntervalSeconds;
                    }
                    else if (effect is OrbitEffect orbit)
                    {
                        repeatCount = Math.Max(1, Mathf.CeilToInt(orbit.DurationSeconds / orbit.HitCooldownSeconds));
                        repeatInterval = orbit.HitCooldownSeconds;
                    }

                    for (var tickIndex = 0; tickIndex < repeatCount; tickIndex++)
                    {
                        _scheduled.Add(new ScheduledEffect(
                            activation,
                            wave,
                            effect,
                            wave.DelaySeconds + repeatInterval * tickIndex,
                            tickIndex));
                    }
                }
            }
        }

        public void Tick(float deltaTime, bool isRunning)
        {
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            if (!isRunning)
                return;

            using var _ = PerfGuard.Measure("SceneActiveSkillEffectExecutor.Tick", TickWarningMilliseconds);
            for (var i = _scheduled.Count - 1; i >= 0; i--)
            {
                var scheduled = _scheduled[i];
                scheduled.RemainingDelay -= deltaTime;
                if (scheduled.RemainingDelay > 0f)
                    continue;

                _scheduled.RemoveAt(i);
                Execute(scheduled);
            }

            TickMines(deltaTime);
        }

        private void Execute(ScheduledEffect scheduled)
        {
            switch (scheduled.Effect)
            {
                case ProjectileBurstEffect projectile:
                    ExecuteProjectileBurst(scheduled, projectile);
                    break;
                case BeamEffect beam:
                    ExecuteBeamTick(scheduled, beam);
                    break;
                case OrbitEffect orbit:
                    ExecuteOrbitTick(scheduled, orbit);
                    break;
                case BoomerangEffect boomerang:
                    ExecuteBoomerang(scheduled, boomerang);
                    break;
                case ChainEffect chain:
                    ExecuteChain(scheduled, chain);
                    break;
                case AreaEffect area:
                    ExecuteArea(scheduled, area);
                    break;
                case MineEffect mine:
                    PlaceMine(scheduled, mine);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported active-skill effect '{scheduled.Effect.GetType().Name}'.");
            }
        }

        private void ExecuteProjectileBurst(ScheduledEffect scheduled, ProjectileBurstEffect effect)
        {
            var directions = ProjectileDirectionGenerator.Create(
                effect.Layout,
                effect.ProjectileCount,
                scheduled.Activation.AimDirection,
                effect.SpreadDegrees,
                scheduled.Wave.RotationDegrees);
            var damage = CreateDamage(scheduled, effect.DamageMultiplier);
            for (var i = 0; i < directions.Length; i++)
            {
                _projectileLauncher.Launch(new ActiveSkillProjectile(
                    scheduled.Activation.Origin,
                    directions[i],
                    effect.Speed,
                    effect.LifetimeSeconds,
                    effect.CollisionRadius,
                    effect.ImpactAreaRadius,
                    damage,
                    effect.PierceCount));
            }
        }

        private void ExecuteBoomerang(ScheduledEffect scheduled, BoomerangEffect effect)
        {
            var directions = ProjectileDirectionGenerator.Create(
                ProjectileLayout.Fan,
                effect.ProjectileCount,
                scheduled.Activation.AimDirection,
                effect.SpreadDegrees,
                scheduled.Wave.RotationDegrees);
            var damage = CreateDamage(scheduled, effect.DamageMultiplier);
            var returnAfter = effect.Range / effect.Speed;
            for (var i = 0; i < directions.Length; i++)
            {
                _projectileLauncher.Launch(new ActiveSkillProjectile(
                    scheduled.Activation.Origin,
                    directions[i],
                    effect.Speed,
                    returnAfter * 2f + 1f,
                    effect.CollisionRadius,
                    0f,
                    damage,
                    returnAfterSeconds: returnAfter,
                    returnDamageMultiplier: effect.ReturnDamageMultiplier,
                    returnTarget: scheduled.Activation.OwnerTransform));
            }
        }

        private static void ExecuteArea(ScheduledEffect scheduled, AreaEffect effect)
        {
            var center = scheduled.CenterOverride ??
                         (scheduled.Activation.LevelDefinition.TargetingMode == ActiveSkillTargetingMode.Self
                             ? scheduled.Activation.Origin
                             : scheduled.Activation.AimPoint);
            EnemyDamageArea.Apply(center, effect.Radius, CreateDamage(scheduled, effect.DamageMultiplier));
        }

        private void ExecuteBeamTick(ScheduledEffect scheduled, BeamEffect effect)
        {
            var direction = scheduled.Activation.AimDirection;
            if (effect.TracksTarget && scheduled.Activation.InitialTarget != null && scheduled.Activation.InitialTarget.IsAlive)
                direction = (scheduled.Activation.InitialTarget.Position - scheduled.Activation.Origin).normalized;

            var damage = CreateDamage(scheduled, effect.DamageMultiplier);
            EnemyRegistry.CopyAliveTo(_enemyBuffer);
            for (var i = 0; i < _enemyBuffer.Count; i++)
            {
                var enemy = _enemyBuffer[i];
                var offset = enemy.Position - scheduled.Activation.Origin;
                var forward = Vector2.Dot(offset, direction);
                if (forward < 0f || forward > effect.Range)
                    continue;
                var perpendicular = Mathf.Abs(direction.x * offset.y - direction.y * offset.x);
                if (perpendicular <= effect.Width * 0.5f)
                    enemy.ApplyDamage(damage);
            }
        }

        private static void ExecuteOrbitTick(ScheduledEffect scheduled, OrbitEffect effect)
        {
            var center = scheduled.Activation.OwnerTransform != null
                ? (Vector2)scheduled.Activation.OwnerTransform.position
                : scheduled.Activation.Origin;
            var baseRotation = scheduled.Wave.RotationDegrees +
                               scheduled.TickIndex * effect.AngularSpeedDegrees * effect.HitCooldownSeconds;
            var directions = ProjectileDirectionGenerator.Create(
                ProjectileLayout.Ring,
                effect.BladeCount,
                Vector2.right,
                rotationDegrees: baseRotation);
            var damage = CreateDamage(scheduled, effect.DamageMultiplier);
            for (var i = 0; i < directions.Length; i++)
                EnemyDamageArea.Apply(center + directions[i] * effect.Radius, 0.3f, damage);
        }

        private void ExecuteChain(ScheduledEffect scheduled, ChainEffect effect)
        {
            EnemyRegistry.CopyAliveTo(_enemyBuffer);
            _chainHitBuffer.Clear();
            IEnemyDamageReceiver current = scheduled.Activation.InitialTarget;
            var damageAmount = scheduled.Activation.Damage * scheduled.Wave.DamageMultiplier * effect.DamageMultiplier;

            for (var jump = 0; jump < effect.TargetCount; jump++)
            {
                if (current == null || !current.IsAlive || !_chainHitBuffer.Add(current))
                    break;

                var currentPosition = current.Position;
                current.ApplyDamage(new EnemyDamageRequest(scheduled.Activation.SourceId, damageAmount));
                damageAmount *= effect.DamageRetentionPerJump;

                IEnemyDamageReceiver next = null;
                var nearestDistance = effect.JumpRange * effect.JumpRange;
                for (var i = 0; i < _enemyBuffer.Count; i++)
                {
                    var candidate = _enemyBuffer[i];
                    if (!candidate.IsAlive || _chainHitBuffer.Contains(candidate))
                        continue;
                    var distance = (candidate.Position - currentPosition).sqrMagnitude;
                    if (distance > nearestDistance)
                        continue;
                    nearestDistance = distance;
                    next = candidate;
                }
                current = next;
            }
        }

        private void PlaceMine(ScheduledEffect scheduled, MineEffect effect)
        {
            var sameSourceCount = 0;
            for (var i = 0; i < _mines.Count; i++)
            {
                if (_mines[i].SourceId == scheduled.Activation.SourceId)
                    sameSourceCount++;
            }
            if (sameSourceCount >= effect.MaxConcurrent)
            {
                for (var i = 0; i < _mines.Count; i++)
                {
                    if (_mines[i].SourceId != scheduled.Activation.SourceId)
                        continue;
                    _mines[i].Dispose();
                    _mines.RemoveAt(i);
                    break;
                }
            }

            var marker = _minePool.Rent();
            marker.transform.SetParent(null, worldPositionStays: false);
            marker.transform.position = scheduled.Activation.Origin;
            marker.transform.localScale = Vector3.one * effect.TriggerRadius * 2f;
            marker.sprite = PlaceholderSprite.Shared;
            marker.color = new Color(1f, 0.35f, 0.1f, 0.8f);
            _mines.Add(new MineState(scheduled, effect, marker.gameObject, _minePool));
        }

        private static SpriteRenderer CreateMineMarker()
        {
            var marker = new GameObject("Fixture Active Skill Mine");
            return marker.AddComponent<SpriteRenderer>();
        }

        private void TickMines(float deltaTime)
        {
            using var minesGuard = PerfGuard.Measure("SceneActiveSkillEffectExecutor.TickMines", TickMinesWarningMilliseconds);
            EnemyRegistry.CopyAliveTo(_enemyBuffer);
            for (var i = _mines.Count - 1; i >= 0; i--)
            {
                var mine = _mines[i];
                mine.Elapsed += deltaTime;
                var triggered = mine.Elapsed >= mine.Effect.LifetimeSeconds;
                for (var enemyIndex = 0; !triggered && enemyIndex < _enemyBuffer.Count; enemyIndex++)
                {
                    if (_enemyBuffer[enemyIndex].IsAlive &&
                        (_enemyBuffer[enemyIndex].Position - mine.Position).sqrMagnitude <=
                        mine.Effect.TriggerRadius * mine.Effect.TriggerRadius)
                    {
                        triggered = true;
                    }
                }

                if (!triggered)
                    continue;

                EnemyDamageArea.Apply(
                    mine.Position,
                    mine.Effect.BlastRadius,
                    CreateDamage(mine.Scheduled, mine.Effect.DamageMultiplier));
                if (mine.Effect.SecondaryDamageMultiplier > 0f)
                {
                    _scheduled.Add(new ScheduledEffect(
                        mine.Scheduled.Activation,
                        mine.Scheduled.Wave,
                        new AreaEffect(mine.Effect.BlastRadius * 0.75f, mine.Effect.SecondaryDamageMultiplier),
                        mine.Effect.SecondaryDelaySeconds,
                        0,
                        mine.Position));
                }
                mine.Dispose();
                _mines.RemoveAt(i);
            }
        }

        private static EnemyDamageRequest CreateDamage(ScheduledEffect scheduled, float effectMultiplier)
        {
            return new EnemyDamageRequest(
                scheduled.Activation.SourceId,
                scheduled.Activation.Damage * scheduled.Wave.DamageMultiplier * effectMultiplier);
        }

        public void Dispose()
        {
            for (var i = 0; i < _mines.Count; i++)
                _mines[i].Dispose();
            _mines.Clear();
            _scheduled.Clear();

            if (_minePoolRoot == null)
                return;
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(_minePoolRoot.gameObject);
            else
                UnityEngine.Object.DestroyImmediate(_minePoolRoot.gameObject);
        }

        private sealed class ScheduledEffect
        {
            public ActiveSkillActivation Activation { get; }
            public ActiveSkillActivationWave Wave { get; }
            public IActiveSkillEffect Effect { get; }
            public int TickIndex { get; }
            public Vector2? CenterOverride { get; }
            public float RemainingDelay { get; set; }

            public ScheduledEffect(
                ActiveSkillActivation activation,
                ActiveSkillActivationWave wave,
                IActiveSkillEffect effect,
                float remainingDelay,
                int tickIndex,
                Vector2? centerOverride = null)
            {
                Activation = activation;
                Wave = wave;
                Effect = effect;
                RemainingDelay = remainingDelay;
                TickIndex = tickIndex;
                CenterOverride = centerOverride;
            }
        }

        private sealed class MineState : IDisposable
        {
            private readonly GameObjectPool<SpriteRenderer> _pool;

            public ScheduledEffect Scheduled { get; }
            public MineEffect Effect { get; }
            public GameObject Marker { get; }
            public ContentId SourceId => Scheduled.Activation.SourceId;
            public Vector2 Position => Marker != null ? (Vector2)Marker.transform.position : Scheduled.Activation.Origin;
            public float Elapsed { get; set; }

            public MineState(ScheduledEffect scheduled, MineEffect effect, GameObject marker, GameObjectPool<SpriteRenderer> pool)
            {
                Scheduled = scheduled;
                Effect = effect;
                Marker = marker;
                _pool = pool;
            }

            public void Dispose()
            {
                if (Marker == null)
                    return;
                _pool.Return(Marker.GetComponent<SpriteRenderer>());
            }
        }
    }
}
