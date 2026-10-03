using System;
using System.Collections.Generic;
using Game.Diagnostics;
using Game.Zones;
using UnityEngine;

namespace Game.Traps
{
    /// <summary>
    /// Simulation of a field's traps (DECISION-0156). Pure C#: the scene driver ticks it only while the run is Running and
    /// draws what it exposes. Rules:
    /// <list type="bullet">
    /// <item>A turret works only while the player is within <c>RadiusScreenWidths</c> screen widths; leaving drops its rage,
    /// aborts a wind-up and cancels shots that have not left yet.</item>
    /// <item>While active the rage grows linearly to the maximum multiplier, shortening only the cooldown between volleys
    /// (never the telegraph or the delays inside a volley).</item>
    /// <item>A projectile ends when it hits the player, an obstacle or the arena edge, reaches its own range, returns to its
    /// trap, or leaves the same radius around the player. Projectiles ignore turret and barrel bodies.</item>
    /// <item>Traps only ever damage the player.</item>
    /// </list>
    /// </summary>
    public sealed class TrapRuntime
    {
        // A projectile moves in sub-steps of at most this length so a slow frame cannot skip over the player or an obstacle.
        private const float MaxSubstepLength = 0.25f;

        private readonly TrapLayoutDefinition _layout;
        private readonly TrapPlacementSet _set;
        private readonly ITrapPlayerTarget _player;
        private readonly IReadOnlyList<IReadOnlyList<Vector2>> _outlines;
        private readonly Rect[] _bounds;
        private readonly float _arenaHalf;
        private readonly List<TrapProjectile> _projectiles = new List<TrapProjectile>();
        private readonly Stack<TrapProjectile> _spare = new Stack<TrapProjectile>();

        public IReadOnlyList<TrapPlacement> Traps => _set.Traps;
        public IReadOnlyList<TrapBarrelPlacement> Barrels => _set.Barrels;
        public IReadOnlyList<TrapProjectile> Projectiles => _projectiles;
        public TrapLayoutDefinition Layout => _layout;
        /// <summary>Activation / cutoff radius in world units, updated every tick from the screen width.</summary>
        public float RadiusWorld { get; private set; }

        public TrapRuntime(TrapLayoutDefinition layout, TrapPlacementSet set, ITrapPlayerTarget player,
            IReadOnlyList<IReadOnlyList<Vector2>> obstacleOutlines, float arenaSideLength)
        {
            _layout = layout ?? throw new ArgumentNullException(nameof(layout));
            _set = set ?? throw new ArgumentNullException(nameof(set));
            _player = player ?? throw new ArgumentNullException(nameof(player));
            Game.Content.NumericValidation.ValidatePositive(arenaSideLength, nameof(arenaSideLength));
            _arenaHalf = arenaSideLength * .5f;
            _outlines = obstacleOutlines ?? Array.Empty<IReadOnlyList<Vector2>>();
            _bounds = new Rect[_outlines.Count];
            for (var i = 0; i < _outlines.Count; i++) _bounds[i] = BoundsOf(_outlines[i]);
        }

        /// <summary>Advances every trap, projectile and barrel by <paramref name="deltaSeconds"/> of running time.</summary>
        public void Tick(float deltaSeconds, float screenWidth)
        {
            Game.Content.NumericValidation.ValidateNonNegative(deltaSeconds, nameof(deltaSeconds));
            Game.Content.NumericValidation.ValidatePositive(screenWidth, nameof(screenWidth));
            using var guard = PerfGuard.Measure("Traps.Tick", 3f);
            RadiusWorld = _layout.RadiusScreenWidths * screenWidth;
            if (deltaSeconds <= 0f) return;
            var player = _player.Position;
            var alive = _player.IsAlive;
            foreach (var trap in _set.Traps) TickTrap(trap, deltaSeconds, player);
            TickProjectiles(deltaSeconds, player, alive);
            foreach (var barrel in _set.Barrels) TickBarrel(barrel, deltaSeconds, player, alive);
        }

        /// <summary>Removes every projectile, e.g. when the run ends.</summary>
        public void Clear()
        {
            for (var i = _projectiles.Count - 1; i >= 0; i--) Release(i);
            foreach (var trap in _set.Traps) Deactivate(trap);
        }

