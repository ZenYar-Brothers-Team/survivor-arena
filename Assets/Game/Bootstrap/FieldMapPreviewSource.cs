using System;
using System.Collections.Generic;
using Game.Presentation;
using Game.UI;
using Game.Zones;
using Game.Diagnostics;
using UnityEngine;

namespace Game.Bootstrap
{
    /// <summary>
    /// Development map preview data for the running field: the arena rectangle, this run's road network (FIELD-003,
    /// DECISION-0137) or platform network (FIELD-009), the outline of every player-only obstacle collider the field art created, and the gameplay camera
    /// frame. Obstacles and roads are read once on first use.
    /// </summary>
    public sealed class FieldMapPreviewSource : IMapPreviewSource
    {
        private const int CircleSegments = 16;

        private readonly FieldEnvironmentArtRuntime _art;
        private readonly Func<Rect> _arena;
        private readonly Camera _camera;
        private List<Vector2[]> _obstacles;
        private List<MapPreviewRoad> _roads;
        private Rect? _arenaRect;
        private readonly ZoneRuntime _zones;
        private readonly List<ZonePlacement> _altarZones = new List<ZonePlacement>();
        private readonly List<MapPreviewAltar> _altars = new List<MapPreviewAltar>();

        public FieldMapPreviewSource(FieldEnvironmentArtRuntime art, Func<Rect> arena, Camera camera, ZoneRuntime zones = null)
        {
            _art = art ?? throw new ArgumentNullException(nameof(art));
            _arena = arena ?? throw new ArgumentNullException(nameof(arena));
            _camera = camera;
            _zones = zones;
            if (zones != null)
                foreach (var zone in zones.Zones)
                    if (zone.Effect.IsAltar)
                    {
                        _altarZones.Add(zone);
                        _altars.Add(default);
                    }
        }

        public IReadOnlyList<MapPreviewAltar> Altars
        {
            get
            {
                using var guard = PerfGuard.Measure("Zones.MapPreview", 2f);
                for (var i = 0; i < _altarZones.Count; i++)
                {
                    var zone = _altarZones[i];
                    _altars[i] = new MapPreviewAltar(zone.Center, zone.Radius, zone.Effect.Color,
                        zone.Effect.Polarity == ZoneAltarPolarity.Negative, zone.IsActive(_zones.Time));
                }
                return _altars;
            }
        }

        public Rect Arena => _arenaRect ??= _arena();

        public IReadOnlyList<Vector2[]> Obstacles => _obstacles ??= Outlines(_art.ObstacleColliders);

        public IReadOnlyList<MapPreviewRoad> Roads => _roads ??= Pieces();

        private List<MapPreviewRoad> Pieces()
        {
            var result = RoadPieces(_art.RoadLayout);
            result.AddRange(PlatformPieces(_art.PlatformLayout));
            return result;
        }

        public Rect View
        {
            get
            {
                if (_camera == null) return default;
                var height = _camera.orthographicSize * 2f;
                var width = height * _camera.aspect;
                var center = _camera.transform.position;
                return new Rect(center.x - width * .5f, center.y - height * .5f, width, height);
            }
        }

        /// <summary>
        /// Road pieces in drawing order at the profile's widths and colors: dead-end corridors, main road centerlines over
        /// them (the main road keeps each junction, as on the field), then the round ends. No layout yields no roads.
        /// </summary>
        public static List<MapPreviewRoad> RoadPieces(FieldRoadLayout layout)
        {
            var result = new List<MapPreviewRoad>();
            if (layout == null) return result;
            var profile = layout.Profile;
            foreach (var branch in layout.DeadEnds)
                result.Add(new MapPreviewRoad(new[] { branch.Entrance, branch.EndCenter }, profile.DeadEndWidth, profile.DeadEndColor));
            foreach (var road in layout.Roads) result.Add(new MapPreviewRoad(road, profile.MainRoadWidth, profile.MainColor));
            foreach (var branch in layout.DeadEnds)
                result.Add(new MapPreviewRoad(new[] { branch.EndCenter }, profile.DeadEndEndRadius * 2f, profile.DeadEndColor));
            return result;
        }

        /// <summary>
        /// Platform field pieces in drawing order: bridges at the bridge width, then each platform as a round dot of its
        /// own diameter (the start platform in its own color). No layout yields nothing.
        /// </summary>
        public static List<MapPreviewRoad> PlatformPieces(FieldPlatformLayout layout)
        {
            var result = new List<MapPreviewRoad>();
            if (layout == null) return result;
            var profile = layout.Profile;
            foreach (var bridge in layout.Bridges)
                result.Add(new MapPreviewRoad(new[] { layout.Platforms[bridge.From].Center, layout.Platforms[bridge.To].Center },
                    profile.BridgeWidth, profile.BridgeColor));
            for (var i = 0; i < layout.Platforms.Count; i++)
                result.Add(new MapPreviewRoad(new[] { layout.Platforms[i].Center }, layout.Platforms[i].Radius * 2f,
                    i == layout.StartIndex ? profile.StartPlatformColor : profile.PlatformColor));
            return result;
        }

        /// <summary>World-space outlines of obstacle colliders: polygons as authored, circles as 16-gons, anything else as its bounds.</summary>
        public static List<Vector2[]> Outlines(IEnumerable<Collider2D> colliders)
        {
            var result = new List<Vector2[]>();
            foreach (var collider in colliders)
            {
                if (collider == null || !collider.enabled) continue;
                switch (collider)
                {
                    case PolygonCollider2D polygon:
                        for (var path = 0; path < polygon.pathCount; path++)
                        {
                            var local = polygon.GetPath(path);
                            var world = new Vector2[local.Length];
                            for (var i = 0; i < local.Length; i++) world[i] = polygon.transform.TransformPoint(local[i]);
                            result.Add(world);
                        }
                        break;
                    case CircleCollider2D circle:
                        var bounds = circle.bounds;
                        var ring = new Vector2[CircleSegments];
                        for (var i = 0; i < CircleSegments; i++)
                        {
                            var angle = 2f * Mathf.PI * i / CircleSegments;
                            ring[i] = new Vector2(bounds.center.x + Mathf.Cos(angle) * bounds.extents.x,
                                bounds.center.y + Mathf.Sin(angle) * bounds.extents.y);
                        }
                        result.Add(ring);
                        break;
                    default:
                        var box = collider.bounds;
                        result.Add(new[]
                        {
                            new Vector2(box.min.x, box.min.y), new Vector2(box.max.x, box.min.y),
                            new Vector2(box.max.x, box.max.y), new Vector2(box.min.x, box.max.y)
                        });
                        break;
                }
            }
            return result;
        }
    }
}
