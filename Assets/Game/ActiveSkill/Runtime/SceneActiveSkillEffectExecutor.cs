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
        // One orbit tick replaces what used to be one EnemyDamageArea.Apply per blade; it is a single area hit per
        // HitCooldown, so it gets the same 2 ms budget as one EnemyDamageArea.Apply and the executor Tick.
        private const float OrbitBladeAreaWarningMilliseconds = 2f;

        private readonly RunController _runController;
        private readonly IActiveSkillProjectileLauncher _projectileLauncher;
        private readonly List<ScheduledSkillEffect> _scheduled = new List<ScheduledSkillEffect>();
        private readonly List<SkillMineState> _mines = new List<SkillMineState>();
        private readonly List<SkillOrbitVisualState> _orbitVisuals = new List<SkillOrbitVisualState>();
        private readonly List<IEnemyDamageReceiver> _enemyBuffer = new List<IEnemyDamageReceiver>();
        private readonly ICombatTargetQuery _targets;
        private readonly HashSet<EnemyTargetLife> _chainHitBuffer = new HashSet<EnemyTargetLife>();
        private readonly List<EnemyTargetLife> _chainCandidates = new List<EnemyTargetLife>();
        private readonly Transform _minePoolRoot;
        private readonly GameObjectPool<SpriteRenderer> _minePool;
        private readonly Transform _orbitPoolRoot;
        private readonly GameObjectPool<SpriteRenderer> _orbitBladePool;
        private readonly ContentRegistry _contentRegistry;
        private readonly List<PersistentOrbitState> _persistentOrbits = new List<PersistentOrbitState>();
        private readonly List<ExpandingAreaState> _expandingAreas = new List<ExpandingAreaState>();
        private readonly List<PendingStrike> _pendingStrikes = new List<PendingStrike>();
        private readonly List<Vector2> _chainPoints = new List<Vector2>();
        private readonly SkillWorldEffectPresenter _worldEffects;

        /// <summary>Minimum running time a persistent orbit survives without a refresh from its skill.</summary>
        public const float PersistentOrbitMinimumLeaseSeconds = 1f;

        private sealed class PendingStrike
        {
            public ScheduledSkillEffect Scheduled;
            public StrikeEffect Effect;
            public Vector2 Point;
            public float Remaining;
            public object Telegraph;
            public SkillWorldEffectProfile Profile;
        }

        public int ScheduledCount => _scheduled.Count;
        public int ActiveMineCount => _mines.Count;
        public int ActiveOrbitBladeCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < _orbitVisuals.Count; i++) count += _orbitVisuals[i].BladeCount;
                for (var i = 0; i < _persistentOrbits.Count; i++) count += _persistentOrbits[i].VisualBladeCount;
                return count;
            }
        }
        public int PersistentOrbitCount => _persistentOrbits.Count;
        public int ExpandingAreaCount => _expandingAreas.Count;
        public int PendingStrikeCount => _pendingStrikes.Count;
        public int ActiveWorldEffectShapeCount => _worldEffects.ActiveShapeCount;

        /// <summary>Current world radius (authored radius × activation range) of a live persistent orbit.</summary>
        public bool TryGetPersistentOrbitRadius(ContentId sourceId, out float radius)
        {
            for (var i = 0; i < _persistentOrbits.Count; i++)
            {
                if (_persistentOrbits[i].SourceId != sourceId) continue;
                radius = _persistentOrbits[i].Effect.Radius * _persistentOrbits[i].RangeMultiplier;
                return true;
            }
            radius = 0f;
            return false;
        }

        /// <summary>Current persistent orbit blade count/phase for tests and debug observability.</summary>
        public bool TryGetPersistentOrbit(ContentId sourceId, out int bladeCount, out float phaseDegrees)
        {
            for (var i = 0; i < _persistentOrbits.Count; i++)
            {
                if (_persistentOrbits[i].SourceId != sourceId) continue;
                bladeCount = _persistentOrbits[i].BladeCount;
                phaseDegrees = _persistentOrbits[i].PhaseDegrees;
                return true;
            }
            bladeCount = 0;
            phaseDegrees = 0f;
            return false;
        }

        public SceneActiveSkillEffectExecutor(
            RunController runController,
            IActiveSkillProjectileLauncher projectileLauncher = null,
            ICombatTargetQuery targets = null,
            ContentRegistry contentRegistry = null,
            IReadOnlyDictionary<ContentId, SkillWorldEffectProfile> worldEffectProfiles = null)
        {
            _runController = runController != null
                ? runController
                : throw new ArgumentNullException(nameof(runController));
            _projectileLauncher = projectileLauncher ?? new SceneProjectileLauncher(runController);
            _targets = targets ?? new SceneCombatTargetQuery();
            _contentRegistry = contentRegistry;
            _minePoolRoot = new GameObject("Mine Pool").transform;
            _minePool = new GameObjectPool<SpriteRenderer>(CreateMineMarker, _minePoolRoot);
            _orbitPoolRoot = new GameObject("Orbit Blade Pool").transform;
            _orbitBladePool = new GameObjectPool<SpriteRenderer>(CreateOrbitBlade, _orbitPoolRoot);
            _worldEffects = new SkillWorldEffectPresenter(worldEffectProfiles);
        }

        public void Schedule(ActiveSkillActivation activation)
        {
            using var guard = PerfGuard.Measure("SceneActiveSkillEffectExecutor.Schedule", 2f);
            var waves = activation.LevelDefinition.Waves;
            var randomTargets = activation.LevelDefinition.TargetingMode == ActiveSkillTargetingMode.RandomEnemy;
            var usedTargets = randomTargets ? new HashSet<EnemyTargetLife>() : null;
            if (randomTargets) usedTargets.Add(activation.TargetLife);
            StrikeTargetSet deferred = null;
            for (var waveIndex = 0; waveIndex < waves.Count; waveIndex++)
            {
                Vector2? centerOverride = null;
                if (randomTargets && waveIndex > 0 && ContainsStrike(waves[waveIndex]))
                {
                    // Strike waves snapshot their own target when their telegraph starts.
                    if (deferred == null)
                    {
                        deferred = new StrikeTargetSet();
                        deferred.Used.Add(activation.TargetLife);
                    }
                }
                else if (randomTargets && waveIndex > 0)
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
                    else if (effect is OrbitEffect persistent && persistent.Persistent)
                    {
                        RefreshPersistentOrbit(activation, wave, persistent);
                        continue;
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
                            tickIndex, centerOverride) { StrikeTargets = waveIndex > 0 ? deferred : null });
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
            TickOrbitVisuals(deltaTime);
            TickPersistentOrbits(deltaTime);
            TickExpandingAreas(deltaTime);
            TickPendingStrikes(deltaTime);
            _worldEffects.Tick(deltaTime);
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
                case AreaEffect area when area.ExpansionSeconds > 0f:
                    BeginExpandingArea(scheduled, area);
                    break;
                case AreaEffect area:
                    ExecuteArea(scheduled, area);
                    break;
                case StrikeEffect strike:
                    BeginStrike(scheduled, strike);
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

        private void ExecuteOrbitTick(ScheduledSkillEffect scheduled, OrbitEffect effect)
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
            var orbitRadius = effect.Radius * scheduled.Activation.RangeMultiplier;
            var bladeRadius = effect.BladeHitboxRadius * scheduled.Activation.SizeMultiplier;
            using (PerfGuard.Measure("SceneActiveSkillEffectExecutor.OrbitalBladeArea", OrbitBladeAreaWarningMilliseconds))
            {
                // Rented per call (like EnemyDamageArea) so a re-entrant damage callback cannot reuse the list mid-loop.
                var bladeCenters = UnityEngine.Pool.ListPool<Vector2>.Get();
                try
                {
                    for (var i = 0; i < directions.Length; i++)
                        bladeCenters.Add(center + directions[i] * orbitRadius);
                    // One physics query for the whole ring instead of one per blade; per-blade hits are unchanged.
                    EnemyDamageArea.ApplyCircles(center, orbitRadius + bladeRadius, bladeCenters, bladeRadius, damage);
                }
                finally { UnityEngine.Pool.ListPool<Vector2>.Release(bladeCenters); }
            }
            if (scheduled.TickIndex == 0 && scheduled.Activation.OwnerTransform != null)
            {
                var visual = ResolveProjectileVisual(scheduled.Activation.LevelDefinition);
                if (visual != null)
                    _orbitVisuals.Add(new SkillOrbitVisualState(scheduled.Activation.OwnerTransform, visual, effect,
                        scheduled.Activation.RangeMultiplier, scheduled.Activation.SizeMultiplier,
                        scheduled.Wave.RotationDegrees + scheduled.Activation.RotationDegrees, _orbitBladePool));
            }
        }

        private void TickOrbitVisuals(float deltaTime)
        {
            for (var i = _orbitVisuals.Count - 1; i >= 0; i--)
            {
                if (!_orbitVisuals[i].Tick(deltaTime, true)) continue;
                _orbitVisuals[i].Dispose();
                _orbitVisuals.RemoveAt(i);
            }
        }

        private void ExecuteChain(ScheduledSkillEffect scheduled, ChainEffect effect)
        {
            _targets.CopyAliveTo(_enemyBuffer);
            _chainHitBuffer.Clear();
            _chainCandidates.Clear();
            foreach (var candidate in _enemyBuffer) _chainCandidates.Add(new EnemyTargetLife(candidate));
            IEnemyDamageReceiver current = scheduled.Activation.TargetLife.IsAlive ? scheduled.Activation.InitialTarget : null;
            var previousPosition = scheduled.Activation.Origin;
            _chainPoints.Clear();
            _chainPoints.Add(previousPosition);
            var damageAmount = scheduled.Activation.Damage * scheduled.Wave.DamageMultiplier * effect.DamageMultiplier;

            for (var jump = 0; jump < effect.TargetCount; jump++)
            {
                if (current == null || !current.IsAlive || !_chainHitBuffer.Add(new EnemyTargetLife(current)))
                    break;

                var currentPosition = current.Position;
                var direction = currentPosition - previousPosition;
                current.ApplyDamage(CreateDamage(scheduled, effect.DamageMultiplier).WithAmount(damageAmount).WithDirection(direction.x, direction.y));
                previousPosition = currentPosition;
                _chainPoints.Add(currentPosition);
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
            if (_chainPoints.Count > 1 && _worldEffects.TryGetProfile(scheduled.Activation.SourceId, SkillWorldEffectKind.ChainArc, out var profile))
                for (var i = 1; i < _chainPoints.Count; i++) _worldEffects.Segment(profile, _chainPoints[i - 1], _chainPoints[i]);
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

        private static SpriteRenderer CreateOrbitBlade()
        {
            var blade = new GameObject("OrbitBladeVisual");
            return blade.AddComponent<SpriteRenderer>();
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

        private static bool ContainsStrike(ActiveSkillActivationWave wave)
        {
            for (var i = 0; i < wave.Effects.Count; i++)
                if (wave.Effects[i] is StrikeEffect) return true;
            return false;
        }

        private void RefreshPersistentOrbit(ActiveSkillActivation activation, ActiveSkillActivationWave wave, OrbitEffect effect)
        {
            if (activation.OwnerTransform == null) return;
            PersistentOrbitState state = null;
            for (var i = 0; i < _persistentOrbits.Count; i++)
                if (_persistentOrbits[i].SourceId == activation.SourceId && _persistentOrbits[i].Owner == activation.OwnerTransform)
                    state = _persistentOrbits[i];
            if (state == null)
            {
                state = new PersistentOrbitState(activation.SourceId, activation.OwnerTransform, _orbitBladePool,
                    wave.RotationDegrees + activation.RotationDegrees);
                _persistentOrbits.Add(state);
            }
            var damage = new EnemyDamageRequest(new CombatDamageRequest(activation.Source,
                activation.Damage * wave.DamageMultiplier * effect.DamageMultiplier, wave.Controls,
                outgoingKnockbackMultiplier: activation.OutgoingKnockbackMultiplier,
                slowedTargetDamageFactor: activation.SlowedTargetDamageFactor,
                slowedTargetKnockbackBonus: activation.SlowedTargetBonus.KnockbackBonus));
            var lease = Mathf.Max(PersistentOrbitMinimumLeaseSeconds, activation.LevelDefinition.CooldownSeconds * 4f);
            state.Refresh(effect, damage, activation.RangeMultiplier, activation.SizeMultiplier, lease,
                ResolveProjectileVisual(activation.LevelDefinition));
        }

        private void TickPersistentOrbits(float deltaTime)
        {
            for (var i = _persistentOrbits.Count - 1; i >= 0; i--)
            {
                if (i >= _persistentOrbits.Count) continue;
                if (_persistentOrbits[i].Tick(deltaTime)) continue;
                _persistentOrbits[i].Dispose();
                _persistentOrbits.RemoveAt(i);
            }
        }

        private void BeginExpandingArea(ScheduledSkillEffect scheduled, AreaEffect effect)
        {
            var center = scheduled.CenterOverride ??
                         (scheduled.Activation.LevelDefinition.TargetingMode == ActiveSkillTargetingMode.Self
                             ? (scheduled.Activation.OwnerTransform != null ? (Vector2)scheduled.Activation.OwnerTransform.position : scheduled.Activation.Origin)
                             : scheduled.Activation.AimPoint);
            var state = new ExpandingAreaState(center, effect.Radius * scheduled.Activation.SizeMultiplier,
                effect.ExpansionSeconds, CreateDamage(scheduled, effect.DamageMultiplier));
            if (_worldEffects.TryGetProfile(scheduled.Activation.SourceId, SkillWorldEffectKind.ExpandingRing, out var profile))
            {
                state.VisualProfile = profile;
                state.VisualHandle = _worldEffects.BeginRing(profile, center, 0f);
            }
            _expandingAreas.Add(state);
            // The wave began when its delay elapsed inside this tick; advance the front by that overshoot so
            // front timing does not depend on frame length (expanding areas were already ticked this frame).
            state.Tick(Mathf.Max(0f, -scheduled.RemainingDelay));
            if (state.VisualHandle != null)
                _worldEffects.UpdateRing(state.VisualHandle, state.CurrentRadius, state.VisualProfile.Thickness);
        }

        private void TickExpandingAreas(float deltaTime)
        {
            for (var i = _expandingAreas.Count - 1; i >= 0; i--)
            {
                if (i >= _expandingAreas.Count) continue;
                var state = _expandingAreas[i];
                var complete = state.Tick(deltaTime);
                if (state.VisualHandle != null)
                    _worldEffects.UpdateRing(state.VisualHandle, state.CurrentRadius, state.VisualProfile.Thickness);
                if (!complete) continue;
                if (state.VisualHandle != null) _worldEffects.Release(state.VisualHandle, state.VisualProfile.FadeSeconds);
                _expandingAreas.RemoveAt(i);
            }
        }

        private void BeginStrike(ScheduledSkillEffect scheduled, StrikeEffect effect)
        {
            Vector2 point;
            if (scheduled.StrikeTargets == null)
            {
                point = scheduled.CenterOverride ?? scheduled.Activation.AimPoint;
            }
            else
            {
                // Later strike: new distinct random valid target at its own telegraph start; none -> skipped.
                _targets.CopyAliveTo(_enemyBuffer);
                var radius = scheduled.Activation.LevelDefinition.Targeting.Radius * scheduled.Activation.RangeMultiplier;
                var origin = scheduled.Activation.OwnerTransform != null ? (Vector2)scheduled.Activation.OwnerTransform.position : scheduled.Activation.Origin;
                IEnemyDamageReceiver selected = null;
                var count = 0;
                foreach (var candidate in _enemyBuffer)
                {
                    var life = new EnemyTargetLife(candidate);
                    if (!life.IsAlive || scheduled.StrikeTargets.Used.Contains(life) || (candidate.Position - origin).sqrMagnitude > radius * radius) continue;
                    var roll = scheduled.Activation.Random != null ? scheduled.Activation.Random.Next(++count) : ++count - 1;
                    if (roll == 0) selected = candidate;
                }
                if (selected == null) return;
                scheduled.StrikeTargets.Used.Add(new EnemyTargetLife(selected));
                point = selected.Position;
            }
            var pending = new PendingStrike { Scheduled = scheduled, Effect = effect, Point = point, Remaining = effect.TelegraphSeconds };
            if (_worldEffects.TryGetProfile(scheduled.Activation.SourceId, SkillWorldEffectKind.StrikeTelegraph, out var profile))
            {
                pending.Profile = profile;
                pending.Telegraph = _worldEffects.BeginTelegraph(profile, point, effect.Radius * scheduled.Activation.SizeMultiplier);
            }
            _pendingStrikes.Add(pending);
            if (effect.TelegraphSeconds <= 0f) TickPendingStrikes(0f);
        }

        private void TickPendingStrikes(float deltaTime)
        {
            for (var i = 0; i < _pendingStrikes.Count; i++) _pendingStrikes[i].Remaining -= deltaTime;
            for (var i = _pendingStrikes.Count - 1; i >= 0; i--)
            {
                if (i >= _pendingStrikes.Count) continue;
                var pending = _pendingStrikes[i];
                if (pending.Remaining > 0f) continue;
                _pendingStrikes.RemoveAt(i);
                var radius = pending.Effect.Radius * pending.Scheduled.Activation.SizeMultiplier;
                if (pending.Telegraph != null) _worldEffects.Release(pending.Telegraph, 0.0001f);
                EnemyDamageArea.Apply(pending.Point, radius, CreateDamage(pending.Scheduled, pending.Effect.DamageMultiplier));
                if (pending.Profile != null) _worldEffects.Flash(pending.Profile, pending.Point, radius);
            }
        }

        private static EnemyDamageRequest CreateDamage(ScheduledSkillEffect scheduled, float effectMultiplier)
        {
            return new EnemyDamageRequest(new CombatDamageRequest(
                scheduled.Activation.Source,
                scheduled.Activation.Damage * scheduled.Wave.DamageMultiplier * effectMultiplier,
                scheduled.Wave.Controls,
                outgoingKnockbackMultiplier: scheduled.Activation.OutgoingKnockbackMultiplier * scheduled.KnockbackMultiplier,
                slowedTargetDamageFactor: scheduled.Activation.SlowedTargetDamageFactor,
                slowedTargetKnockbackBonus: scheduled.Activation.SlowedTargetBonus.KnockbackBonus));
        }

        public void Clear()
        {
            for (var i = 0; i < _persistentOrbits.Count; i++) _persistentOrbits[i].Dispose();
            _persistentOrbits.Clear();
            _expandingAreas.Clear();
            _pendingStrikes.Clear();
            _worldEffects.Clear();
            for (var i = 0; i < _orbitVisuals.Count; i++) _orbitVisuals[i].Dispose();
            _orbitVisuals.Clear();
            for (var i = 0; i < _mines.Count; i++) _mines[i].Dispose();
            _mines.Clear();
            _scheduled.Clear();
            if (_projectileLauncher is SceneProjectileLauncher launcher) launcher.Clear();
        }

        public void Dispose()
        {
            Clear();
            _worldEffects.Dispose();
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
            if (_orbitPoolRoot == null)
                return;
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(_orbitPoolRoot.gameObject);
            else
                UnityEngine.Object.DestroyImmediate(_orbitPoolRoot.gameObject);
        }




    }
}
