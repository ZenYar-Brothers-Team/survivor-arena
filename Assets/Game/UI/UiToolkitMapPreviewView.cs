using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Game.Diagnostics;

namespace Game.UI
{
    /// <summary>Development map pane plus a corner overlay that draws roads, the arena frame, obstacle outlines and the camera frame.</summary>
    public sealed class UiToolkitMapPreviewView : IMapPreviewView, IDisposable
    {
        private const float Padding = 12f;
        private static readonly Color ArenaColor = new Color(.85f, .88f, .93f, 1f);
        private static readonly Color ObstacleFill = new Color(.45f, .5f, .58f, .95f);
        private static readonly Color ObstacleEdge = new Color(.12f, .14f, .18f, 1f);
        private static readonly Color ViewColor = new Color(1f, .62f, .18f, 1f);

        private readonly Button _toggle;
        private readonly Label _summary;
        private readonly VisualElement _overlay;
        private MapPreviewViewState _state = MapPreviewViewState.Hidden;
        public event Action ToggleRequested;

        public UiToolkitMapPreviewView(VisualElement root)
        {
            _toggle = root.Q<Button>(GameplayUiElementIds.MapToggle);
            _summary = root.Q<Label>(GameplayUiElementIds.MapSummary);
            _overlay = root.Q(GameplayUiElementIds.MapOverlay);
            _toggle.clicked += Toggle;
            _overlay.generateVisualContent += Draw;
        }

        private void Toggle() => ToggleRequested?.Invoke();

        public void Render(MapPreviewViewState state)
        {
            _state = state;
            _toggle.SetEnabled(state.Available);
            _toggle.text = state.Visible ? "Скрыть карту" : "Показать карту";
            if (_summary.text != state.Summary) _summary.text = state.Summary;
            _overlay.style.display = state.Visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (state.Visible) _overlay.MarkDirtyRepaint();
        }

        private void Draw(MeshGenerationContext context)
        {
            var state = _state;
            if (!state.Visible || state.Obstacles == null || state.Arena.width <= 0f || state.Arena.height <= 0f) return;
            var size = _overlay.contentRect.size;
            var scale = Mathf.Min((size.x - 2f * Padding) / state.Arena.width, (size.y - 2f * Padding) / state.Arena.height);
            if (scale <= 0f) return;
            var origin = new Vector2((size.x - state.Arena.width * scale) * .5f, (size.y + state.Arena.height * scale) * .5f);
            Vector2 ToPanel(Vector2 world) => new Vector2(origin.x + (world.x - state.Arena.xMin) * scale,
                origin.y - (world.y - state.Arena.yMin) * scale);
            var painter = context.painter2D;

            if (state.Roads != null) DrawRoads(painter, ToPanel, state.Roads, scale);
            foreach (var outline in state.Obstacles)
            {
                if (outline == null || outline.Length < 3) continue;
                painter.BeginPath();
                painter.MoveTo(ToPanel(outline[0]));
                for (var i = 1; i < outline.Length; i++) painter.LineTo(ToPanel(outline[i]));
                painter.ClosePath();
                painter.fillColor = ObstacleFill;
                painter.Fill();
                painter.strokeColor = ObstacleEdge;
                painter.lineWidth = 1f;
                painter.Stroke();
            }
            Frame(painter, ToPanel, state.Arena, ArenaColor, 2f);
            if (state.Altars != null) DrawAltars(painter, ToPanel, state.Altars, scale);
            if (state.Traps != null) DrawTraps(painter, ToPanel, state.Traps);
            Frame(painter, ToPanel, state.View, ViewColor, 2f);
        }

