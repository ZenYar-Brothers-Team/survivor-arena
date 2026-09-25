using System;
using System.Collections.Generic;
using Game.Content;
using Game.Movement;
using Game.Pooling;
using Game.Presentation;
using UnityEngine;

namespace Game.ActiveSkill
{
    /// <summary>
    /// Pooled procedural world shapes for skills without projectile sprites (expanding pulse,
    /// lightning arcs, strike telegraphs). Owns only presentation; gameplay sizes are passed in.
    /// Time advances only while the run is running; <see cref="Clear"/> returns every renderer.
    /// </summary>
    internal sealed class SkillWorldEffectPresenter : IDisposable
    {
        private const int SortingOrder = 5;

        private sealed class Shape
        {
            public SpriteRenderer Renderer;
            public Color Color;
            public float Remaining;
            public float Fade;
            public bool Held;
        }

        private readonly IReadOnlyDictionary<ContentId, SkillWorldEffectProfile> _profiles;
        private readonly Transform _root;
        private readonly GameObjectPool<SpriteRenderer> _pool;
        private readonly List<Shape> _shapes = new List<Shape>();

        public int ActiveShapeCount => _shapes.Count;

        public SkillWorldEffectPresenter(IReadOnlyDictionary<ContentId, SkillWorldEffectProfile> profiles)
        {
            _profiles = profiles ?? new Dictionary<ContentId, SkillWorldEffectProfile>();
            _root = new GameObject("Skill World Effect Pool").transform;
            _pool = new GameObjectPool<SpriteRenderer>(() => new GameObject("SkillWorldEffect").AddComponent<SpriteRenderer>(), _root);
        }

        public bool TryGetProfile(ContentId skillId, SkillWorldEffectKind kind, out SkillWorldEffectProfile profile) =>
            _profiles.TryGetValue(skillId, out profile) && profile.Kind == kind;

        /// <summary>Held ring that the caller resizes each tick; released with <see cref="Release"/>.</summary>
        public object BeginRing(SkillWorldEffectProfile profile, Vector2 center, float radius)
        {
            var shape = Rent(ProceduralShapeSprites.Ring, profile.Color, center, 0f, 0f, true);
            SetRing(shape, radius, profile.Thickness);
            return shape;
        }

        public void UpdateRing(object handle, float radius, float thickness)
        {
            if (handle is Shape shape && shape.Renderer != null) SetRing(shape, radius, thickness);
        }

        /// <summary>Held filled telegraph disc for a pending strike; released with <see cref="Release"/>.</summary>
        public object BeginTelegraph(SkillWorldEffectProfile profile, Vector2 center, float radius)
        {
            var shape = Rent(ProceduralShapeSprites.Disc, profile.Color, center, 0f, 0f, true);
            shape.Renderer.transform.localScale = Vector3.one * radius * 2f;
            return shape;
        }

        /// <summary>Stops holding a shape; it fades over <paramref name="fadeSeconds"/> and returns to the pool.</summary>
        public void Release(object handle, float fadeSeconds)
        {
            if (!(handle is Shape shape) || !shape.Held) return;
            shape.Held = false;
            shape.Remaining = shape.Fade = Mathf.Max(fadeSeconds, 0.0001f);
        }

        public void Flash(SkillWorldEffectProfile profile, Vector2 center, float radius)
        {
            var shape = Rent(ProceduralShapeSprites.Disc, profile.ImpactColor, center, 0f, profile.FadeSeconds, false);
            shape.Renderer.transform.localScale = Vector3.one * radius * 2f;
            if (!profile.HasPillar) return;
            // Light falling from above onto the strike point (OBS-03, playtest 2026-09-24_e1e04fc4).
            var pillar = Rent(ProceduralShapeSprites.Pillar, profile.ImpactColor, center, 0f, profile.FadeSeconds, false);
            pillar.Renderer.sortingOrder = SortingOrder + 1;
            var size = ProceduralShapeSprites.Pillar.bounds.size;
            pillar.Renderer.transform.localScale = new Vector3(profile.PillarWidth / size.x, profile.PillarHeight / size.y, 1f);
        }

        public void Segment(SkillWorldEffectProfile profile, Vector2 from, Vector2 to)
        {
            var offset = to - from;
            var length = offset.magnitude;
            if (length <= Mathf.Epsilon) return;
            var angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            var shape = Rent(PlaceholderSprite.Shared, profile.Color, (from + to) * .5f, angle, profile.FadeSeconds, false);
            shape.Renderer.transform.localScale = new Vector3(length, profile.Thickness, 1f);
        }

        public void Tick(float deltaTime)
        {
            for (var i = _shapes.Count - 1; i >= 0; i--)
            {
                var shape = _shapes[i];
                if (shape.Held) continue;
                shape.Remaining -= deltaTime;
                if (shape.Remaining <= 0f)
                {
                    Return(shape);
                    _shapes.RemoveAt(i);
                    continue;
                }
                var color = shape.Color;
                color.a *= Mathf.Clamp01(shape.Remaining / shape.Fade);
                shape.Renderer.color = color;
            }
        }

        public void Clear()
        {
            for (var i = 0; i < _shapes.Count; i++) Return(_shapes[i]);
            _shapes.Clear();
        }

        public void Dispose()
        {
            Clear();
            if (_root == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(_root.gameObject);
            else UnityEngine.Object.DestroyImmediate(_root.gameObject);
        }

        private Shape Rent(Sprite sprite, Color color, Vector2 center, float angle, float fade, bool held)
        {
            var renderer = _pool.Rent();
            renderer.transform.SetParent(null, false);
            renderer.transform.position = center;
            renderer.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = SortingOrder;
            renderer.enabled = true;
            var shape = new Shape { Renderer = renderer, Color = color, Remaining = fade, Fade = Mathf.Max(fade, 0.0001f), Held = held };
            _shapes.Add(shape);
            return shape;
        }

        private static void SetRing(Shape shape, float radius, float thickness)
        {
            // Ring sprite inner edge is at 88% of the outer edge: keep the visible band close to thickness.
            var diameter = Mathf.Max(radius * 2f, thickness);
            shape.Renderer.transform.localScale = Vector3.one * diameter;
        }

        private void Return(Shape shape)
        {
            if (shape.Renderer == null) return;
            shape.Renderer.sprite = null;
            shape.Renderer.color = Color.white;
            shape.Renderer.enabled = false;
            shape.Renderer.transform.localScale = Vector3.one;
            shape.Renderer.transform.rotation = Quaternion.identity;
            _pool.Return(shape.Renderer);
            shape.Renderer = null;
        }
    }
}
