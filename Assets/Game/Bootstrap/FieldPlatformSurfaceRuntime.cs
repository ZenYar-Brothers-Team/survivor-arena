using System;
using System.Collections.Generic;
using Game.Diagnostics;
using Game.Presentation;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Game.Bootstrap
{
    /// <summary>
    /// Flat-color blockout of a platform field: a void-colored arena floor, bridges and round platforms. Visual only; the
    /// void's damage is <see cref="FieldVoidDamageDriver"/>, and nothing here blocks movement.
    /// </summary>
    public sealed class FieldPlatformSurfaceRuntime : IDisposable
    {
        private const int VoidOrder = -95;
        private const int BridgeOrder = -90;
        private const int PlatformOrder = -89;

        private readonly List<Object> _assets = new List<Object>();
        private GameObject _root;

        public void Initialize(FieldPlatformLayout layout, Transform parent)
        {
            if (_root != null || layout == null || parent == null)
                throw new ArgumentException("Platform surface requires an uninitialized owner and a layout.");
            using var guard = PerfGuard.Measure("PlatformSurface.Initialize", 100f);
            _root = new GameObject("PlatformNetwork");
            _root.transform.SetParent(parent, false);
            try
            {
                var p = layout.Profile;
                var vertices = new List<Vector3>();
                var indices = new List<int>();
                var half = p.ArenaSideLength * .5f;
                AddQuad(vertices, indices, new Vector2(-half, -half), new Vector2(half, -half), new Vector2(half, half), new Vector2(-half, half));
                CreateMesh("Void", vertices, indices, p.VoidColor, VoidOrder);

                vertices = new List<Vector3>();
                indices = new List<int>();
                var halfWidth = p.BridgeWidth * .5f;
                foreach (var bridge in layout.Bridges)
                {
                    var a = layout.Platforms[bridge.From].Center;
                    var b = layout.Platforms[bridge.To].Center;
                    var side = Vector2.Perpendicular((b - a).normalized) * halfWidth;
                    AddQuad(vertices, indices, a - side, b - side, b + side, a + side);
                }
                CreateMesh("Bridges", vertices, indices, p.BridgeColor, BridgeOrder);

                vertices = new List<Vector3>();
                indices = new List<int>();
                for (var i = 0; i < layout.Platforms.Count; i++)
                    if (i != layout.StartIndex) AddDisc(vertices, indices, layout.Platforms[i], p.CircleSegments);
                CreateMesh("Platforms", vertices, indices, p.PlatformColor, PlatformOrder);

                vertices = new List<Vector3>();
                indices = new List<int>();
                AddDisc(vertices, indices, layout.Platforms[layout.StartIndex], p.CircleSegments);
                CreateMesh("StartPlatform", vertices, indices, p.StartPlatformColor, PlatformOrder);
            }
            catch { Dispose(); throw; }
        }

        private static void AddQuad(List<Vector3> vertices, List<int> indices, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            var first = vertices.Count;
            vertices.Add(a); vertices.Add(b); vertices.Add(c); vertices.Add(d);
            indices.AddRange(new[] { first, first + 1, first + 2, first, first + 2, first + 3 });
        }

        private static void AddDisc(List<Vector3> vertices, List<int> indices, FieldPlatformDisc disc, int segments)
        {
            var center = vertices.Count;
            vertices.Add(disc.Center);
            for (var i = 0; i < segments; i++)
            {
                var angle = i * Mathf.PI * 2f / segments;
                vertices.Add(disc.Center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * disc.Radius);
            }
            for (var i = 0; i < segments; i++)
            {
                indices.Add(center);
                indices.Add(center + 1 + i);
                indices.Add(center + 1 + (i + 1) % segments);
            }
        }

        private void CreateMesh(string name, List<Vector3> vertices, List<int> indices, Color color, int sortingOrder)
        {
            if (vertices.Count == 0) return;
            var mesh = new Mesh { name = name, indexFormat = IndexFormat.UInt32 };
            _assets.Add(mesh);
            mesh.SetVertices(vertices);
            var colors = new Color[vertices.Count];
            for (var i = 0; i < colors.Length; i++) colors[i] = color;
            mesh.colors = colors;
            mesh.SetTriangles(indices, 0);
            mesh.RecalculateBounds();
            var shader = Shader.Find("Sprites/Default") ?? throw new InvalidOperationException("Sprites/Default shader missing.");
            var material = new Material(shader);
            _assets.Add(material);
            var go = new GameObject(name);
            go.transform.SetParent(_root.transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingOrder = sortingOrder;
        }

        public void Dispose()
        {
            if (_root != null)
            {
                if (Application.isPlaying) Object.Destroy(_root); else Object.DestroyImmediate(_root);
                _root = null;
            }
            foreach (var asset in _assets)
                if (asset != null) { if (Application.isPlaying) Object.Destroy(asset); else Object.DestroyImmediate(asset); }
            _assets.Clear();
        }
    }
}
