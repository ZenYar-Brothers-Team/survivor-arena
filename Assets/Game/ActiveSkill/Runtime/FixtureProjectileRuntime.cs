using System;
using System.Collections.Generic;
using Game.Combat;
using Game.Content;
using Game.Diagnostics;
using Game.Enemy;
using Game.Movement;
using Game.Pooling;
using Game.Run;
using Game.Presentation;
using UnityEngine;

namespace Game.ActiveSkill
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
    public sealed class FixtureProjectileRuntime : MonoBehaviour
    {
        private Rigidbody2D _body;
        private CircleCollider2D _collider;
        private SpriteRenderer _renderer;
        private SpriteRenderer _visualRenderer;
        private ProjectileImpactRuntime _impact;
        private ExplosionBurstRuntime _explosion;
        private RunController _runController;
        private ActiveSkillProjectile _projectile;
        private GameObjectPool<FixtureProjectileRuntime> _pool;
        private readonly HashSet<EnemyTargetLife> _hitThisPass = new HashSet<EnemyTargetLife>();
        private readonly List<IEnemyDamageReceiver> _targets = new List<IEnemyDamageReceiver>();
        private readonly ICombatTargetQuery _query = new SceneCombatTargetQuery();
        private Vector2 _direction;
        private float _elapsed;
        private int _remainingHits;
        private int _ricochets;
        private float _retention;
        private bool _isReturning;
        private bool _initialized;
        private bool _despawned;
        private int _generation;
        private bool _impactReleasePending;
        public event Action<FixtureProjectileRuntime> Returned;
        public ContentId SourceId => _projectile.Damage.SourceId;
        public Vector2 Direction => _direction;
        public Vector2 Position => _body != null ? _body.position : (Vector2)transform.position;
        public bool IsDespawned => _despawned;

        private void Awake() => CacheComponents();
        public void Initialize(ActiveSkillProjectile projectile, RunController runController, GameObjectPool<FixtureProjectileRuntime> pool = null)
        {
            Shutdown();
            _generation++;
            _runController = runController != null ? runController : throw new ArgumentNullException(nameof(runController));
            _projectile = projectile;
            _pool = pool;
            CacheComponents();
            _body.bodyType = RigidbodyType2D.Kinematic;
            _body.gravityScale = 0f;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _body.constraints |= RigidbodyConstraints2D.FreezeRotation;
            _body.linearVelocity = Vector2.zero;
            _collider.isTrigger = true;
            _collider.radius = projectile.CollisionRadius;
            _collider.enabled = true;
            _body.position = projectile.Origin;
            transform.position = projectile.Origin;
            _direction = projectile.Direction;
            _remainingHits = 1 + projectile.PierceCount;
            _ricochets = projectile.Behavior.RicochetCount;
            _retention = 1f;
            _despawned = false;
            _initialized = true;
            _impactReleasePending = false;
            ConfigureVisual(projectile);
        }
        private void FixedUpdate() => Simulate(Time.fixedDeltaTime);
        public void Simulate(float deltaTime)
        {
            NumericValidation.ValidateNonNegative(deltaTime, nameof(deltaTime));
            if (!_initialized || _runController.Model == null) return;
            var state = _runController.Model.State;
            if (_impactReleasePending)
            {
                if (state == RunState.Won || state == RunState.Lost || state == RunState.Stopped ||
                    TickPresentationTail(deltaTime, state == RunState.Running)) ReleaseNow();
                return;
            }
            if (_despawned) return;
            if (state == RunState.Won || state == RunState.Lost || state == RunState.Stopped) { Despawn(); return; }
            if (state != RunState.Running) return;
            if (_visualRenderer != null && _visualRenderer.enabled)
                _visualRenderer.transform.Rotate(0f, 0f,
                    _projectile.Visual.ProjectilePresentation.SpinDegreesPerSecond * deltaTime);
            var stop = _projectile.Behavior.StopAfterSeconds * _projectile.RangeMultiplier;
            var end = stop > 0f ? Mathf.Min(stop, _projectile.LifetimeSeconds) : _projectile.LifetimeSeconds;
            var dt = Mathf.Min(deltaTime, Mathf.Max(0f, end - _elapsed));
            if (_projectile.Returns && _projectile.ReturnTarget == null) { Despawn(); return; }
            if (_projectile.Returns && !_isReturning)
            {
                var outbound = Mathf.Min(dt, Mathf.Max(0f, _projectile.ReturnAfterSeconds - _elapsed));
                _body.position += _direction * (_projectile.Speed * outbound);
                _elapsed += outbound;
                dt -= outbound;
                if (_elapsed >= _projectile.ReturnAfterSeconds)
                {
                    _isReturning = true;
                    if (_projectile.HitCooldownSeconds == 0f) _hitThisPass.Clear();
                }
            }
            if (_isReturning)
            {
                var offset = (Vector2)_projectile.ReturnTarget.position - _body.position;
                var distance = _projectile.Speed * dt;
                if (offset.sqrMagnitude <= distance * distance) { Despawn(); return; }
                if (offset.sqrMagnitude > Mathf.Epsilon) _direction = offset.normalized;
            }
            var next = _elapsed + dt;
            // Exact integral of linear speed decay: independent of frame subdivision, never accelerates again.
            var travel = stop > 0f ? _projectile.Speed * (dt - (next * next - _elapsed * _elapsed) / (2f * stop)) : _projectile.Speed * dt;
            _body.position += _direction * travel;
            _elapsed = next;
            if (_elapsed >= end)
            {
                var generation = _generation;
                if (_projectile.Behavior.ExplodeOnExpiry) Explode(Position);
                if (!_despawned && generation == _generation)
                {
                    if (HasPlayingPresentationTail()) DespawnAfterImpact();
                    else Despawn();
                }
            }
        }
        private void OnTriggerEnter2D(Collider2D other) => TryImpact(other.GetComponentInParent<IEnemyDamageReceiver>(), other.ClosestPoint(Position));
        private void OnTriggerStay2D(Collider2D other)
        {
            if (_projectile.HitCooldownSeconds > 0f) TryImpact(other.GetComponentInParent<IEnemyDamageReceiver>(), other.ClosestPoint(Position));
        }
        public bool TryImpact(IEnemyDamageReceiver receiver, Vector2 impactPoint)
        {
            if (!_initialized || _despawned || _runController.Model?.State != RunState.Running) return false;
            if (_projectile.Returns && _projectile.ReturnTarget == null) { Despawn(); return false; }
            var life = new EnemyTargetLife(receiver);
            if (!life.IsAlive) return false;
            if (_projectile.HitCooldownSeconds > 0f)
            {
                if (!_projectile.HitLedger.TryHit(life, _projectile.HitCooldownSeconds)) return false;
            }
            else if (!_hitThisPass.Add(life)) return false;
            var generation = _generation;
            var direction = _projectile.Returns ? receiver.Position - (Vector2)_projectile.ReturnTarget.position : _direction;
            var multiplier = _retention * (_isReturning ? _projectile.ReturnDamageMultiplier : 1f);
            var knockback = _retention * (_isReturning ? _projectile.ReturnKnockbackMultiplier : 1f);
            var damage = WithMultipliers(_projectile.Damage, multiplier, knockback).WithDirection(direction.x, direction.y);
            if (_projectile.Behavior.ExplosionDamageMultiplier > 0f) receiver.ApplyDamage(damage);
            else EnemyDamageArea.Apply(impactPoint, _projectile.ImpactAreaRadius, damage, receiver);
            PlayImpact(impactPoint);
            if (_despawned || generation != _generation) return true;
            if (_projectile.Returns || _projectile.Behavior.UnlimitedPierce) return true;
            if (_ricochets > 0 && TryRicochet(receiver, impactPoint)) return true;
            if (--_remainingHits <= 0)
            {
                if (_projectile.Behavior.ExplosionDamageMultiplier > 0f) Explode(impactPoint);
                if (!_despawned && generation == _generation) DespawnAfterImpact();
            }
            return true;
        }
        private bool TryRicochet(IEnemyDamageReceiver previous, Vector2 position)
        {
            using var guard = PerfGuard.Measure("FixtureProjectileRuntime.Ricochet", 1f);
            _query.CopyAliveTo(_targets);
            IEnemyDamageReceiver nearest = FindNext(previous, position, false);
            if (nearest == null && _projectile.Behavior.RepeatRicochetTargets) nearest = FindNext(previous, position, true);
            if (nearest == null) return false;
            _ricochets--;
            _retention *= _projectile.Behavior.RicochetRetention;
            _direction = (nearest.Position - position).normalized;
            if (_direction.sqrMagnitude <= Mathf.Epsilon) _direction = _projectile.Direction;
            _hitThisPass.Remove(new EnemyTargetLife(nearest));
            return true;
        }
        private IEnemyDamageReceiver FindNext(IEnemyDamageReceiver previous, Vector2 position, bool allowRepeat)
        {
            IEnemyDamageReceiver nearest = null;
            var radius = _projectile.Behavior.RicochetRange * _projectile.RangeMultiplier;
            var best = radius * radius;
            foreach (var candidate in _targets)
            {
                var life = new EnemyTargetLife(candidate);
                if (!life.IsAlive || ReferenceEquals(candidate, previous) || (!allowRepeat && _hitThisPass.Contains(life))) continue;
                var distance = (candidate.Position - position).sqrMagnitude;
                if (distance > best) continue;
                nearest = candidate;
                best = distance;
            }
            return nearest;
        }
        private void Explode(Vector2 position)
        {
            var generation = _generation;
            var behavior = _projectile.Behavior;
            EnemyDamageArea.Apply(position, _projectile.ImpactAreaRadius,
                WithMultipliers(_projectile.Damage, behavior.ExplosionDamageMultiplier, behavior.ExplosionKnockbackMultiplier));
            if (!_despawned && generation == _generation) PlayExplosion(position);
        }
        private static EnemyDamageRequest WithMultipliers(EnemyDamageRequest request, float damage, float knockback)
        {
            return new EnemyDamageRequest(request.Combat.Scaled(damage, knockback));
        }
        public void Shutdown()
        {
            _initialized = false;
            _elapsed = 0f;
            _isReturning = false;
            _hitThisPass.Clear();
            _targets.Clear();
            _projectile = default;
            _runController = null;
            _direction = Vector2.zero;
            if (_collider != null) _collider.enabled = false;
            if (_body != null) _body.linearVelocity = Vector2.zero;
            if (_renderer != null) _renderer.enabled = false;
            if (_visualRenderer != null) { _visualRenderer.enabled = false; _visualRenderer.sprite = null; }
            _impact?.ResetPresentation();
            _explosion?.ResetPresentation();
            _impactReleasePending = false;
        }
        public void Despawn()
        {
            if (!_initialized) return;
            if (_impactReleasePending) { _despawned = true; ReleaseNow(); return; }
            if (_despawned) return;
            _despawned = true;
            ReleaseNow();
        }

        private void DespawnAfterImpact()
        {
            if (_despawned) return;
            if (!HasPlayingPresentationTail()) { Despawn(); return; }
            _despawned = true;
            _impactReleasePending = true;
            _collider.enabled = false;
            _body.linearVelocity = Vector2.zero;
            _renderer.enabled = false;
            if (_visualRenderer != null) _visualRenderer.enabled = false;
        }

        private void ReleaseNow()
        {
            var pool = _pool;
            Shutdown();
            Returned?.Invoke(this);
            if (pool != null) pool.Return(this);
            else if (Application.isPlaying) Destroy(gameObject);
            else DestroyImmediate(gameObject);
        }

        private void PlayImpact(Vector2 position)
        {
            var profile = _projectile.Visual?.ProjectilePresentation;
            if (profile == null) return;
            EnsurePresentationObjects();
            _impact.Play(profile, position);
        }

        private void PlayExplosion(Vector2 position)
        {
            var profile = _projectile.Visual?.ProjectilePresentation?.Explosion;
            if (profile == null) return;
            EnsurePresentationObjects();
            _explosion.Play(profile, position, _projectile.ImpactAreaRadius);
        }

        private bool HasPlayingPresentationTail()
        {
            return (_impact != null && _impact.IsPlaying) || (_explosion != null && _explosion.IsPlaying);
        }

        private bool TickPresentationTail(float deltaTime, bool isRunning)
        {
            var impactDone = _impact == null || _impact.Tick(deltaTime, isRunning);
            var explosionDone = _explosion == null || _explosion.Tick(deltaTime, isRunning);
            return impactDone && explosionDone;
        }

        private void ConfigureVisual(ActiveSkillProjectile projectile)
        {
            var visual = projectile.Visual;
            if (visual == null)
            {
                _renderer.sprite = PlaceholderSprite.Shared;
                _renderer.color = new Color(1f, .85f, .15f, 1f);
                _renderer.enabled = true;
                if (_visualRenderer != null) _visualRenderer.enabled = false;
                return;
            }
            EnsurePresentationObjects();
            _renderer.enabled = false;
            _visualRenderer.sprite = visual.Sprite;
            _visualRenderer.color = Color.white;
            _visualRenderer.enabled = true;
            _visualRenderer.transform.localPosition = Vector3.zero;
            var size = visual.Sprite.bounds.size;
            var diameter = projectile.CollisionRadius * 2f * visual.ProjectilePresentation.VisualScale;
            var scale = diameter / Mathf.Max(size.x, size.y);
            _visualRenderer.transform.localScale = Vector3.one * scale;
            var angle = Mathf.Atan2(projectile.Direction.y, projectile.Direction.x) * Mathf.Rad2Deg;
            _visualRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void EnsurePresentationObjects()
        {
            if (_visualRenderer == null)
            {
                var visual = new GameObject("ProjectileVisual");
                visual.transform.SetParent(transform, false);
                _visualRenderer = visual.AddComponent<SpriteRenderer>();
            }
            if (_impact == null) _impact = gameObject.GetComponent<ProjectileImpactRuntime>() ??
                gameObject.AddComponent<ProjectileImpactRuntime>();
            if (_explosion == null) _explosion = gameObject.GetComponent<ExplosionBurstRuntime>() ??
                gameObject.AddComponent<ExplosionBurstRuntime>();
        }
        private void OnDestroy() { Shutdown(); Returned?.Invoke(this); Returned = null; }
        private void CacheComponents()
        {
            if (_body == null) _body = GetComponent<Rigidbody2D>();
            if (_collider == null) _collider = GetComponent<CircleCollider2D>();
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
            if (_renderer.sprite == null) _renderer.sprite = PlaceholderSprite.Shared;
            _renderer.color = new Color(1f, 0.85f, 0.15f, 1f);
        }
    }
}
