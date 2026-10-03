using System;
using System.Collections.Generic;
using Game.Content;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Main-thread-only shared quad and eight materials, owned and disposed by one screen-event driver.</summary>
    public sealed class ScreenEventArtResources : IDisposable
    {
        private readonly Dictionary<string, Material> _materials = new Dictionary<string, Material>(StringComparer.Ordinal);
        public ScreenEventPresentationProfile Profile { get; }
        public Mesh Quad { get; private set; }

        public ScreenEventArtResources(ScreenEventPresentationProfile profile, ContentRegistry registry)
        {
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
            if (registry == null) throw new ArgumentNullException(nameof(registry));
            try
            {
                var shader = Resources.Load<Shader>("Shaders/ScreenEventArtwork");
                if (shader == null) throw new InvalidOperationException("Missing screen-event artwork shader.");
                foreach (var pair in profile.Artwork)
                {
                    var sprite = pair.Value.Visual.Resolve(registry);
                    sprite.RequireRole(pair.Value.Role);
                    var crop = pair.Value.UvRect;
                    var material = new Material(shader) { name = "Screen event " + pair.Key, mainTexture = sprite.Sprite.texture,
                        hideFlags = HideFlags.HideAndDontSave };
                    material.SetVector("_UvRect", new Vector4(crop.x, crop.y, crop.width, crop.height));
                    material.SetFloat("_BorderWidth", profile.BorderWidth);
                    material.SetFloat("_RepeatLength", profile.RepeatLength);
                    material.SetFloat("_ExteriorRibbonWidth", profile.ExteriorRibbonWidth);
                    _materials.Add(pair.Key, material);
                }
                Quad = new Mesh { name = "Screen event unit quad", hideFlags = HideFlags.HideAndDontSave };
                Quad.vertices = new[] { new Vector3(-.5f, -.5f, 0f), new Vector3(.5f, -.5f, 0f), new Vector3(.5f, .5f, 0f), new Vector3(-.5f, .5f, 0f) };
                Quad.uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
                Quad.triangles = new[] { 0, 1, 2, 0, 2, 3 };
                Quad.RecalculateBounds();
            }
            catch { Dispose(); throw; }
        }

        public Material MaterialFor(string key) => _materials[key];

        public void Dispose()
        {
            foreach (var material in _materials.Values) Release(material);
            _materials.Clear();
            Release(Quad);
            Quad = null;
        }

        private static void Release(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(value); else UnityEngine.Object.DestroyImmediate(value);
        }
    }
}