        private void TickTrap(TrapPlacement trap, float dt, Vector2 player)
        {
            trap.SecondsSinceVolley = Mathf.Min(trap.SecondsSinceVolley + dt, 999f);
            // A spinning head turns while the trap rests and stops for a volley (it fires along the pose it stopped in).
            if (trap.Type.SpinDegreesPerSecond != 0f && trap.State == TrapState.Idle)
                trap.HeadAngleDegrees = Mathf.Repeat(trap.HeadAngleDegrees + trap.Type.SpinDegreesPerSecond * dt, 360f);
            var radius = RadiusWorld;
            if ((trap.Center - player).sqrMagnitude > radius * radius)
            {
                if (trap.IsActive) Deactivate(trap);
                return;
            }
            trap.IsActive = true;
            trap.RageSeconds = Mathf.Min(trap.RageSeconds + dt, _layout.Rage.SecondsToMax);
            trap.RageProgress = _layout.Rage.Progress(trap.RageSeconds);
            trap.RageMultiplier = _layout.Rage.Multiplier(trap.RageSeconds);
            switch (trap.State)
            {
                case TrapState.Idle:
                    trap.CooldownRemaining -= dt * trap.RageMultiplier;
                    if (trap.CooldownRemaining <= 0f) BeginTelegraph(trap, player);
                    break;
                case TrapState.Telegraph:
                    trap.TelegraphRemaining -= dt;
                    if (trap.TelegraphRemaining <= 0f) Fire(trap);
                    break;
                case TrapState.Firing:
                    TickPending(trap, dt);
                    break;
            }
        }

        private static void Deactivate(TrapPlacement trap)
        {
            trap.IsActive = false;
            trap.RageSeconds = 0f;
            trap.RageProgress = 0f;
            trap.RageMultiplier = 1f;
            trap.Pending.Clear();
            // An aborted wind-up starts over on return; a running cooldown simply keeps what is left of it.
            if (trap.State != TrapState.Idle) trap.CooldownRemaining = 0f;
            trap.State = TrapState.Idle;
            trap.TelegraphRemaining = 0f;
        }

        private void BeginTelegraph(TrapPlacement trap, Vector2 player)
        {
            var type = trap.Type;
            var baseHeading = type.Heading == TrapHeadingMode.Aim
                ? Mathf.Atan2(player.y - trap.Center.y, player.x - trap.Center.x) * Mathf.Rad2Deg
                : trap.HeadAngleDegrees;
            trap.LockedHeadingDegrees = baseHeading + type.HeadingOffset(trap.VolleyIndex);
            trap.TelegraphRemaining = type.TelegraphSeconds;
            trap.State = TrapState.Telegraph;
        }

        private void Fire(TrapPlacement trap)
        {
            var type = trap.Type;
            trap.VolleyIndex++;
            trap.TelegraphRemaining = 0f;
            trap.SecondsSinceVolley = 0f;
            trap.LastShotAngleDegrees = 0f;
            // A volley that would overfill the scene budget is skipped whole rather than fired in part.
            if (_projectiles.Count + type.Shots.Count > _layout.MaxActiveProjectiles)
            {
                trap.State = TrapState.Idle;
                trap.CooldownRemaining = type.CooldownSeconds;
                return;
            }
            foreach (var shot in type.Shots)
            {
                if (shot.DelaySeconds <= 0f) Spawn(trap, shot, trap.LockedHeadingDegrees);
                else trap.Pending.Add(new TrapPendingShot { Shot = shot, HeadingDegrees = trap.LockedHeadingDegrees, DelayRemaining = shot.DelaySeconds });
            }
            if (trap.Pending.Count == 0)
            {
                trap.State = TrapState.Idle;
                trap.CooldownRemaining = type.CooldownSeconds;
            }
            else trap.State = TrapState.Firing;
        }

        private void TickPending(TrapPlacement trap, float dt)
        {
            for (var i = trap.Pending.Count - 1; i >= 0; i--)
            {
                var pending = trap.Pending[i];
                pending.DelayRemaining -= dt;
                if (pending.DelayRemaining > 0f)
                {
                    trap.Pending[i] = pending;
                    continue;
                }
                Spawn(trap, pending.Shot, pending.HeadingDegrees);
                trap.Pending.RemoveAt(i);
            }
            if (trap.Pending.Count > 0) return;
            trap.State = TrapState.Idle;
            trap.CooldownRemaining = trap.Type.CooldownSeconds;
        }

        /// <summary>
        /// Body radius as drawn: the model is scaled to the size of the projectile it fires, so shots leave from the muzzle of the model
        /// they are drawn with, not from a fixed distance that would put a small cannon's shots far from it.
        /// </summary>
        private float VisualBodyRadius(TrapPlacement trap) =>
            trap.Type.BodyRadius * (trap.ModelKey != null && _layout.Models.TryGetValue(trap.ModelKey, out var model) ? model.Scale : 1f);

