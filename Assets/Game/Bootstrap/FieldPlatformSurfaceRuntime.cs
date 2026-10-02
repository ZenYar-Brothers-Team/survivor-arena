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
    /// Approved FIELD-009 material regions and union-edge bands, with flat-color fallback. Visual only; the
    /// void's damage is <see cref="FieldVoidDamageDriver"/>, and nothing here blocks movement.
    /// </summary>
    public sealed class FieldPlatformSurfaceRuntime : IDisposable
    {
        private const int VoidOrder = -95;
        private const int BridgeOrder = -90;
        private const int PlatformOrder = -89;

        private readonly List<Object> _assets = new List<Object>();
        private GameObject _root;
        private Sprite _reference;
        private FieldPlatformArtDefinition _art;

        public void Initialize(FieldPlatformLayout layout, Transform parent, Sprite reference = null)
        {
            if (_root != null || layout == null || parent == null)
                throw new ArgumentException("Platform surface requires an uninitialized owner and a layout.");
            using var guard = PerfGuard.Measure("PlatformSurface.Initialize", 100f);
            _reference = reference;
            _art = reference == null ? null : layout.Profile.Art
                ?? throw new ArgumentException("Textured platforms need an art profile.");
            _root = new GameObject("PlatformNetwork");
            _root.transform.SetParent(parent, false);
            try
            {
                var p = layout.Profile;
                var vertices = new List<Vector3>();
                var indices = new List<int>();
                var half = p.ArenaSideLength * .5f;
                AddQuad(vertices, indices, new Vector2(-half, -half), new Vector2(half, -half), new Vector2(half, half), new Vector2(-half, half));
                CreateMesh("Void", vertices, indices, _art == null ? p.VoidColor : Color.white, VoidOrder, true);

                vertices = new List<Vector3>();
                indices = new List<int>();
                var halfWidth = p.BridgeWidth * .5f;
                if (_art != null && _art.BridgeVeil && _art.BridgeEdgeWidth >= halfWidth)
                    throw new ArgumentException("Bridge veil edge must fit within the safe bridge width.");
                var bridgeUv = _art != null && _art.BridgeVeil ? new List<Vector2>() : null;
                foreach (var bridge in layout.Bridges)
                {
                    var a = layout.Platforms[bridge.From].Center;
                    var b = layout.Platforms[bridge.To].Center;
                    var side = Vector2.Perpendicular((b - a).normalized) * halfWidth;
                    AddQuad(vertices, indices, a - side, b - side, b + side, a + side);
                    if (bridgeUv != null)
                    {
                        var length = Vector2.Distance(a, b);
                        bridgeUv.Add(new Vector2(0f, -halfWidth)); bridgeUv.Add(new Vector2(length, -halfWidth));
                        bridgeUv.Add(new Vector2(length, halfWidth)); bridgeUv.Add(new Vector2(0f, halfWidth));
                    }
                }
                CreateMesh("Bridges", vertices, indices, _art == null ? p.BridgeColor : Color.white, BridgeOrder,
                    bridgeUv: bridgeUv, bridgeHalfWidth: halfWidth);

                vertices = new List<Vector3>();
                indices = new List<int>();
                for (var i = 0; i < layout.Platforms.Count; i++)
                    if (i != layout.StartIndex) AddDisc(vertices, indices, layout.Platforms[i], p.CircleSegments);
                CreateMesh("Platforms", vertices, indices, _art == null ? p.PlatformColor : Color.white, PlatformOrder);

                vertices = new List<Vector3>();
                indices = new List<int>();
                AddDisc(vertices, indices, layout.Platforms[layout.StartIndex], p.CircleSegments);
                CreateMesh("StartPlatform", vertices, indices, _art == null ? p.StartPlatformColor : Color.white, PlatformOrder);
                if (_art != null) CreateEdges(layout);
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

        private void CreateMesh(string name, List<Vector3> vertices, List<int> indices, Color color, int sortingOrder,
            bool ground = false, bool solid = false, List<Color> vertexColors = null,
            List<Vector2> bridgeUv = null, float bridgeHalfWidth = 0f)
        {
            if (vertices.Count == 0) return;
            var mesh = new Mesh { name = name, indexFormat = IndexFormat.UInt32 };
            _assets.Add(mesh);
            mesh.SetVertices(vertices);
            var colors = new Color[vertices.Count];
            for (var i = 0; i < colors.Length; i++)
            {
                var c = vertexColors == null ? color : vertexColors[i];
                colors[i] = QualitySettings.activeColorSpace == ColorSpace.Linear ? c.linear : c;
            }
            mesh.colors = colors;
            if (_art != null && !solid)
            {
                var uv = new List<Vector2>(vertices.Count);
                var repeat = ground ? _art.GroundRepeat : _art.SurfaceRepeat;
                foreach (var vertex in vertices) uv.Add((Vector2)vertex / repeat);
                mesh.SetUVs(0, bridgeUv ?? uv);
            }
            mesh.SetTriangles(indices, 0);
            mesh.RecalculateBounds();
            var shaderName = _art != null && !solid ? "SurvivorArena/FieldPlatformSurface" : "Sprites/Default";
            var shader = Shader.Find(shaderName) ?? throw new InvalidOperationException($"{shaderName} shader missing.");
            var material = new Material(shader);
            _assets.Add(material);
            if (_art != null && !solid)
            {
                if (bridgeUv != null)
                {
                    material.SetFloat("_BridgeVeil", 1f);
                    material.SetColor("_VeilColor", QualitySettings.activeColorSpace == ColorSpace.Linear ? _art.BridgeVeilColor.linear : _art.BridgeVeilColor);
                    material.SetColor("_ThreadColor", QualitySettings.activeColorSpace == ColorSpace.Linear ? _art.BridgeThreadColor.linear : _art.BridgeThreadColor);
                    material.SetColor("_EdgeColor", QualitySettings.activeColorSpace == ColorSpace.Linear ? _art.BridgeEdgeColor.linear : _art.BridgeEdgeColor);
                    material.SetVector("_Weave", new Vector4(_art.BridgeWeaveLength, _art.BridgeThreadWidth, _art.BridgeEdgeWidth, bridgeHalfWidth));
                }
                else
                {
                    material.mainTexture = _reference.texture;
                    var r = ground ? _art.GroundUvBounds : _art.SurfaceUvBounds;
                    material.SetVector("_UvBounds", new Vector4(r.xMin, r.yMin, r.xMax, r.yMax));
                }
            }
            var go = new GameObject(name);
            go.transform.SetParent(_root.transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingOrder = sortingOrder;
        }

        private void CreateEdges(FieldPlatformLayout layout)
        {
            // One outer contour of the union: platform circles never draw a rim across a bridge entrance.
            var field = FieldRoadDistanceField.Platforms(layout, _art.ContourStep);
            var contours = FieldRoadMarchingSquares.Contours(field, .01f);
            var rim = new List<Vector3>(); var gold = new List<Vector3>(); var faces = new List<Vector3>();
            var rimTriangles = new List<int>(); var goldTriangles = new List<int>(); var faceTriangles = new List<int>();
            var faceColors = new List<Color>();
            foreach (var contour in contours)
            {
                var count = contour.Length - 1;
                var normals = new Vector2[count];
                for (var i = 0; i < count; i++)
                {
                    var previous = (contour[i] - contour[(i + count - 1) % count]).normalized;
                    var next = (contour[(i + 1) % count] - contour[i]).normalized;
                    var normal = Vector2.Perpendicular(previous + next).normalized;
                    if (layout.IsWalkable(contour[i] + normal * _art.ContourStep)) normal = -normal;
                    normals[i] = normal;
                }
                for (var i = 0; i < count; i++)
                {
                    var j = (i + 1) % count;
                    var a = contour[i]; var b = contour[j]; var an = normals[i]; var bn = normals[j];
                    // D: only the circular plazas retain masonry. Bridge contours have no extruded face,
                    // rim or gold band; the veil shader marks the exact safe-width edges instead.
                    if (_art.BridgeVeil && !TouchesPlatform(layout, (a + b) * .5f)) continue;
                    AddQuad(rim, rimTriangles, a, b, b - bn * _art.RimWidth, a - an * _art.RimWidth);
                    var inset = _art.RimWidth * .65f;
                    AddQuad(gold, goldTriangles, a - an * inset, b - bn * inset,
                        b - bn * (inset + _art.InlayWidth), a - an * (inset + _art.InlayWidth));
                    var down = Vector2.down * _art.FaceHeight;
                    AddQuad(faces, faceTriangles, a, b, b + down, a + down);
                    var ac = _art.FaceColor; var bc = _art.FaceColor;
                    ac.a *= Mathf.Clamp01(-an.y); bc.a *= Mathf.Clamp01(-bn.y);
                    faceColors.Add(ac); faceColors.Add(bc); faceColors.Add(bc); faceColors.Add(ac);
                }
            }
            CreateMesh("PlatformFaces", faces, faceTriangles, _art.FaceColor, BridgeOrder - 1, solid: true, vertexColors: faceColors);
            CreateMesh("PlatformRim", rim, rimTriangles, _art.RimColor, PlatformOrder + 1, solid: true);
            CreateMesh("PlatformInlay", gold, goldTriangles, _art.InlayColor, PlatformOrder + 2, solid: true);
        }

        private bool TouchesPlatform(FieldPlatformLayout layout, Vector2 point)
        {
            // Marching-squares contours lie within one sampling step of the analytic circle.
            foreach (var disc in layout.Platforms)
                if ((point - disc.Center).sqrMagnitude <= (disc.Radius + _art.ContourStep) * (disc.Radius + _art.ContourStep))
                    return true;
            return false;
        }

        public void Dispose()
        {
            if (_root != null)
            {
                _root.SetActive(false);
                if (Application.isPlaying) Object.Destroy(_root); else Object.DestroyImmediate(_root);
                _root = null;
            }
            foreach (var asset in _assets)
                if (asset != null) { if (Application.isPlaying) Object.Destroy(asset); else Object.DestroyImmediate(asset); }
            _assets.Clear();
            _reference = null; _art = null;
        }
    }
}
