using System.Collections.Generic;
using Game.Presentation;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// Draws one boss life's zones, beams and summon markers (DECISION-0066) from <see cref="BossHazardField.Visuals"/>
    /// with procedural shapes only: rings mark zone edges and markers, discs fill zones and burning ground, the beam band
    /// draws beams. Colors come from the attack profiles; timing comes from the domain, so pause freezes the picture.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BossHazardPresentation : MonoBehaviour
    {
        /// <summary>Same ground layer as the teleport marker: above ground shadows, below player skill world effects.</summary>
        public const int SortingOrder = BossTeleportPresentation.SortingOrder;
        private readonly List<SpriteRenderer> _renderers = new List<SpriteRenderer>();

        public int VisibleShapes { get; private set; }

        /// <param name="viewCenter">World center of the gameplay view; the safe-circles danger area covers it.</param>
        /// <param name="viewRadius">Radius that covers the whole gameplay view.</param>
        public void Render(IReadOnlyList<BossHazardVisual> visuals, Vector2 viewCenter, float viewRadius)
        {
            var used = 0;
            if (visuals != null)
                foreach (var visual in visuals)
                    Draw(Renderer(used++), visual, viewCenter, viewRadius);
            for (var i = used; i < _renderers.Count; i++) _renderers[i].enabled = false;
            VisibleShapes = used;
        }

        public void ResetPresentation() => Render(null, Vector2.zero, 0f);

        private static void Draw(SpriteRenderer renderer, BossHazardVisual visual, Vector2 viewCenter, float viewRadius)
        {
            var t = visual.Progress;
            var color = visual.Color;
            var diameter = visual.Radius * 2f;
            var shape = renderer.transform;
            shape.rotation = Quaternion.identity;
            shape.position = visual.Center;
            renderer.sortingOrder = SortingOrder;
            switch (visual.Kind)
            {
                case BossHazardVisualKind.ZoneEdge:
                    // Pulses faster as the hit approaches, like the teleport marker.
                    var pulse = .65f + .35f * Mathf.Abs(Mathf.Sin(t * Mathf.PI * (3f + 6f * t)));
                    Set(renderer, ProceduralShapeSprites.Ring, Vector2.one * diameter, WithAlpha(color, color.a * pulse), SortingOrder + 1);
                    break;
                case BossHazardVisualKind.ZoneFill:
                    Set(renderer, ProceduralShapeSprites.Disc, Vector2.one * diameter * Mathf.Lerp(.15f, 1f, t),
                        WithAlpha(color, color.a * .35f), SortingOrder);
                    break;
                case BossHazardVisualKind.ZoneImpact:
                    var ease = 1f - (1f - t) * (1f - t);
                    Set(renderer, ProceduralShapeSprites.Ring, Vector2.one * diameter * Mathf.Lerp(.45f, 1.2f, ease),
                        WithAlpha(color, color.a * (1f - t)), SortingOrder + 1);
                    break;
                case BossHazardVisualKind.Burning:
                    Set(renderer, ProceduralShapeSprites.Disc, Vector2.one * diameter, WithAlpha(color, color.a * .35f * (1f - .5f * t)),
                        SortingOrder);
                    break;
                case BossHazardVisualKind.DangerWash:
                    shape.position = viewCenter;
                    Set(renderer, ProceduralShapeSprites.Disc, Vector2.one * viewRadius * 2f, WithAlpha(color, color.a * .45f * t),
                        SortingOrder);
                    break;
                case BossHazardVisualKind.SafeCircle:
                    Set(renderer, ProceduralShapeSprites.Ring, Vector2.one * diameter, color, SortingOrder + 2);
                    break;
                case BossHazardVisualKind.BeamTelegraph:
                case BossHazardVisualKind.BeamActive:
                    var active = visual.Kind == BossHazardVisualKind.BeamActive;
                    // The warning line thickens toward the shot; the active beam has its full hit width.
                    var width = active ? visual.Width : visual.Width * Mathf.Lerp(.08f, .3f, t);
                    shape.rotation = Quaternion.Euler(0f, 0f, visual.AngleDegrees);
                    Set(renderer, ProceduralShapeSprites.Beam, new Vector2(visual.Length, width), color, SortingOrder + 1);
                    break;
                case BossHazardVisualKind.SummonMarker:
                    Set(renderer, ProceduralShapeSprites.Ring, Vector2.one * diameter * Mathf.Lerp(.5f, 1f, t), color, SortingOrder + 1);
                    break;
            }
        }

        private static void Set(SpriteRenderer renderer, Sprite sprite, Vector2 size, Color color, int order)
        {
            renderer.sprite = sprite;
            renderer.transform.localScale = new Vector3(size.x, size.y, 1f);
            renderer.color = color;
            renderer.sortingOrder = order;
            renderer.enabled = true;
        }

        private static Color WithAlpha(Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);

        private SpriteRenderer Renderer(int index)
        {
            while (_renderers.Count <= index)
            {
                var child = new GameObject("BossHazardShape");
                child.transform.SetParent(transform, false);
                var renderer = child.AddComponent<SpriteRenderer>();
                renderer.enabled = false;
                _renderers.Add(renderer);
            }
            return _renderers[index];
        }
    }
}
