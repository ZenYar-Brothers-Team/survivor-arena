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
    /// Static road blockout: one signed distance field drives the colored meshes and the player-only boundary contours, so
    /// the edge is smooth along straight, oblique and round outlines and matches what is drawn (DECISION-0137 revision).
    /// </summary>
    public sealed class FieldRoadSurfaceRuntime : IDisposable
    {
        // Straight runs collapse to their ends; arcs keep their grid samples (well under a player's contact radius).
        private const float ContourTolerance = .01f;

        private GameObject _root;
        private readonly List<Object> _assets = new List<Object>();
        private readonly List<EdgeCollider2D> _boundaries = new List<EdgeCollider2D>();
        public IReadOnlyList<EdgeCollider2D> BoundaryColliders => _boundaries;
        public void Initialize(FieldRoadLayout layout, Transform parent)
        {
            if (_root != null || layout == null || parent == null) throw new ArgumentException("Road surface requires an uninitialized owner and layout.");
            using var guard = PerfGuard.Measure("RoadSurface.Initialize", 500f);
            _root = new GameObject("RoadNetwork"); _root.transform.SetParent(parent,false);
            try
            {
                var p = layout.Profile;
                var walkable = FieldRoadDistanceField.Walkable(layout);
                var playerLayer = LayerMask.NameToLayer("Player");
                if (playerLayer < 0) throw new InvalidOperationException("Player layer missing.");
                foreach (var contour in FieldRoadMarchingSquares.Contours(walkable,ContourTolerance))
                {
                    var go = new GameObject("RoadBoundary"); go.transform.SetParent(_root.transform,false);
                    var collider = go.AddComponent<EdgeCollider2D>();
                    collider.points = contour;
                    collider.excludeLayers = ~(1 << playerLayer);
                    _boundaries.Add(collider);
                }
                // Main color fills the whole walkable area; dead ends are drawn over it, as the former cell classification did.
                CreateMesh("MainRoads",walkable,p.MainColor,-90);
                CreateMesh("BookBranches",FieldRoadDistanceField.Branches(layout),p.DeadEndColor,-89);
                Physics2D.SyncTransforms();
            }
            catch { Dispose(); throw; }
        }
        private void CreateMesh(string name, FieldRoadDistanceField field, Color color, int sortingOrder)
        {
            var vertices = new List<Vector3>(); var indices = new List<int>();
            FieldRoadMarchingSquares.Fill(field,vertices,indices);
            var colors = new Color[vertices.Count];
            for (var i = 0; i < colors.Length; i++) colors[i] = color;
            var mesh = new Mesh { name = name, indexFormat = IndexFormat.UInt32 };
            _assets.Add(mesh); mesh.SetVertices(vertices); mesh.colors = colors; mesh.SetTriangles(indices,0); mesh.RecalculateBounds();
            var shader = Shader.Find("Sprites/Default");
            if (shader == null) throw new InvalidOperationException("Road blockout shader missing.");
            var material = new Material(shader); _assets.Add(material);
            var go = new GameObject(name); go.transform.SetParent(_root.transform,false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material; renderer.sortingOrder = sortingOrder;
        }
        public void Dispose()
        {
            if (_root != null) { _root.SetActive(false); Destroy(_root); _root = null; }
            _boundaries.Clear();
            foreach (var asset in _assets) Destroy(asset);
            _assets.Clear();
        }
        private static void Destroy(Object value)
        { if (value == null) return; if (Application.isPlaying) Object.Destroy(value); else Object.DestroyImmediate(value); }
    }
}
