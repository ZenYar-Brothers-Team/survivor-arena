using System;
using System.Collections.Generic;
using Game.Character;
using Game.Content;
using Game.Diagnostics;
using Game.Enemy;
using Game.Pooling;
using Game.Run;
using UnityEngine;
using Game.Presentation;
namespace Game.Pickup
{
    /// <summary>Single main-thread owner: expiry before contact, stable spawn-sequence order, no collection during draft pause.</summary>
    public sealed class WorldPickupRuntime : MonoBehaviour, IPickupRuntime, IEnemyLifecycleSink
    {
        private readonly List<WorldPickupVisual> _active = new List<WorldPickupVisual>();
        private readonly HashSet<Guid> _rolledLives = new HashSet<Guid>();
        private GameObjectPool<WorldPickupVisual> _pool;
        private FixturePickupCatalog _catalog;
        private RunModel _run;
        private PlayerCharacterRuntime _player;
        private Collider2D _playerCollider;
        private IPickupRewardTarget _target;
        private IPickupPlacement _placement;
        private ContentId _field;
        private System.Random _random;
        private System.Random _scatterRandom;
        private IReadOnlyDictionary<ContentId, SpriteDefinition> _visuals;
        private long _sequence;
        private int _spawned, _collected, _expired, _cancelled, _rejected;
        private string _feedback = "";
        private float _feedbackRemaining;
        private bool _ticking;
        public bool IsInitialized => _run != null;
        public event Action Changed;
        public event Action<PickupEvent> Resolved;
        public event Action<PickupEvent> Spawned;
        public PickupSnapshot Snapshot => new PickupSnapshot(_spawned, _collected, _expired, _cancelled, _rejected, _active.Count, _feedback);
        public int InactiveCount => _pool?.InactiveCount ?? 0;
        public void Initialize(FixturePickupCatalog catalog, RunModel run, PlayerCharacterRuntime player,
            IPickupRewardTarget target, IPickupPlacement placement, ContentId field,
            IReadOnlyDictionary<ContentId, SpriteDefinition> visuals = null)
        {
            if (catalog == null || run == null || player == null || player.Health == null || target == null || placement == null || !field.IsValid)
                throw new ArgumentException("Pickup runtime requires initialized dependencies.");
            var collider = player.GetComponent<Collider2D>();
            if (collider == null) throw new ArgumentException("Pickup target requires contact geometry.");
            Shutdown();
            _catalog = catalog; _run = run; _player = player; _playerCollider = collider; _target = target; _placement = placement; _field = field;
            _random = new System.Random(catalog.Seed);
            _scatterRandom = new System.Random(catalog.DropScatterSeed);
            _visuals = visuals;
            _pool ??= new GameObjectPool<WorldPickupVisual>(WorldPickupVisual.CreateInstance, transform);
            _run.StateChanged += HandleState;
        }
        public WorldPickupVisual Spawn(PickupDefinition definition, Vector2 position, Guid? sourceLifeId = null, ContentId? sourceContentId = null)
        {
            if (_run == null || _run.State != RunState.Running || _player.Health == null || _player.Health.IsDead) return null;
            var scattered = Scatter(position, _catalog.DropScatterRadius, _scatterRandom);
            if (!_placement.TryPlace(scattered, out var reachable)) { _rejected++; Changed?.Invoke(); return null; }
            var identity = new PickupIdentity(Guid.NewGuid(), _run.RunId, _sequence++, sourceLifeId, sourceContentId);
            var life = new PickupLife(definition, identity);
            var visual = _pool.Rent();
            visual.transform.SetParent(transform, false);
            SpriteDefinition sprite = null;
            if (definition.Visual.Id.IsValid)
            {
                if (_visuals != null && !_visuals.TryGetValue(definition.Id, out sprite))
                    throw new InvalidOperationException($"Pickup '{definition.Id}' is missing its resolved visual.");
                sprite?.RequireRole(SpriteRole.Pickup);
            }
            visual.Initialize(life, reachable, sprite);
            _active.Add(visual); _spawned++;
            Spawned?.Invoke(new PickupEvent(life, reachable)); Changed?.Invoke();
            return visual;
        }
        public void OnEnemyLifeEvent(EnemyLifeEvent snapshot)
        {
            if (_run == null || _run.State != RunState.Running || snapshot.RunId != _run.RunId ||
                snapshot.Kind != EnemyLifeEventKind.Died || snapshot.Category != EnemyCategory.Ordinary || !_rolledLives.Add(snapshot.LifeId)) return;
            var chance = _catalog.Chance(snapshot.ContentId, _field, _player.Stats.PotionDropMultiplier);
            if (PotionDropPolicy.Roll(chance, _random.NextDouble())) Spawn(_catalog.Potion, snapshot.Position, snapshot.LifeId, snapshot.ContentId);
        }
        public void DropDevelopmentPickup(PickupRewardKind kind)
        {
            if ((!Application.isEditor && !Debug.isDebugBuild) || _player == null) return;
            Spawn(kind == PickupRewardKind.Potion ? _catalog.Potion : _catalog.Book, _player.transform.position);
        }
        private void LateUpdate() => Tick(Time.deltaTime);
        public void Tick(float deltaTime)
        {
            NumericValidation.ValidateNonNegative(deltaTime, nameof(deltaTime));
            if (_ticking || _run == null || _run.State != RunState.Running) return;
            using var guard = PerfGuard.Measure("Pickup.Tick", 3f);
            _ticking = true;
            try
            {
                if (_feedbackRemaining > 0)
                {
                    _feedbackRemaining = Mathf.Max(0, _feedbackRemaining - deltaTime);
                    if (_feedbackRemaining == 0) { _feedback = ""; Changed?.Invoke(); }
                }
                // Capture life tokens as well as objects: callbacks can return/rent the same pooled visual.
                var batch = new List<(WorldPickupVisual visual, PickupLife life)>(_active.Count);
                foreach (var visual in _active) batch.Add((visual, visual.Life));
                foreach (var item in batch)
                {
                    if (_run == null || _run.State != RunState.Running) break;
                    if (item.visual == null || !ReferenceEquals(item.visual.Life, item.life)) continue;
                    item.visual.TickPresentation(deltaTime);
                    if (item.life.Tick(deltaTime, true)) { _expired++; Finish(item.visual, item.life); continue; }
                    TryCollect(item.visual, item.life.Identity.DropId);
                }
            }
            finally { _ticking = false; }
        }
        public bool TryCollect(WorldPickupVisual visual, Guid expectedDropId)
        {
            if (visual == null || !_active.Contains(visual) || visual.Life == null || visual.Life.Identity.DropId != expectedDropId ||
                !_target.CanCollect(visual.Life.Identity) || !_playerCollider.enabled) return false;
            var life = visual.Life;
            var ownerRun = _run;
            var position = (Vector2)visual.transform.position;
            if ((position - _playerCollider.ClosestPoint(position)).sqrMagnitude > life.Definition.ContactRadius * life.Definition.ContactRadius) return false;
            var feedbackSeconds = _catalog.FeedbackSeconds;
            PickupRewardResult result;
            try { result = life.TryCollect(_target); }
            catch { if (ReferenceEquals(visual.Life, life)) Finish(visual, life); throw; }
            if (!result.Accepted) return false;
            if (!ReferenceEquals(_run, ownerRun)) return true;
            _collected++;
            _feedback = life.Definition.Kind == PickupRewardKind.Potion ? $"Potion +{result.Healing.Actual:0.#} HP" : "Book collected";
            _feedbackRemaining = feedbackSeconds;
            Finish(visual, life, result.Healing, position);
            return true;
        }
        private void Finish(WorldPickupVisual visual, PickupLife life, Game.Combat.HealthChange healing = default, Vector2? capturedPosition = null)
        {
            var snapshot = new PickupEvent(life, capturedPosition ?? (Vector2)visual.transform.position, healing);
            if (ReferenceEquals(visual.Life, life)) { _active.Remove(visual); visual.Shutdown(); _pool.Return(visual); }
            Resolved?.Invoke(snapshot); Changed?.Invoke();
        }
        private void HandleState(RunState state)
        { if (state == RunState.Won || state == RunState.Lost || state == RunState.Stopped) Clear(); }
        private void Clear()
        {
            foreach (var visual in _active.ToArray())
            {
                var life = visual.Life;
                // A reward in progress owns its eventual result, even if its callbacks end the run.
                if (life.State == PickupLifeState.Claiming) continue;
                life.Cancel(); _cancelled++; Finish(visual, life);
            }
            _feedback = ""; _feedbackRemaining = 0;
        }
        public void Shutdown()
        {
            if (_run != null) _run.StateChanged -= HandleState;
            Clear();
            // Shutdown may be requested from inside a reward callback. Detach the old claimed visual
            // without reporting a second terminal result; life identity guards the returning callback.
            foreach (var visual in _active.ToArray())
            { visual.Life.Cancel(); _active.Remove(visual); visual.Shutdown(); _pool.Return(visual); }
            _run = null; _player = null; _playerCollider = null; _target = null; _placement = null; _catalog = null;
            _visuals = null; _scatterRandom = null;
            _rolledLives.Clear(); _sequence = 0; _spawned = _collected = _expired = _cancelled = _rejected = 0;
        }
        private void OnDestroy() => Shutdown();

        private static Vector2 Scatter(Vector2 origin, float radius, System.Random random)
        {
            if (radius <= 0f) return origin;
            var angle = random.NextDouble() * Math.PI * 2d;
            var distance = Math.Sqrt(random.NextDouble()) * radius;
            return origin + new Vector2((float)(Math.Cos(angle) * distance), (float)(Math.Sin(angle) * distance));
        }
    }
}
