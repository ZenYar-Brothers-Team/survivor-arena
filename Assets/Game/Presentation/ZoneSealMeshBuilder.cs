using System.Collections.Generic;
using Game.Zones;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Cutout seal strokes, kept inside a unit circle. Builds once; animation moves child meshes only.</summary>
    public sealed class ZoneSealMeshBuilder
    {
        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<int> _indices = new List<int>();
        private readonly float _width;

        public ZoneSealMeshBuilder(float width) => _width = width;

        public Mesh Boundary(ZoneEffectKind kind)
        {
            Reset();
            Arc(.97f, 0f, 360f, 96, false);
            for (var i = 0; i < 8; i++)
            {
                var angle = i * 45f;
                Arc(.86f, angle + 5f, angle + 28f, 8, false);
                var a = Point(.83f, angle + 36f);
                var b = Point(.92f, angle + 36f);
                Line(a, b);
                Line(b, Point(.87f, angle + (kind == ZoneEffectKind.Rift ? 30f : 40f)));
            }
            return Finish("Zone seal boundary");
        }

        public Mesh Glyph(ZoneEffectKind kind)
        {
            Reset();
            switch (kind)
            {
                case ZoneEffectKind.Slow:
                    for (var i = 0; i < 6; i++)
                    {
                        var angle = i * 60f;
                        Line(Vector2.zero, Point(.3f, angle));
                        Line(Point(.18f, angle), Point(.25f, angle - 22f));
                        Line(Point(.18f, angle), Point(.25f, angle + 22f));
                    }
                    break;
                case ZoneEffectKind.Haste:
                    for (var i = 0; i < 3; i++)
                        Path(new Vector2(-.32f, -.2f + i * .19f), new Vector2(.12f, -.2f + i * .19f),
                            new Vector2(.28f, -.1f + i * .19f), new Vector2(.15f, i * .19f));
                    break;
                case ZoneEffectKind.Regeneration:
                    Path(new Vector2(0f, -.3f), new Vector2(0f, .24f));
                    Path(new Vector2(0f, -.04f), new Vector2(-.24f, .08f), new Vector2(-.28f, .27f), new Vector2(-.08f, .22f), Vector2.zero);
                    Path(new Vector2(0f, -.16f), new Vector2(.25f, -.03f), new Vector2(.28f, .17f), new Vector2(.08f, .11f), new Vector2(0f, -.1f));
                    Arc(.33f, 195f, 345f, 18, false);
                    break;
                case ZoneEffectKind.ArcanePower:
                    Star(6, .33f, .15f);
                    Arc(.1f, 0f, 360f, 20, false);
                    break;
                case ZoneEffectKind.Rift:
                    Path(new Vector2(-.16f, .34f), new Vector2(.08f, .14f), new Vector2(-.11f, -.04f), new Vector2(.18f, -.34f));
                    Path(new Vector2(.08f, .14f), new Vector2(.3f, .19f));
                    Path(new Vector2(-.11f, -.04f), new Vector2(-.3f, -.16f));
                    break;
                case ZoneEffectKind.Portal:
                    Arc(.29f, 0f, 310f, 40, false);
                    Arc(.16f, 65f, 360f, 30, false);
                    Line(Point(.29f, 310f), Vector2.zero);
                    break;
                case ZoneEffectKind.SpeedBurst:
                    for (var i = 0; i < 2; i++)
                        Path(new Vector2(-.25f + i * .22f, -.28f), new Vector2(-.03f + i * .22f, 0f), new Vector2(-.25f + i * .22f, .28f));
                    break;
                case ZoneEffectKind.Protection:
                    Path(new Vector2(-.28f, .22f), new Vector2(.28f, .22f), new Vector2(.24f, -.08f),
                        new Vector2(0f, -.33f), new Vector2(-.24f, -.08f), new Vector2(-.28f, .22f));
                    Path(new Vector2(0f, .15f), new Vector2(0f, -.2f));
                    break;
            }
            return Finish("Zone seal " + kind);
        }

        public Mesh Motion(ZoneEffectKind kind, float radius)
        {
            Reset();
            var sharp = kind == ZoneEffectKind.Rift;
            for (var i = 0; i < 3; i++)
            {
                var angle = i * 120f;
                Arc(radius, angle, angle + 68f, 18, sharp);
                if (kind == ZoneEffectKind.Slow)
                    Path(Point(radius - .08f, angle), Point(radius + .03f, angle + 8f), Point(radius - .06f, angle + 17f));
                else if (kind == ZoneEffectKind.Protection)
                    Path(Point(radius, angle), Point(radius - .12f, angle + 8f), Point(radius, angle + 16f));
                else
                    Line(Point(radius, angle + 68f), Point(radius - .1f, angle + 62f));
            }
            return Finish("Zone seal moving arcs");
        }

        private void Star(int tips, float outer, float inner)
        {
            for (var i = 0; i < tips * 2; i++)
                Line(Point(i % 2 == 0 ? outer : inner, i * 180f / tips),
                    Point(i % 2 == 0 ? inner : outer, (i + 1) * 180f / tips));
        }

        private void Arc(float radius, float from, float to, int segments, bool sharp)
        {
            var previous = Point(radius, from);
            for (var i = 1; i <= segments; i++)
            {
                var angle = Mathf.Lerp(from, to, i / (float)segments);
                // Slight fixed waviness, never per-frame jitter, gives the drawn contour a cutout character.
                var offset = sharp ? (i % 2 == 0 ? -.035f : 0f) : .004f * Mathf.Sin(angle * .17f);
                var next = Point(radius + offset, angle);
                Line(previous, next);
                previous = next;
            }
        }

        private static Vector2 Point(float radius, float degrees) =>
            new Vector2(Mathf.Cos(degrees * Mathf.Deg2Rad), Mathf.Sin(degrees * Mathf.Deg2Rad)) * radius;

        private void Path(params Vector2[] points)
        {
            for (var i = 1; i < points.Length; i++) Line(points[i - 1], points[i]);
        }

        private void Line(Vector2 a, Vector2 b)
        {
            var normal = new Vector2(-(b - a).y, (b - a).x).normalized * (_width * .5f);
            var first = _vertices.Count;
            _vertices.Add(a + normal); _vertices.Add(b + normal); _vertices.Add(b - normal); _vertices.Add(a - normal);
            _indices.Add(first); _indices.Add(first + 1); _indices.Add(first + 2);
            _indices.Add(first); _indices.Add(first + 2); _indices.Add(first + 3);
        }

        private void Reset() { _vertices.Clear(); _indices.Clear(); }

        private Mesh Finish(string name)
        {
            var mesh = new Mesh { name = name, hideFlags = HideFlags.HideAndDontSave };
            mesh.SetVertices(_vertices); mesh.SetTriangles(_indices, 0); mesh.RecalculateBounds();
            return mesh;
        }
    }
}