        // Turrets are red diamonds, barrels small dots (explosive ones orange); a blown barrel is only an outline.
        private static void DrawTraps(Painter2D painter, Func<Vector2, Vector2> toPanel, IReadOnlyList<MapPreviewTrap> traps)
        {
            using var guard = PerfGuard.Measure("UI.MapTraps", 2f);
            foreach (var trap in traps)
            {
                var center = toPanel(trap.Center);
                painter.BeginPath();
                if (trap.Barrel)
                {
                    painter.Arc(center, 2.5f, Angle.Degrees(0f), Angle.Degrees(360f));
                    var color = trap.Explosive ? new Color(1f, .55f, .1f, 1f) : new Color(.62f, .45f, .3f, 1f);
                    if (trap.Intact) { painter.fillColor = color; painter.Fill(); }
                    else { color.a = .5f; painter.strokeColor = color; painter.lineWidth = 1f; painter.Stroke(); }
                    continue;
                }
                painter.MoveTo(center + new Vector2(0f, -5f));
                painter.LineTo(center + new Vector2(5f, 0f));
                painter.LineTo(center + new Vector2(0f, 5f));
                painter.LineTo(center + new Vector2(-5f, 0f));
                painter.ClosePath();
                painter.fillColor = new Color(.95f, .15f, .12f, 1f);
                painter.Fill();
                painter.strokeColor = new Color(0f, 0f, 0f, .7f);
                painter.lineWidth = 1f;
                painter.Stroke();
            }
        }

        private static void DrawAltars(Painter2D painter, Func<Vector2, Vector2> toPanel,
            IReadOnlyList<MapPreviewAltar> altars, float scale)
        {
            using var guard = PerfGuard.Measure("UI.MapAltars", 2f);
            foreach (var altar in altars)
            {
                var center = toPanel(altar.Center);
                var color = altar.Color;
                painter.BeginPath();
                painter.Arc(center, altar.Radius * scale, Angle.Degrees(0f), Angle.Degrees(360f));
                color.a = altar.Active ? .18f : .07f;
                painter.fillColor = color; painter.Fill();
                color.a = altar.Active ? .8f : .4f;
                painter.strokeColor = color; painter.lineWidth = 1f; painter.Stroke();
                painter.BeginPath();
                if (!altar.Negative)
                    painter.Arc(center, 3f, Angle.Degrees(0f), Angle.Degrees(360f));
                else
                {
                    for (var i = 0; i < 16; i++)
                    {
                        var angle = i * Mathf.PI / 8f;
                        var radius = i % 2 == 0 ? 4f : 2.5f;
                        var point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                        if (i == 0) painter.MoveTo(point); else painter.LineTo(point);
                    }
                    painter.ClosePath();
                }
                color.a = altar.Active ? 1f : .65f;
                painter.fillColor = color; painter.Fill();
            }
        }

        // Roads are stroked at their world width so the map shows the actual walkable network; a single point is a round end.
        private static void DrawRoads(Painter2D painter, Func<Vector2, Vector2> toPanel, IReadOnlyList<MapPreviewRoad> roads, float scale)
        {
            painter.lineCap = LineCap.Round;
            painter.lineJoin = LineJoin.Round;
            foreach (var road in roads)
            {
                painter.BeginPath();
                if (road.Points.Count == 1)
                {
                    painter.Arc(toPanel(road.Points[0]), road.Width * .5f * scale, Angle.Degrees(0f), Angle.Degrees(360f));
                    painter.fillColor = road.Color;
                    painter.Fill();
                    continue;
                }
                painter.MoveTo(toPanel(road.Points[0]));
                for (var i = 1; i < road.Points.Count; i++) painter.LineTo(toPanel(road.Points[i]));
                painter.strokeColor = road.Color;
                painter.lineWidth = road.Width * scale;
                painter.Stroke();
            }
            painter.lineCap = LineCap.Butt;
            painter.lineJoin = LineJoin.Miter;
        }

        private static void Frame(Painter2D painter, Func<Vector2, Vector2> toPanel, Rect rect, Color color, float width)
        {
            painter.BeginPath();
            painter.MoveTo(toPanel(new Vector2(rect.xMin, rect.yMin)));
            painter.LineTo(toPanel(new Vector2(rect.xMax, rect.yMin)));
            painter.LineTo(toPanel(new Vector2(rect.xMax, rect.yMax)));
            painter.LineTo(toPanel(new Vector2(rect.xMin, rect.yMax)));
            painter.ClosePath();
            painter.strokeColor = color;
            painter.lineWidth = width;
            painter.Stroke();
        }

        public void Dispose()
        {
            _toggle.clicked -= Toggle;
            _overlay.generateVisualContent -= Draw;
        }
    }
}
