using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    /// <summary>
    /// Off-screen Traveler pointer (DECISION-0109): a filled arrowhead pointing right, rotated by the owner.
    /// Fill comes from the USS `color`, outline from `border-color`, so the look stays in the stylesheet.
    /// </summary>
    public sealed class TravelerPointerArrow : VisualElement
    {
        public TravelerPointerArrow()
        {
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
        }

        private void Draw(MeshGenerationContext context)
        {
            var rect = contentRect;
            if (rect.width <= 0f || rect.height <= 0f) return;
            var painter = context.painter2D;
            var w = rect.width;
            var h = rect.height;
            painter.BeginPath();
            painter.MoveTo(new Vector2(w, h * .5f));
            painter.LineTo(new Vector2(0f, 0f));
            painter.LineTo(new Vector2(w * .3f, h * .5f));
            painter.LineTo(new Vector2(0f, h));
            painter.ClosePath();
            painter.fillColor = resolvedStyle.color;
            painter.Fill();
            painter.strokeColor = resolvedStyle.borderTopColor;
            painter.lineWidth = 2f;
            painter.lineJoin = LineJoin.Round;
            painter.Stroke();
        }
    }
}
