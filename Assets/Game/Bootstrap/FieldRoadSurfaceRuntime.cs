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
    /// Road covers and visual curb share the smooth player-only boundary contours. Without supplied art sprites,
    /// renders the original colored blockout (DECISION-0137).
    /// </summary>
    public sealed class FieldRoadSurfaceRuntime : IDisposable
    {
        // Straight runs collapse to their ends; arcs keep their grid samples (well under a player's contact radius).
        private const float ContourTolerance = .01f;

        private GameObject _root;
        private readonly List<Object> _assets = new List<Object>();
        private readonly List<EdgeCollider2D> _boundaries = new List<EdgeCollider2D>();
        public IReadOnlyList<EdgeCollider2D> BoundaryColliders => _boundaries;
        public void Initialize(FieldRoadLayout layout, Transform parent, Sprite main = null, Sprite branch = null, Sprite curb = null)
        {
            if (_root != null || layout == null || parent == null) throw new ArgumentException("Road surface requires an uninitialized owner and layout.");
            using var guard = PerfGuard.Measure("RoadSurface.Initialize", 500f);
            _root = new GameObject("RoadNetwork"); _root.transform.SetParent(parent,false);
            try
            {
                var p = layout.Profile;
                var walkable = FieldRoadDistanceField.Walkable(layout);
                var textured = main != null || branch != null || curb != null;
                if (textured && (main == null || branch == null || curb == null || p.Art == null))
                    throw new ArgumentException("Textured roads require all three sprites and an art profile.");
                var playerLayer = LayerMask.NameToLayer("Player");
                if (playerLayer < 0) throw new InvalidOperationException("Player layer missing.");
                var contours = FieldRoadMarchingSquares.Contours(walkable,ContourTolerance);
                foreach (var contour in contours)
                {
                    var go = new GameObject("RoadBoundary"); go.transform.SetParent(_root.transform,false);
                    var collider = go.AddComponent<EdgeCollider2D>();
                    collider.points = contour;
                    collider.excludeLayers = ~(1 << playerLayer);
                    _boundaries.Add(collider);
                }
                // Main cover fills the walking area; the broken cover retains the approved branch-mouth overlap.
                CreateMesh("MainRoads",walkable,textured ? Color.white : p.MainColor,-90, main, p.Art);
                CreateMesh("BookBranches",FieldRoadDistanceField.Branches(layout),textured ? Color.white : p.DeadEndColor,-89, branch, p.Art);
                if (textured) CreateCurb(contours, walkable, curb, branch, p.Art);
                Physics2D.SyncTransforms();
            }
            catch { Dispose(); throw; }
        }
        private void CreateMesh(string name, FieldRoadDistanceField field, Color color, int sortingOrder,
            Sprite sprite, FieldRoadArtDefinition art)
        {
            var vertices = new List<Vector3>(); var indices = new List<int>();
            FieldRoadMarchingSquares.Fill(field,vertices,indices);
            var colors = new Color[vertices.Count];
            for (var i = 0; i < colors.Length; i++) colors[i] = color;
            var uv = new List<Vector2>(vertices.Count);
            foreach (var vertex in vertices) uv.Add(sprite == null ? Vector2.zero : (Vector2)vertex / art.SurfaceRepeat);
            CreateRenderer(name, vertices, indices, uv, colors, sortingOrder, sprite, true);
        }

        private MeshRenderer CreateRenderer(string name, List<Vector3> vertices, List<int> indices, List<Vector2> uv,
            Color[] colors, int sortingOrder, Sprite sprite, bool mirror, List<Vector2> band = null)
        {
            var mesh = new Mesh { name = name, indexFormat = IndexFormat.UInt32 };
            _assets.Add(mesh); mesh.SetVertices(vertices); mesh.colors = colors; mesh.SetUVs(0,uv);
            if (band != null) mesh.SetUVs(1,band);
            mesh.SetTriangles(indices,0); mesh.RecalculateBounds();
            var shader = sprite == null ? Shader.Find("Sprites/Default") : Resources.Load<Shader>("Shaders/FieldRoadSurface");
            if (shader == null) throw new InvalidOperationException("Road blockout shader missing.");
            var material = new Material(shader); _assets.Add(material);
            if (sprite != null)
            {
                // Mirroring happens in the shader: imported non-readable textures remain shared and unchanged.
                material.mainTexture = sprite.texture;
                material.SetFloat("_MirrorRepeat",mirror ? 1f : 0f);
            }
            var go = new GameObject(name); go.transform.SetParent(_root.transform,false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material; renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private void CreateCurb(IReadOnlyList<Vector2[]> contours, FieldRoadDistanceField field, Sprite sprite, Sprite soil,
            FieldRoadArtDefinition art)
        {
            var vertices = new List<Vector3>(); var triangles = new List<int>(); var uv = new List<Vector2>();
            var faces = new List<Vector3>(); var faceTriangles = new List<int>();
            var faceUv = new List<Vector2>(); var faceColors = new List<Color>();
            var edge = new List<Vector3>(); var edgeTriangles = new List<int>();
            var edgeUv = new List<Vector2>(); var edgeBand = new List<Vector2>();
            var rect = art.CurbUvBounds;
            foreach (var contour in contours)
            {
                var count = contour.Length - 1; // Contours close with a duplicated first point.
                var offsets = new Vector2[count];
                for (var i = 0; i < count; i++)
                {
                    var previous = (contour[i] - contour[(i + count - 1) % count]).normalized;
                    var next = (contour[(i + 1) % count] - contour[i]).normalized;
                    var normal = new Vector2(-(previous.y + next.y), previous.x + next.x).normalized;
                    if (Sample(field, contour[i] + normal * field.Step) < Sample(field, contour[i] - normal * field.Step)) normal = -normal;
                    var nextNormal = new Vector2(-next.y, next.x);
                    // Bounded miter keeps a continuous strip without spikes at concave mouths.
                    var miter = Mathf.Min(2f, 1f / Mathf.Max(.01f, Mathf.Abs(Vector2.Dot(normal, nextNormal))));
                    offsets[i] = normal * (art.CurbWidth * .5f * miter);
                }
                var distance = 0f;
                for (var i = 0; i < count; i++)
                {
                    var next = (i + 1) % count;
                    var length = Vector2.Distance(contour[i], contour[next]);
                    var consumed = 0f;
                    while (consumed < length)
                    {
                        var phase = Mathf.Repeat(distance, art.CurbRepeat);
                        var span = Mathf.Min(length - consumed, art.CurbRepeat - phase);
                        if (span <= 1e-5f) { distance += 1e-5f; continue; }
                        var a = Vector2.Lerp(contour[i], contour[next], consumed / length);
                        var b = Vector2.Lerp(contour[i], contour[next], (consumed + span) / length);
                        var an = Vector2.Lerp(offsets[i], offsets[next], consumed / length);
                        var bn = Vector2.Lerp(offsets[i], offsets[next], (consumed + span) / length);
                        var index = vertices.Count;
                        vertices.Add(a - an); vertices.Add(b - bn); vertices.Add(b + bn); vertices.Add(a + an);
                        var u0 = Mathf.Lerp(rect.xMin, rect.xMax, phase / art.CurbRepeat);
                        var u1 = Mathf.Lerp(rect.xMin, rect.xMax, (phase + span) / art.CurbRepeat);
                        uv.Add(new Vector2(u0,rect.yMin)); uv.Add(new Vector2(u1,rect.yMin));
                        uv.Add(new Vector2(u1,rect.yMax)); uv.Add(new Vector2(u0,rect.yMax));
                        triangles.Add(index); triangles.Add(index+1); triangles.Add(index+2);
                        triangles.Add(index); triangles.Add(index+2); triangles.Add(index+3);
                        // Both physical sides are candidates, but only faces toward the viewer are visible.
                        // Their projected height always goes down the screen, never inward along the road normal.
                        AddFace(a,b,an,bn,1f,u0,u1);
                        AddFace(a,b,an,bn,-1f,u0,u1);
                        var edgeIndex = edge.Count;
                        edge.Add(a+an); edge.Add(b+bn);
                        edge.Add(b+bn+bn.normalized*art.EdgeWidth); edge.Add(a+an+an.normalized*art.EdgeWidth);
                        for (var k = 0; k < 4; k++)
                        {
                            edgeUv.Add((Vector2)edge[edgeIndex+k]/art.SurfaceRepeat);
                            edgeBand.Add(new Vector2(k < 2 ? 0f : 1f,0f));
                        }
                        AddQuad(edgeTriangles,edgeIndex);
                        consumed += span; distance += span;
                    }
                }
            }
            var colors = new Color[vertices.Count];
            for (var i = 0; i < colors.Length; i++) colors[i] = Color.white;
            CreateRenderer("RoadCurb",vertices,triangles,uv,colors,-87,sprite,false);
            CreateRenderer("RoadCurbFaces",faces,faceTriangles,faceUv,faceColors.ToArray(),-88,sprite,false);
            var edgeColors = new Color[edge.Count];
            for (var i = 0; i < edgeColors.Length; i++) edgeColors[i] = Color.white;
            var edgeRenderer = CreateRenderer("RoadGrassTransition",edge,edgeTriangles,edgeUv,edgeColors,-92,soil,true,edgeBand);
            var material = edgeRenderer.sharedMaterial;
            material.SetFloat("_EdgeBand",1f); material.SetFloat("_EdgeOpacity",art.EdgeOpacity);
            material.SetFloat("_EdgeNoiseScale",art.EdgeNoiseScale); material.SetFloat("_EdgeNoiseStrength",art.EdgeNoiseStrength);
            material.SetColor("_EdgeSoilColor",QualitySettings.activeColorSpace == ColorSpace.Linear
                ? art.EdgeSoilColor.linear : art.EdgeSoilColor);

            void AddFace(Vector2 a, Vector2 b, Vector2 an, Vector2 bn, float side, float u0, float u1)
            {
                var index = faces.Count;
                var start = a+an*side; var end = b+bn*side;
                var down = Vector2.down*art.CurbFaceHeight;
                faces.Add(start); faces.Add(end); faces.Add(end+down); faces.Add(start+down);
                faceUv.Add(new Vector2(u0,rect.yMax)); faceUv.Add(new Vector2(u1,rect.yMax));
                faceUv.Add(new Vector2(u1,rect.yMin)); faceUv.Add(new Vector2(u0,rect.yMin));
                var ac = art.CurbFaceTint; var bc = art.CurbFaceTint;
                ac.a *= Mathf.Clamp01(-an.normalized.y*side); bc.a *= Mathf.Clamp01(-bn.normalized.y*side);
                faceColors.Add(ac); faceColors.Add(bc); faceColors.Add(bc); faceColors.Add(ac);
                AddQuad(faceTriangles,index);
            }
        }

        private static void AddQuad(List<int> triangles, int index)
        {
            triangles.Add(index); triangles.Add(index+1); triangles.Add(index+2);
            triangles.Add(index); triangles.Add(index+2); triangles.Add(index+3);
        }

        private static float Sample(FieldRoadDistanceField field, Vector2 position)
        {
            var x = Mathf.Clamp((position.x-field.Origin)/field.Step,0,field.Cells);
            var y = Mathf.Clamp((position.y-field.Origin)/field.Step,0,field.Cells);
            var ix = Mathf.Min(Mathf.FloorToInt(x),field.Cells-1); var iy = Mathf.Min(Mathf.FloorToInt(y),field.Cells-1);
            return Mathf.Lerp(Mathf.Lerp(field[ix,iy],field[ix+1,iy],x-ix),
                Mathf.Lerp(field[ix,iy+1],field[ix+1,iy+1],x-ix),y-iy);
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
