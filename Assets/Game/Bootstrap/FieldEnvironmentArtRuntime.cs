using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
using Game.Field;
using Game.Movement;
using Game.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace Game.Bootstrap
{
    /// <summary>Visual-only fixture field dressing. Physics remains on the validated scene colliders.</summary>
    public sealed class FieldEnvironmentArtRuntime : IDisposable
    {
        private readonly List<PlaceholderState> _placeholders = new List<PlaceholderState>();
        private GameObject _root;

        public Transform Root => _root != null ? _root.transform : null;

        public void Initialize(FieldEnvironmentPresentationDefinition definition, ContentRegistry registry,
            FieldEnvironmentDefinition environment, Scene scene, float sideLength)
        {
            if (_root != null) throw new InvalidOperationException("Field environment art is already initialized.");
            if (definition == null || registry == null || environment == null || !scene.IsValid())
                throw new ArgumentException("Field environment art requires definition, registry, environment and scene.");
            if (definition.EnvironmentId != environment.Id)
                throw new InvalidOperationException("Field presentation targets a different environment.");

            var ground = Resolve(definition.Ground, registry, SpriteRole.Tile);
            var fence = Resolve(definition.Fence, registry, SpriteRole.Prop);
            var obstacle = Resolve(definition.Obstacle, registry, SpriteRole.Prop);
            var bush = Resolve(definition.Bush, registry, SpriteRole.Prop);
            var grass = Resolve(definition.Grass, registry, SpriteRole.Prop);
            var transforms = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToList();
            var spawn = RequireUnique(transforms, environment.SpawnPointName);
            var obstacleTransform = RequireUnique(transforms, definition.ObstacleName);

            _root = new GameObject("FieldEnvironmentArt");
            SceneManager.MoveGameObjectToScene(_root, scene);
            try
            {
                foreach (var name in environment.ObstacleNames) HidePlaceholder(RequireUnique(transforms, name));
                CreateGround(ground, sideLength);
                CreateBoundary(fence, sideLength, definition.FenceHeight);
                CreateSprite("Stump", obstacle, obstacleTransform.position, definition.ObstacleScale, 0, -2, _root.transform);
                CreateDecor(definition, bush, grass, spawn.position, obstacleTransform.position, sideLength);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            for (var i = 0; i < _placeholders.Count; i++) _placeholders[i].Restore();
            _placeholders.Clear();
            if (_root == null) return;
            _root.SetActive(false);
            if (Application.isPlaying) Object.Destroy(_root); else Object.DestroyImmediate(_root);
            _root = null;
        }

        private void CreateGround(Sprite sprite, float sideLength)
        {
            var renderer = CreateRenderer("Ground", sprite, Vector3.zero, -100, _root.transform);
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = new Vector2(sideLength, sideLength);
        }

        private void CreateBoundary(Sprite sprite, float sideLength, float height)
        {
            var half = sideLength * .5f;
            CreateFence("FenceTop", sprite, new Vector2(0, half), sideLength, height, 0);
            CreateFence("FenceBottom", sprite, new Vector2(0, -half), sideLength, height, 0);
            CreateFence("FenceLeft", sprite, new Vector2(-half, 0), sideLength, height, 90);
            CreateFence("FenceRight", sprite, new Vector2(half, 0), sideLength, height, 90);
        }

        private void CreateFence(string name, Sprite sprite, Vector2 position, float length, float height, float rotation)
        {
            var renderer = CreateRenderer(name, sprite, position, -2, _root.transform);
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = new Vector2(length, height);
            renderer.transform.rotation = Quaternion.Euler(0, 0, rotation);
        }

        private void CreateDecor(FieldEnvironmentPresentationDefinition definition, Sprite bush, Sprite grass,
            Vector2 spawn, Vector2 obstacle, float sideLength)
        {
            var random = new Random(definition.Seed);
            var half = sideLength * .5f - definition.DecorationMargin;
            for (var y = -half; y <= half; y += definition.DecorationSpacing)
            for (var x = -half; x <= half; x += definition.DecorationSpacing)
            {
                var position = new Vector2(
                    x + Range(random, -definition.DecorationJitter, definition.DecorationJitter),
                    y + Range(random, -definition.DecorationJitter, definition.DecorationJitter));
                if (random.NextDouble() > definition.DecorationChance ||
                    Vector2.Distance(position, spawn) < definition.SafeRadius ||
                    Vector2.Distance(position, obstacle) < definition.SafeRadius)
                    continue;
                var useBush = random.NextDouble() < definition.BushChance;
                var scale = useBush
                    ? Range(random, definition.BushScaleMin, definition.BushScaleMax)
                    : Range(random, definition.GrassScaleMin, definition.GrassScaleMax);
                var renderer = CreateSprite(useBush ? "Bush" : "Grass", useBush ? bush : grass, position,
                    scale, Range(random, -12, 12), useBush ? -8 : -12, _root.transform);
                renderer.flipX = random.Next(0, 2) == 0;
            }
        }

        private void HidePlaceholder(Transform target)
        {
            var placeholder = target.GetComponent<PlaceholderColorRenderer>();
            var renderer = target.GetComponent<SpriteRenderer>();
            _placeholders.Add(new PlaceholderState(placeholder, renderer));
            if (placeholder != null) placeholder.enabled = false;
            if (renderer != null) renderer.enabled = false;
        }

        private static SpriteRenderer CreateSprite(string name, Sprite sprite, Vector2 position, float scale,
            float rotation, int sortingOrder, Transform parent)
        {
            var renderer = CreateRenderer(name, sprite, position, sortingOrder, parent);
            renderer.transform.localScale = Vector3.one * scale;
            renderer.transform.rotation = Quaternion.Euler(0, 0, rotation);
            return renderer;
        }

        private static SpriteRenderer CreateRenderer(string name, Sprite sprite, Vector2 position, int sortingOrder,
            Transform parent)
        {
            var item = new GameObject(name);
            item.transform.SetParent(parent, false);
            item.transform.position = new Vector3(position.x, position.y, 0);
            var renderer = item.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private static Sprite Resolve(ContentRef<SpriteDefinition> reference, ContentRegistry registry, SpriteRole role)
        {
            var definition = reference.Resolve(registry);
            definition.RequireRole(role);
            return definition.Sprite;
        }

        private static Transform RequireUnique(IEnumerable<Transform> transforms, string name)
        {
            var matches = transforms.Where(item => item.name == name).ToList();
            if (matches.Count != 1) throw new InvalidOperationException($"Field art binding '{name}' must occur exactly once.");
            return matches[0];
        }

        private static float Range(Random random, float minimum, float maximum) =>
            minimum + (float)random.NextDouble() * (maximum - minimum);

        private readonly struct PlaceholderState
        {
            private readonly PlaceholderColorRenderer _placeholder;
            private readonly SpriteRenderer _renderer;
            private readonly bool _placeholderEnabled;
            private readonly bool _rendererEnabled;

            public PlaceholderState(PlaceholderColorRenderer placeholder, SpriteRenderer renderer)
            {
                _placeholder = placeholder;
                _renderer = renderer;
                _placeholderEnabled = placeholder != null && placeholder.enabled;
                _rendererEnabled = renderer != null && renderer.enabled;
            }

            public void Restore()
            {
                if (_placeholder != null) _placeholder.enabled = _placeholderEnabled;
                if (_renderer != null) _renderer.enabled = _rendererEnabled;
            }
        }
    }
}
