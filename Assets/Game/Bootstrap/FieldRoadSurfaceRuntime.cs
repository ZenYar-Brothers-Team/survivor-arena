using System;
using System.Collections.Generic;
using Game.Diagnostics;
using Game.Presentation;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Game.Bootstrap
{
    /// <summary>Static road blockout: shared cell mask produces colored meshes and player-only boundary contours.</summary>
    public sealed class FieldRoadSurfaceRuntime : IDisposable
    {
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
                var cells = layout.BuildSurface(); var p = layout.Profile;
                var side = Mathf.CeilToInt(p.ArenaSideLength/p.SurfaceStep);
                // Combine identical row spans vertically. Rectangles share borders, never intrude into road cells.
                var rectangles = new List<Rect>[3] { new List<Rect>(),new List<Rect>(),new List<Rect>() };
                var previous = new Dictionary<(int x,int width,byte kind),int>();
                for (var y = 0; y < side; y++)
                {
                    var current = new Dictionary<(int x,int width,byte kind),int>();
                    for (var x = 0; x < side;)
                    {
                        var begin = x; var kind = cells[y*side+x];
                        while (x < side && cells[y*side+x] == kind) x++;
                        var key = (begin,x-begin,kind);
                        if (previous.TryGetValue(key,out var index))
                        { var r = rectangles[kind][index]; r.height += p.SurfaceStep; rectangles[kind][index] = r; }
                        else
                        {
                            index = rectangles[kind].Count;
                            rectangles[kind].Add(new Rect(begin*p.SurfaceStep-p.ArenaSideLength*.5f,y*p.SurfaceStep-p.ArenaSideLength*.5f,(x-begin)*p.SurfaceStep,p.SurfaceStep));
                        }
                        current.Add(key,index);
                    }
                    previous = current;
                }
                var playerLayer = LayerMask.NameToLayer("Player");
                if (playerLayer < 0) throw new InvalidOperationException("Player layer missing.");
                foreach (var contour in TraceBoundaries(cells,side))
                {
                    var go = new GameObject("RoadBoundary"); go.transform.SetParent(_root.transform,false);
                    var collider = go.AddComponent<EdgeCollider2D>();
                    var points = new Vector2[contour.Count];
                    for (var i = 0; i < points.Length; i++) points[i] = (Vector2)contour[i]*p.SurfaceStep-Vector2.one*(p.ArenaSideLength*.5f);
                    collider.points = points;
                    collider.excludeLayers = ~(1 << playerLayer);
                    _boundaries.Add(collider);
                }
                CreateMesh("MainRoads",rectangles[1],p.MainColor);
                CreateMesh("BookBranches",rectangles[2],p.DeadEndColor);
                Physics2D.SyncTransforms();
            }
            catch { Dispose(); throw; }
        }
        private static Vector2[] Quad(Rect r) => new[] { new Vector2(r.xMin,r.yMin),new Vector2(r.xMin,r.yMax),new Vector2(r.xMax,r.yMax),new Vector2(r.xMax,r.yMin) };
        private static List<List<Vector2Int>> TraceBoundaries(byte[] cells, int side)
        {
            using var guard = PerfGuard.Measure("RoadSurface.TraceBoundaries", 100f);
            var outgoing = new Dictionary<Vector2Int,List<Vector2Int>>();
            bool Road(int x,int y) => x >= 0 && y >= 0 && x < side && y < side && cells[y*side+x] != 0;
            void Add(Vector2Int a,Vector2Int b)
            { if (!outgoing.TryGetValue(a,out var ends)) { ends = new List<Vector2Int>(); outgoing.Add(a,ends); } ends.Add(b); }
            for (var y = 0; y < side; y++) for (var x = 0; x < side; x++)
            {
                if (!Road(x,y)) continue;
                if (!Road(x,y-1)) Add(new Vector2Int(x,y),new Vector2Int(x+1,y));
                if (!Road(x+1,y)) Add(new Vector2Int(x+1,y),new Vector2Int(x+1,y+1));
                if (!Road(x,y+1)) Add(new Vector2Int(x+1,y+1),new Vector2Int(x,y+1));
                if (!Road(x-1,y)) Add(new Vector2Int(x,y+1),new Vector2Int(x,y));
            }
            var contours = new List<List<Vector2Int>>();
            while (outgoing.Count > 0)
            {
                using var keys = outgoing.Keys.GetEnumerator(); keys.MoveNext();
                var first = keys.Current; var current = first; var previous = first; var raw = new List<Vector2Int>();
                do
                {
                    raw.Add(current);
                    if (!outgoing.TryGetValue(current,out var ends)) throw new InvalidOperationException("Open road boundary.");
                    var index = 0;
                    // At a diagonal cell contact keep the contour around the occupied cell (left turn).
                    if (ends.Count > 1 && current != previous)
                    {
                        var direction = current-previous;
                        for (var i = 0; i < ends.Count; i++)
                        { var next = ends[i]-current; if (direction.x*next.y-direction.y*next.x > 0) { index = i; break; } }
                    }
                    var destination = ends[index]; ends.RemoveAt(index);
                    if (ends.Count == 0) outgoing.Remove(current);
                    previous = current; current = destination;
                } while (current != first);
                var contour = new List<Vector2Int>();
                for (var i = 0; i < raw.Count; i++)
                {
                    var before = raw[i]-raw[(i+raw.Count-1)%raw.Count]; var after = raw[(i+1)%raw.Count]-raw[i];
                    if (before != after) contour.Add(raw[i]);
                }
                if (contour.Count < 3) throw new InvalidOperationException("Degenerate road boundary.");
                contour.Add(contour[0]); contours.Add(contour);
            }
            return contours;
        }
        private void CreateMesh(string name, List<Rect> rectangles, Color color)
        {
            var vertices = new Vector3[rectangles.Count*4]; var colors = new Color[vertices.Length]; var indices = new int[rectangles.Count*6];
            for (var i = 0; i < rectangles.Count; i++)
            {
                var quad = Quad(rectangles[i]);
                for (var j = 0; j < 4; j++) { vertices[i*4+j] = quad[j]; colors[i*4+j] = color; }
                var offset = i*4; var t = i*6;
                indices[t] = offset; indices[t+1] = offset+1; indices[t+2] = offset+2;
                indices[t+3] = offset; indices[t+4] = offset+2; indices[t+5] = offset+3;
            }
            var mesh = new Mesh { name = name, indexFormat = IndexFormat.UInt32 };
            _assets.Add(mesh); mesh.vertices = vertices; mesh.colors = colors; mesh.triangles = indices; mesh.RecalculateBounds();
            var shader = Shader.Find("Sprites/Default");
            if (shader == null) throw new InvalidOperationException("Road blockout shader missing.");
            var material = new Material(shader); _assets.Add(material);
            var go = new GameObject(name); go.transform.SetParent(_root.transform,false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material; renderer.sortingOrder = -90;
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
