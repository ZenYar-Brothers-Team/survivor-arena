using System;
using System.Collections.Generic;
using Game.Content;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// Pure state of one boss life's zones, beams and summon markers (Game Design «Враги, волны, элиты и боссы»,
    /// DECISION-0066). <see cref="Start"/> places a special; <see cref="Tick"/> advances running time only and reports
    /// the player hits and summon spawns of that tick; <see cref="Visuals"/> describes what to draw.
    /// Rules: a zone hits once when its fill completes and only inside its circle (safe circles: only outside every
    /// circle); burning ground hits every tick while the player stands in it; a beam hits at most once per activation and
    /// only while active; summons never exceed the alive limit counting markers still pending.
    /// </summary>
    public sealed class BossHazardField
    {
        /// <summary>Visual radius meaning "the whole view" (safe-circles danger area).</summary>
        public const float FullViewRadius = -1f;
        private const int PlacementAttempts = 30;
        private readonly System.Random _random;
        private readonly List<BossZoneInstance> _zones = new List<BossZoneInstance>();
        private readonly List<BossBeamInstance> _beams = new List<BossBeamInstance>();
        private readonly List<BossSummonCall> _calls = new List<BossSummonCall>();
        private readonly List<BossHazardHit> _hits = new List<BossHazardHit>();
        private readonly List<BossSummonSpawn> _spawns = new List<BossSummonSpawn>();
        private readonly List<BossHazardVisual> _visuals = new List<BossHazardVisual>();
        private Vector2 _boss;

        public BossHazardField(System.Random random) => _random = random ?? throw new ArgumentNullException(nameof(random));

        /// <summary>Player hits of the last <see cref="Tick"/>.</summary>
        public IReadOnlyList<BossHazardHit> Hits => _hits;
        /// <summary>Summoned enemies released by the last <see cref="Tick"/>.</summary>
        public IReadOnlyList<BossSummonSpawn> Spawns => _spawns;
        public IReadOnlyList<BossHazardVisual> Visuals => _visuals;
        public bool IsEmpty => _zones.Count == 0 && _beams.Count == 0 && _calls.Count == 0;
        public int ActiveZones => _zones.Count;
        public int ActiveBeams => _beams.Count;

        /// <summary>Markers of <paramref name="profile"/> not yet released.</summary>
        public int PendingSummons(BossSummonProfile profile)
        {
            var count = 0;
            foreach (var call in _calls)
                if (call.Profile == profile) count += call.Positions.Length;
            return count;
        }

        /// <param name="aliveSummons">This boss life's living summons of the same profile (alive limit).</param>
        public void Start(BossSpecialRequest request, Vector2 boss, Vector2 player, int aliveSummons = 0)
        {
            _boss = boss;
            switch (request.Kind)
            {
                case BossSpecialKind.Zone: StartZone(request.Zone, request.SourceId, boss, player); break;
                case BossSpecialKind.Beam: StartBeam(request.Beam, request.SourceId, boss, player); break;
                case BossSpecialKind.Summon: StartSummon(request.Summon, player, aliveSummons); break;
                default: throw new ArgumentOutOfRangeException(nameof(request));
            }
            RebuildVisuals();
        }

        public void Tick(float deltaTime, bool isRunning, Vector2 boss, Vector2 player)
        {
            NumericValidation.ValidateNonNegative(deltaTime, nameof(deltaTime));
            _hits.Clear();
            _spawns.Clear();
            if (!isRunning) return;
            _boss = boss;
            for (var i = 0; i < _zones.Count; i++)
                if (TickZone(_zones[i], deltaTime, boss, player)) _zones.RemoveAt(i--);
            for (var i = 0; i < _beams.Count; i++)
                if (TickBeam(_beams[i], deltaTime, boss, player)) _beams.RemoveAt(i--);
            for (var i = 0; i < _calls.Count; i++)
            {
                var call = _calls[i];
                call.Remaining -= deltaTime;
                if (call.Remaining > 0f) continue;
                foreach (var position in call.Positions) _spawns.Add(new BossSummonSpawn(call.Profile, position));
                _calls.RemoveAt(i--);
            }
            RebuildVisuals();
        }

        /// <summary>Drops every zone, beam and marker (boss death, run end).</summary>
        public void Clear()
        {
            _zones.Clear();
            _beams.Clear();
            _calls.Clear();
            _hits.Clear();
            _spawns.Clear();
            _visuals.Clear();
        }

        private void StartZone(BossZoneProfile zone, ContentId source, Vector2 boss, Vector2 player)
        {
            switch (zone.Placement)
            {
                case BossZonePlacement.AtPlayer: AddZone(zone, source, new[] { player }, 0f); break;
                case BossZonePlacement.AroundSelf: AddZone(zone, source, new[] { boss }, 0f); break;
                case BossZonePlacement.AroundPlayer:
                    foreach (var center in Scatter(player, zone.Count, zone.ScatterRadius, zone.MinSpacing))
                        AddZone(zone, source, new[] { center }, 0f);
                    break;
                case BossZonePlacement.SafeCircles:
                    AddZone(zone, source, Scatter(player, zone.Count, zone.ScatterRadius, zone.MinSpacing), 0f);
                    break;
                case BossZonePlacement.Trail:
                    // Each circle lands where the player is when it appears, so the bombing follows the path.
                    for (var i = 0; i < zone.Count; i++)
                        AddZone(zone, source, i == 0 ? new[] { player } : null, zone.IntervalSeconds * i);
                    break;
            }
        }

        private void AddZone(BossZoneProfile zone, ContentId source, Vector2[] centers, float wait) =>
            _zones.Add(new BossZoneInstance
            {
                Profile = zone, SourceId = source, Centers = centers, WaitRemaining = wait,
                Stage = wait > 0f ? BossZoneStage.Waiting : BossZoneStage.Filling
            });

        private Vector2[] Scatter(Vector2 around, int count, float radius, float spacing)
        {
            var points = new Vector2[count];
            for (var i = 0; i < count; i++)
            {
                var candidate = around;
                for (var attempt = 0; attempt < PlacementAttempts; attempt++)
                {
                    var angle = (float)(_random.NextDouble() * Math.PI * 2d);
                    var distance = radius * Mathf.Sqrt((float)_random.NextDouble());
                    candidate = around + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                    if (FarFromAll(candidate, points, i, spacing)) break;
                }
                points[i] = candidate;
            }
            return points;
        }

        private static bool FarFromAll(Vector2 candidate, Vector2[] points, int count, float spacing)
        {
            for (var j = 0; j < count; j++)
                if ((points[j] - candidate).sqrMagnitude < spacing * spacing) return false;
            return true;
        }

        /// <returns>True when the zone is finished and can be removed.</returns>
        private bool TickZone(BossZoneInstance zone, float deltaTime, Vector2 boss, Vector2 player)
        {
            var profile = zone.Profile;
            if (zone.Stage == BossZoneStage.Waiting)
            {
                zone.WaitRemaining -= deltaTime;
                if (zone.WaitRemaining > 0f) return false;
                zone.Centers = new[] { player };
                zone.Stage = BossZoneStage.Filling;
                deltaTime = -zone.WaitRemaining;
            }
            if (zone.Stage == BossZoneStage.Filling)
            {
                zone.FillElapsed += deltaTime;
                if (zone.FillElapsed < profile.FillSeconds) return false;
                deltaTime = zone.FillElapsed - profile.FillSeconds;
                zone.Stage = BossZoneStage.Settling;
                zone.SettleElapsed = 0f;
                zone.NextBurnAt = profile.LingerTickSeconds;
                Impact(zone, boss, player);
            }
            zone.SettleElapsed += deltaTime;
            while (profile.LingerSeconds > 0f && zone.NextBurnAt <= zone.SettleElapsed && zone.NextBurnAt <= profile.LingerSeconds + 1e-4f)
            {
                if (Inside(zone.Centers[0], profile.Radius, player))
                    _hits.Add(new BossHazardHit(profile.LingerDamagePerSecond * profile.LingerTickSeconds,
                        null, player - zone.Centers[0], zone.SourceId, BossSpecialKind.Zone));
                zone.NextBurnAt += profile.LingerTickSeconds;
            }
            return zone.SettleElapsed >= Mathf.Max(profile.ImpactEffectSeconds, profile.LingerSeconds);
        }

        private void Impact(BossZoneInstance zone, Vector2 boss, Vector2 player)
        {
            var profile = zone.Profile;
            if (profile.Placement == BossZonePlacement.SafeCircles)
            {
                foreach (var center in zone.Centers)
                    if (Inside(center, profile.Radius, player)) return;
                _hits.Add(new BossHazardHit(profile.Damage, profile.Controls, player - boss, zone.SourceId, BossSpecialKind.Zone));
                return;
            }
            if (Inside(zone.Centers[0], profile.Radius, player))
                _hits.Add(new BossHazardHit(profile.Damage, profile.Controls, player - zone.Centers[0], zone.SourceId, BossSpecialKind.Zone));
        }

        private static bool Inside(Vector2 center, float radius, Vector2 point) => (point - center).sqrMagnitude <= radius * radius;

        private void StartBeam(BossBeamProfile beam, ContentId source, Vector2 boss, Vector2 player)
        {
            var toward = player - boss;
            _beams.Add(new BossBeamInstance
            {
                Profile = beam, SourceId = source,
                BaseAngleDegrees = toward.sqrMagnitude > Mathf.Epsilon ? Mathf.Atan2(toward.y, toward.x) * Mathf.Rad2Deg : 0f
            });
        }

        /// <returns>True when the beam attack is over.</returns>
        private bool TickBeam(BossBeamInstance beam, float deltaTime, Vector2 boss, Vector2 player)
        {
            var profile = beam.Profile;
            beam.Elapsed += deltaTime;
            if (!beam.Active)
            {
                if (beam.Elapsed < profile.TelegraphSeconds) return false;
                beam.Active = true;
                beam.Elapsed -= profile.TelegraphSeconds;
            }
            var sweep = profile.SweepDegrees * Mathf.Clamp01(beam.Elapsed / profile.ActiveSeconds);
            if (!beam.HasHit)
            {
                foreach (var angle in profile.AnglesDegrees)
                {
                    var direction = Direction(beam.BaseAngleDegrees + angle + sweep);
                    var along = Mathf.Clamp(Vector2.Dot(player - boss, direction), 0f, profile.Length);
                    var closest = boss + direction * along;
                    if ((player - closest).sqrMagnitude > profile.Width * profile.Width * .25f) continue;
                    var push = player - closest;
                    _hits.Add(new BossHazardHit(profile.Damage, profile.Controls,
                        push.sqrMagnitude > Mathf.Epsilon ? push : new Vector2(-direction.y, direction.x), beam.SourceId, BossSpecialKind.Beam));
                    beam.HasHit = true;
                    break;
                }
            }
            return beam.Elapsed >= profile.ActiveSeconds;
        }

        private static Vector2 Direction(float degrees)
        {
            var radians = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        private void StartSummon(BossSummonProfile summon, Vector2 player, int aliveSummons)
        {
            var count = Mathf.Min(summon.Count, summon.MaxAlive - aliveSummons - PendingSummons(summon));
            if (count <= 0) return;
            var positions = new Vector2[count];
            var start = _random.NextDouble() * 360d;
            for (var i = 0; i < count; i++)
                positions[i] = player + Direction((float)(start + 360d * i / count)) * summon.SpawnDistance;
            _calls.Add(new BossSummonCall { Profile = summon, Positions = positions, Remaining = summon.TelegraphSeconds });
        }

        private void RebuildVisuals()
        {
            _visuals.Clear();
            foreach (var zone in _zones) AddZoneVisuals(zone);
            foreach (var beam in _beams)
            {
                var profile = beam.Profile;
                var progress = beam.Active ? beam.Elapsed / profile.ActiveSeconds : beam.Elapsed / profile.TelegraphSeconds;
                var sweep = beam.Active ? profile.SweepDegrees * Mathf.Clamp01(progress) : 0f;
                foreach (var angle in profile.AnglesDegrees)
                    _visuals.Add(new BossHazardVisual(beam.Active ? BossHazardVisualKind.BeamActive : BossHazardVisualKind.BeamTelegraph,
                        _boss, 0f, progress, beam.Active ? profile.BeamColor : profile.TelegraphColor,
                        beam.BaseAngleDegrees + angle + sweep, profile.Length, profile.Width));
            }
            foreach (var call in _calls)
            {
                var progress = 1f - call.Remaining / call.Profile.TelegraphSeconds;
                foreach (var position in call.Positions)
                    _visuals.Add(new BossHazardVisual(BossHazardVisualKind.SummonMarker, position,
                        call.Profile.Enemy.CollisionSize * .5f, progress, call.Profile.MarkerColor));
            }
        }

        private void AddZoneVisuals(BossZoneInstance zone)
        {
            var profile = zone.Profile;
            if (zone.Stage == BossZoneStage.Waiting) return;
            if (zone.Stage == BossZoneStage.Filling)
            {
                var progress = zone.FillElapsed / profile.FillSeconds;
                if (profile.Placement == BossZonePlacement.SafeCircles)
                {
                    _visuals.Add(new BossHazardVisual(BossHazardVisualKind.DangerWash, zone.Centers[0], FullViewRadius, progress,
                        profile.TelegraphColor));
                    foreach (var center in zone.Centers)
                        _visuals.Add(new BossHazardVisual(BossHazardVisualKind.SafeCircle, center, profile.Radius, progress, profile.ImpactColor));
                    return;
                }
                _visuals.Add(new BossHazardVisual(BossHazardVisualKind.ZoneEdge, zone.Centers[0], profile.Radius, progress, profile.TelegraphColor));
                _visuals.Add(new BossHazardVisual(BossHazardVisualKind.ZoneFill, zone.Centers[0], profile.Radius, progress, profile.TelegraphColor));
                return;
            }
            if (zone.SettleElapsed < profile.ImpactEffectSeconds)
            {
                var progress = zone.SettleElapsed / profile.ImpactEffectSeconds;
                var center = profile.Placement == BossZonePlacement.SafeCircles ? _boss : zone.Centers[0];
                var radius = profile.Placement == BossZonePlacement.SafeCircles ? profile.ScatterRadius + profile.Radius : profile.Radius;
                _visuals.Add(new BossHazardVisual(BossHazardVisualKind.ZoneImpact, center, radius, progress, profile.ImpactColor));
            }
            if (profile.LingerSeconds > 0f && zone.SettleElapsed < profile.LingerSeconds)
                _visuals.Add(new BossHazardVisual(BossHazardVisualKind.Burning, zone.Centers[0], profile.Radius,
                    zone.SettleElapsed / profile.LingerSeconds, profile.ImpactColor));
        }
    }
}