        private void Spawn(TrapPlacement trap, TrapShot shot, float headingDegrees)
        {
            trap.LastShotAngleDegrees = shot.AngleDegrees;
            var degrees = headingDegrees + shot.AngleDegrees;
            var radians = degrees * Mathf.Deg2Rad;
            var direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            var sideways = new Vector2(-direction.y, direction.x);
            var projectile = _spare.Count > 0 ? _spare.Pop() : new TrapProjectile();
            projectile.Definition = shot.Projectile;
            projectile.Owner = trap;
            projectile.Position = trap.Center + sideways * shot.Lateral + direction * VisualBodyRadius(trap);
            projectile.Direction = direction;
            projectile.AgeSeconds = 0f;
            projectile.DistanceTraveled = 0f;
            projectile.Returning = false;
            projectile.SpinDegrees = 0f;
            projectile.FacingDegrees = degrees;
            _projectiles.Add(projectile);
        }

        private void TickProjectiles(float dt, Vector2 player, bool alive)
        {
            var radiusSqr = RadiusWorld * RadiusWorld;
            var hitRadius = _layout.PlayerHitRadius;
            for (var i = _projectiles.Count - 1; i >= 0; i--)
            {
                var projectile = _projectiles[i];
                var definition = projectile.Definition;
                projectile.AgeSeconds += dt;
                projectile.SpinDegrees += definition.SpinDegreesPerSecond * dt;
                if (definition.ReturnAfterSeconds > 0f && projectile.AgeSeconds >= definition.ReturnAfterSeconds)
                    projectile.Returning = true;
                var remaining = definition.Speed * dt;
                var substeps = Mathf.Max(1, Mathf.CeilToInt(remaining / MaxSubstepLength));
                var step = remaining / substeps;
                var ended = false;
                for (var s = 0; s < substeps && !ended; s++)
                {
                    if (projectile.Returning)
                    {
                        var toOwner = projectile.Owner.Center - projectile.Position;
                        if (toOwner.magnitude <= step + VisualBodyRadius(projectile.Owner)) { ended = true; break; }
                        projectile.Direction = toOwner.normalized;
                        projectile.FacingDegrees = Mathf.Atan2(projectile.Direction.y, projectile.Direction.x) * Mathf.Rad2Deg;
                    }
                    projectile.Position += projectile.Direction * step;
                    projectile.DistanceTraveled += step;
                    if (alive && (projectile.Position - player).sqrMagnitude <= (definition.Radius + hitRadius) * (definition.Radius + hitRadius))
                    {
                        _player.Hit(definition.Damage, projectile.Owner.Type.Id);
                        ended = true;
                    }
                    else if (definition.MaxDistance > 0f && projectile.DistanceTraveled >= definition.MaxDistance) ended = true;
                    else if (Mathf.Abs(projectile.Position.x) > _arenaHalf || Mathf.Abs(projectile.Position.y) > _arenaHalf) ended = true;
                    else if (HitsObstacle(projectile.Position)) ended = true;
                }
                if (!ended && (projectile.Position - player).sqrMagnitude > radiusSqr) ended = true;
                if (ended) Release(i);
            }
        }

        private void TickBarrel(TrapBarrelPlacement barrel, float dt, Vector2 player, bool alive)
        {
            var spec = _layout.Barrels;
            switch (barrel.State)
            {
                case TrapBarrelState.Intact:
                    if (!barrel.Explosive || !alive) return;
                    if ((barrel.Center - player).sqrMagnitude > spec.TriggerRadius * spec.TriggerRadius) return;
                    barrel.State = TrapBarrelState.Fusing;
                    barrel.FuseRemaining = spec.FuseSeconds;
                    break;
                case TrapBarrelState.Fusing:
                    barrel.FuseRemaining -= dt;
                    if (barrel.FuseRemaining > 0f) return;
                    barrel.State = TrapBarrelState.Exploded;
                    barrel.SecondsSinceExplosion = 0f;
                    var reach = spec.BlastRadius + _layout.PlayerHitRadius;
                    if (alive && (barrel.Center - player).sqrMagnitude <= reach * reach) _player.Hit(spec.Damage, spec.Id);
                    break;
                case TrapBarrelState.Exploded:
                    barrel.SecondsSinceExplosion += dt;
                    break;
            }
        }

        private bool HitsObstacle(Vector2 point)
        {
            for (var i = 0; i < _bounds.Length; i++)
                if (_bounds[i].Contains(point) && ZoneGeometry.Contains(_outlines[i], point)) return true;
            return false;
        }

        private void Release(int index)
        {
            var projectile = _projectiles[index];
            var last = _projectiles.Count - 1;
            _projectiles[index] = _projectiles[last];
            _projectiles.RemoveAt(last);
            projectile.Definition = null;
            projectile.Owner = null;
            _spare.Push(projectile);
        }

        private static Rect BoundsOf(IReadOnlyList<Vector2> outline)
        {
            if (outline == null || outline.Count == 0) return default;
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            foreach (var point in outline)
            {
                minX = Mathf.Min(minX, point.x); maxX = Mathf.Max(maxX, point.x);
                minY = Mathf.Min(minY, point.y); maxY = Mathf.Max(maxY, point.y);
            }
            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }
    }
}
