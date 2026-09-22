using System;
using System.Collections.Generic;
using Game.Content;
using Game.Combat;
using Game.Diagnostics;
using Game.Enemy;
using Game.Movement;
using Game.Pooling;
using Game.Run;
using Game.Presentation;
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
        private readonly List<ScheduledSkillEffect> _scheduled = new List<ScheduledSkillEffect>();
        private readonly List<SkillMineState> _mines = new List<SkillMineState>();
        private readonly List<IEnemyDamageReceiver> _enemyBuffer = new List<IEnemyDamageReceiver>();
        private readonly ICombatTargetQuery _targets;
        private readonly HashSet<EnemyTargetLife> _chainHitBuffer = new HashSet<EnemyTargetLife>();
        private readonly List<EnemyTargetLife> _chainCandidates = new List<EnemyTargetLife>();
        private readonly Transform _minePoolRoot;
        private readonly GameObjectPool<SpriteRenderer> _minePool;
        private readonly ContentRegistry _contentRegistry;

        public int ScheduledCount => _scheduled.Count;
        public int ActiveMineCount => _mines.Count;

        public SceneActiveSkillEffectExecutor(
            RunController runController,
            IActiveSkillProjectileLauncher projectileLauncher = null,
            ICombatTargetQuery targets = null,
            ContentRegistry contentRegistry = null)
        {
            _runController = runController != null
                ? runController
                : throw new ArgumentNullException(nameof(runController));
            _projectileLauncher = projectileLauncher ?? new SceneProjectileLauncher(runController);
            _targets = targets ?? new SceneCombatTargetQuery();
            _contentRegistry = contentRegistry;
            _minePoolRoot = new GameObject("Mine Pool").transform;
            _minePool = new GameObjectPool<SpriteRenderer>(CreateMineMarker, _minePoolRoot);
        }

        public void Schedule(ActiveSkillActivation activation)
        {
            using var guard = PerfGuard.Measure("SceneActiveSkillEffectExecutor.Schedule", 2f);
            var waves = activation.LevelDefinition.Waves;
            var randomTargets = activation.LevelDefinition.TargetingMode == ActiveSkillTargetingMode.RandomEnemy;
            var usedTargets = randomTargets ? new HashSet<EnemyTargetLife>() : null;
            if (randomTargets) usedTargets.Add(activation.TargetLife);
            for (var waveIndex = 0; waveIndex < waves.Count; waveIndex++)
            {
                Vector2? centerOverride = null;
                if (randomTargets && waveIndex > 0)
                {
                    _targets.CopyAliveTo(_enemyBuffer);
                    var radius = activation.LevelDefinition.Targeting.Radius * activation.RangeMultiplier;
                    IEnemyDamageReceiver selected = null;
                    var count = 0;
                    foreach (var candidate in _enemyBuffer)
                    {
                        var life = new EnemyTargetLife(candidate);
                        if (!life.IsAlive || usedTargets.Contains(life) || (candidate.Position - activation.Origin).sqrMagnitude > radius * radius) continue;
                        if (activation.Random.Next(++count) == 0) selected = candidate;
                    }
                    if (selected == null) continue;
                    usedTargets.Add(new EnemyTargetLife(selected));
                    centerOverride = selected.Position;
                }
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
                        _scheduled.Add(new ScheduledSkillEffect(
                            activation,
                            wave,
                            effect,
                            wave.DelaySeconds + repeatInterval * tickIndex,
                            tickIndex, centerOverride));
                    }
                }
            }
        }

        public void Tick(float deltaTime, bool isRunning)
        {
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            if (!isRunning)
            {
                if (_runController.Model != null && (_runController.Model.State == RunState.Won ||
                    _runController.Model.State == RunState.Lost || _runController.Model.State == RunState.Stopped)) Clear();
                return;
            }

            using var _ = PerfGuard.Measure("SceneActiveSkillEffectExecutor.Tick", TickWarningMilliseconds);
            foreach (var scheduled in _scheduled) scheduled.RemainingDelay -= deltaTime;
            _scheduled.Sort((left, right) => left.RemainingDelay.CompareTo(right.RemainingDelay));
            while (_scheduled.Count > 0 && _scheduled[0].RemainingDelay <= 0f)
            {
                var scheduled = _scheduled[0];
                _scheduled.RemoveAt(0);
                Execute(scheduled);
            }

            TickMines(deltaTime);
        }

        private void Execute(ScheduledSkillEffect scheduled)
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

        private void ExecuteProjectileBurst(ScheduledSkillEffect scheduled, ProjectileBurstEffect effect)
        {
            var directions = ProjectileDirectionGenerator.Create(
                effect.Layout,
                effect.ProjectileCount,
                scheduled.Activation.AimDirection,
                effect.SpreadDegrees,
                scheduled.Wave.RotationDegrees + scheduled.Activation.RotationDegrees, scheduled.Activation.Random);
            var damage = CreateDamage(scheduled, effect.DamageMultiplier);
            if (effect.Behavior.DistinctNearestTargets) _targets.CopyAliveTo(_enemyBuffer);
            for (var i = 0; i < directions.Length; i++)
            {
                if (effect.Behavior.DistinctNearestTargets)
                {
                    var nearest = -1;
                    var distance = float.PositiveInfinity;
                    for (var targetIndex = 0; targetIndex < _enemyBuffer.Count; targetIndex++)
                    {
                        var candidate = _enemyBuffer[targetIndex];
                        var candidateDistance = (candidate.Position - scheduled.Activation.Origin).sqrMagnitude;
                        var radius = scheduled.Activation.LevelDefinition.Targeting.Radius * scheduled.Activation.RangeMultiplier;
                        if (radius > 0f && candidateDistance > radius * radius) continue;
                        if (candidate.IsAlive && candidateDistance < distance) { nearest = targetIndex; distance = candidateDistance; }
                    }
                    if (nearest < 0) break;
                    directions[i] = (_enemyBuffer[nearest].Position - scheduled.Activation.Origin).normalized;
                    if (directions[i].sqrMagnitude <= Mathf.Epsilon) directions[i] = scheduled.Activation.AimDirection;
                    _enemyBuffer.RemoveAt(nearest);
                }
                _projectileLauncher.Launch(new ActiveSkillProjectile(
                    scheduled.Activation.Origin, directions[i], effect.Speed,
                    effect.LifetimeSeconds * scheduled.Activation.RangeMultiplier,
                    effect.CollisionRadius * scheduled.Activation.SizeMultiplier,
                    effect.ImpactAreaRadius * scheduled.Activation.SizeMultiplier,
                    damage, effect.PierceCount, behavior: effect.Behavior,
                    rangeMultiplier: scheduled.Activation.RangeMultiplier,
                    visual: ResolveProjectileVisual(scheduled.Activation.LevelDefinition)));
            }
        }

        private void ExecuteBoomerang(ScheduledSkillEffect scheduled, BoomerangEffect effect)
        {
            var directions = ProjectileDirectionGenerator.Create(
                ProjectileLayout.Fan,
                effect.ProjectileCount,
                scheduled.Activation.AimDirection,
                effect.SpreadDegrees,
                scheduled.Wave.RotationDegrees + scheduled.Activation.RotationDegrees);
            var damage = CreateDamage(scheduled, effect.DamageMultiplier);
            var returnAfter = effect.Range * scheduled.Activation.RangeMultiplier / effect.Speed;
            for (var i = 0; i < directions.Length; i++)
            {
                _projectileLauncher.Launch(new ActiveSkillProjectile(
                    scheduled.Activation.Origin,
                    directions[i],
                    effect.Speed,
                    effect.LifetimeSeconds * scheduled.Activation.RangeMultiplier,
                    effect.CollisionRadius * scheduled.Activation.SizeMultiplier,
                    0f,
                    damage,
                    returnAfterSeconds: returnAfter,
                    returnDamageMultiplier: effect.ReturnDamageMultiplier,
                    returnTarget: scheduled.Activation.OwnerTransform,
                    hitLedger: scheduled.Activation.HitLedger,
                    hitCooldownSeconds: effect.HitCooldownSeconds,
                    returnKnockbackMultiplier: effect.ReturnKnockbackMultiplier,
                    visual: ResolveProjectileVisual(scheduled.Activation.LevelDefinition)));
            }
        }

        private SpriteDefinition ResolveProjectileVisual(ActiveSkillLevelDefinition level)
        {
            if (_contentRegistry == null || !level.Visual.Id.IsValid) return null;
            var visual = level.Visual.Resolve(_contentRegistry);
            if (visual.Role == SpriteRole.Unspecified) return null;
            visual.RequireRole(SpriteRole.Projectile);
            return visual;
        }

        private static void ExecuteArea(ScheduledSkillEffect scheduled, AreaEffect effect)
        {
            var center = scheduled.CenterOverride ??
                         (scheduled.Activation.LevelDefinition.TargetingMode == ActiveSkillTargetingMode.Self
                             ? scheduled.Activation.Origin
                             : scheduled.Activation.AimPoint);
            EnemyDamageArea.Apply(center, effect.Radius * scheduled.Activation.SizeMultiplier, CreateDamage(scheduled, effect.DamageMultiplier));
        }

        private void ExecuteBeamTick(ScheduledSkillEffect scheduled, BeamEffect effect)
        {
            var direction = scheduled.Activation.AimDirection;
            if (effect.TracksTarget && scheduled.Activation.TargetLife.IsAlive)
                direction = (scheduled.Activation.InitialTarget.Position - scheduled.Activation.Origin).normalized;

            var damage = CreateDamage(scheduled, effect.DamageMultiplier);
            _targets.CopyAliveTo(_enemyBuffer);
            for (var i = 0; i < _enemyBuffer.Count; i++)
            {
                var enemy = _enemyBuffer[i];
                var offset = enemy.Position - scheduled.Activation.Origin;
                var forward = Vector2.Dot(offset, direction);
                if (forward < 0f || forward > effect.Range * scheduled.Activation.RangeMultiplier)
                    continue;
                var perpendicular = Mathf.Abs(direction.x * offset.y - direction.y * offset.x);
                if (perpendicular <= effect.Width * scheduled.Activation.SizeMultiplier * 0.5f)
                    enemy.ApplyDamage(damage.WithDirection(offset.x, offset.y));
            }
        }

        private static void ExecuteOrbitTick(ScheduledSkillEffect scheduled, OrbitEffect effect)
        {
            var center = scheduled.Activation.OwnerTransform != null
                ? (Vector2)scheduled.Activation.OwnerTransform.position
                : scheduled.Activation.Origin;
            var baseRotation = scheduled.Wave.RotationDegrees + scheduled.Activation.RotationDegrees +
                               scheduled.TickIndex * effect.AngularSpeedDegrees * effect.HitCooldownSeconds;
            var directions = ProjectileDirectionGenerator.Create(
                ProjectileLayout.Ring,
                effect.BladeCount,
                Vector2.right,
                rotationDegrees: baseRotation);
            var damage = CreateDamage(scheduled, effect.DamageMultiplier);
            for (var i = 0; i < directions.Length; i++)
                EnemyDamageArea.Apply(center + directions[i] * effect.Radius * scheduled.Activation.RangeMultiplier, effect.BladeHitboxRadius * scheduled.Activation.SizeMultiplier, damage);
        }

        private void ExecuteChain(ScheduledSkillEffect scheduled, ChainEffect effect)
        {
            _targets.CopyAliveTo(_enemyBuffer);
            _chainHitBuffer.Clear();
            _chainCandidates.Clear();
            foreach (var candidate in _enemyBuffer) _chainCandidates.Add(new EnemyTargetLife(candidate));
            IEnemyDamageReceiver current = scheduled.Activation.TargetLife.IsAlive ? scheduled.Activation.InitialTarget : null;
            var previousPosition = scheduled.Activation.Origin;
            var damageAmount = scheduled.Activation.Damage * scheduled.Wave.DamageMultiplier * effect.DamageMultiplier;

            for (var jump = 0; jump < effect.TargetCount; jump++)
            {
                if (current == null || !current.IsAlive || !_chainHitBuffer.Add(new EnemyTargetLife(current)))
                    break;

                var currentPosition = current.Position;
                var direction = currentPosition - previousPosition;
                current.ApplyDamage(CreateDamage(scheduled, effect.DamageMultiplier).WithAmount(damageAmount).WithDirection(direction.x, direction.y));
                previousPosition = currentPosition;
                damageAmount *= effect.DamageRetentionPerJump;

                IEnemyDamageReceiver next = null;
                var range = effect.JumpRange * scheduled.Activation.RangeMultiplier;
                var nearestDistance = range * range;
                for (var i = 0; i < _chainCandidates.Count; i++)
                {
                    var life = _chainCandidates[i];
                    var candidate = life.Target;
                    if (!life.IsAlive || _chainHitBuffer.Contains(life))
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

        private void PlaceMine(ScheduledSkillEffect scheduled, MineEffect effect)
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
            _mines.Add(new SkillMineState(scheduled, effect, marker.gameObject, _minePool));
        }

        private static SpriteRenderer CreateMineMarker()
        {
            var marker = new GameObject("Fixture Active Skill Mine");
            return marker.AddComponent<SpriteRenderer>();
        }

        private void TickMines(float deltaTime)
        {
            using var minesGuard = PerfGuard.Measure("SceneActiveSkillEffectExecutor.TickMines", TickMinesWarningMilliseconds);
            _targets.CopyAliveTo(_enemyBuffer);
            for (var i = _mines.Count - 1; i >= 0; i--)
            {
                var mine = _mines[i];
                mine.Elapsed += deltaTime;
                var triggered = mine.Elapsed >= mine.Effect.LifetimeSeconds;
                for (var enemyIndex = 0; !triggered && enemyIndex < _enemyBuffer.Count; enemyIndex++)
                {
                    if (_enemyBuffer[enemyIndex].IsAlive &&
                        (_enemyBuffer[enemyIndex].Position - mine.Position).sqrMagnitude <=
                        mine.Effect.TriggerRadius * mine.Effect.TriggerRadius * mine.Scheduled.Activation.RangeMultiplier * mine.Scheduled.Activation.RangeMultiplier)
                    {
                        triggered = true;
                    }
                }

                if (!triggered)
                    continue;

                EnemyDamageArea.Apply(
                    mine.Position,
                    mine.Effect.BlastRadius * mine.Scheduled.Activation.SizeMultiplier,
                    CreateDamage(mine.Scheduled, mine.Effect.DamageMultiplier));
                if (mine.Effect.SecondaryDamageMultiplier > 0f)
                {
                    _scheduled.Add(new ScheduledSkillEffect(
                        mine.Scheduled.Activation,
                        mine.Scheduled.Wave,
                        new AreaEffect(mine.Effect.BlastRadius * mine.Effect.SecondaryRadiusMultiplier, mine.Effect.DamageMultiplier * mine.Effect.SecondaryDamageMultiplier),
                        mine.Effect.SecondaryDelaySeconds,
                        0,
                        mine.Position, mine.Effect.SecondaryKnockbackMultiplier));
                }
                mine.Dispose();
                _mines.RemoveAt(i);
            }
        }

        private static EnemyDamageRequest CreateDamage(ScheduledSkillEffect scheduled, float effectMultiplier)
        {
            return new EnemyDamageRequest(new CombatDamageRequest(
                scheduled.Activation.Source,
                scheduled.Activation.Damage * scheduled.Wave.DamageMultiplier * effectMultiplier,
                scheduled.Wave.Controls,
                outgoingKnockbackMultiplier: scheduled.Activation.OutgoingKnockbackMultiplier * scheduled.KnockbackMultiplier));
        }

        public void Clear()
        {
            for (var i = 0; i < _mines.Count; i++) _mines[i].Dispose();
            _mines.Clear();
            _scheduled.Clear();
            if (_projectileLauncher is SceneProjectileLauncher launcher) launcher.Clear();
        }

        public void Dispose()
        {
            Clear();
            if (_projectileLauncher is SceneProjectileLauncher launcher) launcher.Dispose();
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




    }
}
