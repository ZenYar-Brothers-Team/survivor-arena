using Game.Character;
using Game.Combat;
using Game.Movement;
using Game.Pooling;
using Game.Presentation;
using Game.Run;
using UnityEngine;

namespace Game.Enemy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class EnemyProjectileRuntime : MonoBehaviour
    {
        private Rigidbody2D _body;
        private CircleCollider2D _collider;
        private SpriteRenderer _renderer;
        private SpriteRenderer _visualRenderer;
        private SpriteRenderer _haloRenderer;
        private float _haloBaseDiameter;
        private float _haloElapsed;
        private ProjectileImpactRuntime _impact;
        private SpriteDefinition _visual;
        private EnemyAttackProfile _profile;
        private Vector2 _direction;
        private PlayerCharacterRuntime _target;
        private RunModel _run;
        private TrailRenderer _trail;
        private GameObjectPool<EnemyProjectileRuntime> _pool;
        private EnemyProjectileLifetime _lifetime;
        private bool _initialized;
        private bool _despawned;
        private bool _impactReleasePending;

        public bool IsActive => _initialized && !_despawned;
        public float RemainingSeconds => _lifetime?.RemainingSeconds ?? 0f;
        public EnemyAttackProfile Profile => _profile;
        public CombatSource Source { get; private set; }

        public void Initialize(
            EnemyAttackProfile profile,
            Vector2 direction,
            PlayerCharacterRuntime target,
            RunController runController,
            GameObjectPool<EnemyProjectileRuntime> pool = null,
            CombatSource source = default,
            SpriteDefinition visual = null)
        {
            if (profile == null) throw new System.ArgumentNullException(nameof(profile));
            if (runController == null || runController.Model == null) throw new System.ArgumentException("Projectile requires an initialized run.", nameof(runController));
            ClearState();
            _profile = profile;
            _run = runController.Model;
            _run.StateChanged += HandleRunState;
            _target = target;
            Source = source;
            _pool = pool;
            if (visual != null) visual.RequireRole(SpriteRole.Projectile);
            _visual = visual;
            _direction = direction.sqrMagnitude <= Mathf.Epsilon ? Vector2.right : direction.normalized;
            _lifetime = new EnemyProjectileLifetime(profile.ProjectileLifetimeSeconds);
            _despawned = false;
            _initialized = true;

            CacheComponents();
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
            _body.rotation = 0f;
            transform.localRotation = Quaternion.identity;
            if (_trail != null) { _trail.Clear(); _trail.emitting = true; }
            _renderer.flipX = _renderer.flipY = false;
            _body.bodyType = RigidbodyType2D.Kinematic;
            _body.gravityScale = 0f;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _collider.isTrigger = true;
            _collider.radius = profile.ProjectileRadius;
            _collider.enabled = true;
            transform.localScale = Vector3.one;
            _impactReleasePending = false;
            ConfigureVisual();
            gameObject.name = $"Enemy Projectile [{profile.Pattern}]";
            HandleRunState(_run.State);
        }

        private void FixedUpdate() => Tick(Time.fixedDeltaTime);

        public void Tick(float deltaTime)
        {
            Game.Content.NumericValidation.ValidateNonNegative(deltaTime, nameof(deltaTime));
            if (!_initialized || _run == null)
                return;
            var state = _run.State;
            if (_impactReleasePending)
            {
                if (state == RunState.Won || state == RunState.Lost || state == RunState.Stopped ||
                    _impact.Tick(deltaTime, state == RunState.Running)) ReleaseNow();
                return;
            }
            if (_despawned) return;
            if (state == RunState.Won || state == RunState.Lost || state == RunState.Stopped)
            {
                Despawn();
                return;
            }
            var running = state == RunState.Running;
            _body.linearVelocity = running ? _direction * _profile.ProjectileSpeed : Vector2.zero;
            if (running && _visualRenderer != null && _visualRenderer.enabled)
                _visualRenderer.transform.Rotate(0f, 0f,
                    _visual.ProjectilePresentation.SpinDegreesPerSecond * deltaTime);
            if (running && _haloRenderer != null && _haloRenderer.enabled)
            {
                _haloElapsed += deltaTime;
                var halo = _visual.ProjectilePresentation.ThreatHalo;
                _haloRenderer.transform.localScale = Vector3.one * (_haloBaseDiameter * halo.PulseFactor(_haloElapsed));
            }
            if (_lifetime.Tick(deltaTime, running))
            {
                if (_profile.Pattern == EnemyProjectilePattern.Explosive) ApplyImpact(_body.position);
                else Despawn();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_initialized || _despawned || _run?.State != RunState.Running || _target == null || other.gameObject != _target.gameObject)
                return;
            ApplyImpact(_body.position);
        }

        private void ApplyImpact(Vector2 impactPosition)
        {
            if (!IsActive || _run?.State != RunState.Running) return;
            // Return before callbacks. A lethal hit can end the run and immediately reuse a pool slot.
            var target = _target;
            var profile = _profile;
            var source = Source;
            var direction = _direction;
            var canHit = target != null && target.Health != null && !target.Health.IsDead;
            if (canHit && profile.Pattern == EnemyProjectilePattern.Explosive)
            {
                direction = (Vector2)target.transform.position - impactPosition;
                canHit = direction.sqrMagnitude <= profile.ExplosionRadius * profile.ExplosionRadius;
            }
            PlayImpact(impactPosition);
            if (_impact != null && _impact.IsPlaying) BeginImpactRelease();
            else Despawn();
            if (canHit) target.ApplyDamage(new CombatDamageRequest(source, profile.Damage, profile.Controls, direction.x, direction.y));
        }

        private void HandleRunState(RunState state)
        {
            // A prior run's multicast invocation may still contain this handler after pool reuse.
            if (_run == null || _run.State != state) return;
            if (state == RunState.Won || state == RunState.Lost || state == RunState.Stopped) Despawn();
            else if (state != RunState.Running && _body != null) _body.linearVelocity = Vector2.zero;
        }

        public void Despawn()
        {
            if (!_initialized) return;
            if (_impactReleasePending) { ReleaseNow(); return; }
            if (_despawned) return;
            _despawned = true;
            ReleaseNow();
        }

        private void BeginImpactRelease()
        {
            _despawned = true;
            _impactReleasePending = true;
            _body.linearVelocity = Vector2.zero;
            _collider.enabled = false;
            _renderer.enabled = false;
            if (_visualRenderer != null) _visualRenderer.enabled = false;
            if (_haloRenderer != null) _haloRenderer.enabled = false;
        }

        private void ReleaseNow()
        {
            var pool = _pool;
            ClearState();
            if (pool != null) { pool.Return(this); return; }
            if (Application.isPlaying) Destroy(gameObject);
            else DestroyImmediate(gameObject);
        }

        public void Shutdown() => Despawn();
        private void OnDisable() => ClearState();
        private void OnDestroy() => ClearState();

        private void ClearState()
        {
            if (_run != null) _run.StateChanged -= HandleRunState;
            _initialized = false;
            _despawned = true;
            if (_body != null) { _body.linearVelocity = Vector2.zero; _body.angularVelocity = 0f; }
            if (_collider != null) _collider.enabled = false;
            if (_renderer != null) _renderer.enabled = false;
            if (_visualRenderer != null) { _visualRenderer.enabled = false; _visualRenderer.sprite = null; }
            if (_haloRenderer != null) _haloRenderer.enabled = false;
            _haloElapsed = 0f;
            _impact?.ResetPresentation();
            if (_trail != null) { _trail.emitting = false; _trail.Clear(); }
            Source = default;
            _profile = null;
            _target = null;
            _run = null;
            _pool = null;
            _lifetime = null;
            _direction = Vector2.zero;
            _visual = null;
            _impactReleasePending = false;
        }

        private void PlayImpact(Vector2 position)
        {
            var profile = _visual?.ProjectilePresentation;
            if (profile == null) return;
            EnsurePresentationObjects();
            _impact.Play(profile, position);
        }

        private void ConfigureVisual()
        {
            if (_visual == null)
            {
                _renderer.sprite = PlaceholderSprite.Shared;
                _renderer.color = _profile.Pattern == EnemyProjectilePattern.Explosive
                    ? new Color(1f, .25f, .05f, 1f) : new Color(1f, .8f, .15f, 1f);
                _renderer.enabled = true;
                if (_visualRenderer != null) _visualRenderer.enabled = false;
                if (_haloRenderer != null) _haloRenderer.enabled = false;
                return;
            }
            EnsurePresentationObjects();
            _renderer.enabled = false;
            _visualRenderer.sprite = _visual.Sprite;
            _visualRenderer.color = Color.white;
            _visualRenderer.enabled = true;
            _visualRenderer.transform.localPosition = Vector3.zero;
            var size = _visual.Sprite.bounds.size;
            var diameter = _profile.ProjectileRadius * 2f * _visual.ProjectilePresentation.VisualScale;
            _visualRenderer.transform.localScale = Vector3.one * (diameter / Mathf.Max(size.x, size.y));
            var angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            _visualRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            ConfigureHalo(_visual.ProjectilePresentation.ThreatHalo);
        }

        private void ConfigureHalo(ProjectileThreatHaloProfile halo)
        {
            _haloElapsed = 0f;
            if (halo == null)
            {
                if (_haloRenderer != null) _haloRenderer.enabled = false;
                return;
            }
            if (_haloRenderer == null)
            {
                var haloObject = new GameObject("ThreatHalo");
                haloObject.transform.SetParent(transform, false);
                _haloRenderer = haloObject.AddComponent<SpriteRenderer>();
            }
            _haloRenderer.sprite = ProceduralShapeSprites.Disc;
            _haloRenderer.color = halo.Color;
            _haloRenderer.sortingLayerID = _visualRenderer.sortingLayerID;
            _haloRenderer.sortingOrder = _visualRenderer.sortingOrder - 1;
            _haloRenderer.transform.localPosition = Vector3.zero;
            _haloBaseDiameter = _profile.ProjectileRadius * 2f * halo.Scale;
            _haloRenderer.transform.localScale = Vector3.one * _haloBaseDiameter;
            _haloRenderer.enabled = true;
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
        }

        private void CacheComponents()
        {
            if (_body == null)
                _body = GetComponent<Rigidbody2D>();
            if (_collider == null)
                _collider = GetComponent<CircleCollider2D>();
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();
            if (_trail == null) _trail = GetComponent<TrailRenderer>();
        }
    }
}
