using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    /// <summary>
    /// Off-screen Traveler pointer (DECISION-0109): a filled arrowhead drawn already rotated toward the
    /// Traveler (degrees clockwise from «right», screen y down). Rotation is applied to the geometry itself,
    /// not through a style transform, so the pointer always turns with the Traveler's direction.
    /// Fill comes from the USS `color`, outline from `border-color`.
    /// </summary>
    public sealed class TravelerPointerArrow : VisualElement
    {
        private float _angleDegrees;

        public TravelerPointerArrow()
        {
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
        }

        public float AngleDegrees
        {
            get => _angleDegrees;
            set
            {
                if (Mathf.Approximately(_angleDegrees, value)) return;
                _angleDegrees = value;
                MarkDirtyRepaint();
            }
        }

        private void Draw(MeshGenerationContext context)
        {
            var rect = contentRect;
            if (rect.width <= 0f || rect.height <= 0f) return;
            var center = rect.center;
            var half = Mathf.Min(rect.width, rect.height) * .5f;
            var radians = _angleDegrees * Mathf.Deg2Rad;
            var cos = Mathf.Cos(radians);
            var sin = Mathf.Sin(radians);
            Vector2 Point(float x, float y) => center + new Vector2(x * cos - y * sin, x * sin + y * cos) * half;
            var painter = context.painter2D;
            painter.BeginPath();
            painter.MoveTo(Point(.95f, 0f));
            painter.LineTo(Point(-.75f, -.72f));
            painter.LineTo(Point(-.35f, 0f));
            painter.LineTo(Point(-.75f, .72f));
            painter.ClosePath();
            painter.fillColor = resolvedStyle.color;
            painter.Fill();
            painter.strokeColor = resolvedStyle.borderTopColor;
            painter.lineWidth = 2.5f;
            painter.lineJoin = LineJoin.Round;
            painter.Stroke();
        }
    }
}
