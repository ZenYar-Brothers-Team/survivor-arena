using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    /// <summary>Development map pane plus a corner overlay that draws the arena frame, obstacle outlines and the camera frame.</summary>
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
            Frame(painter, ToPanel, state.View, ViewColor, 2f);
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
