using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    /// <summary>Transparent paper-fibre treatment for broad folio panels.</summary>
    internal sealed class FolioPanelTexture : VisualElement
    {
        public FolioPanelTexture()
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList("folio-panel-texture");
            generateVisualContent += Draw;
        }

        public static void Attach(VisualElement parent)
        {
            if (parent == null || parent.Q<FolioPanelTexture>() != null) return;
            parent.Insert(0, new FolioPanelTexture());
        }

        private void Draw(MeshGenerationContext context)
        {
            var width = contentRect.width;
            var height = contentRect.height;
            if (width < 1f || height < 1f) return;

            var painter = context.painter2D;
            painter.fillGradient = FillGradient.MakeLinearGradient(
                BuildGradient(new Color(0.93f, 0.79f, 0.58f, 0.035f), new Color(0.20f, 0.14f, 0.22f, 0f)),
                Vector2.zero, new Vector2(0, height * 0.55f), AddressMode.Clamp);
            FillRect(painter, width, height);

            DrawFibres(painter, width, height);
        }

        private static void DrawFibres(Painter2D painter, float width, float height)
        {
            painter.strokeColor = new Color(0.92f, 0.82f, 0.67f, 0.055f);
            painter.lineWidth = 0.75f;
            painter.BeginPath();
            for (var i = 0; i < 78; i++)
            {
                var x = 14f + ((i * 223) % Mathf.Max(1, (int)(width - 28f)));
                var y = 12f + ((i * 139) % Mathf.Max(1, (int)(height - 24f)));
                var length = 7f + (i * 17) % 24;
                var rise = ((i * 7) % 5 - 2) * 0.7f;
                painter.MoveTo(new Vector2(x, y));
                painter.QuadraticCurveTo(
                    new Vector2(x + length * 0.52f, y + rise),
                    new Vector2(Mathf.Min(width - 8f, x + length), y + rise * 0.35f));
            }
            painter.Stroke();

            painter.strokeColor = new Color(0.12f, 0.08f, 0.14f, 0.07f);
            painter.lineWidth = 0.6f;
            painter.BeginPath();
            for (var i = 0; i < 24; i++)
            {
                var x = 20f + ((i * 311) % Mathf.Max(1, (int)(width - 40f)));
                var y = 18f + ((i * 181) % Mathf.Max(1, (int)(height - 36f)));
                painter.MoveTo(new Vector2(x, y));
                painter.LineTo(new Vector2(Mathf.Min(width - 8f, x + 18f + i % 13), y + (i % 3 - 1)));
            }
            painter.Stroke();
        }

        private static void FillRect(Painter2D painter, float width, float height)
        {
            painter.BeginPath();
            painter.MoveTo(Vector2.zero);
            painter.LineTo(new Vector2(width, 0));
            painter.LineTo(new Vector2(width, height));
            painter.LineTo(new Vector2(0, height));
            painter.ClosePath();
            painter.Fill();
        }

        private static Gradient BuildGradient(Color start, Color end)
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(start, 0f), new GradientColorKey(end, 1f) },
                new[] { new GradientAlphaKey(start.a, 0f), new GradientAlphaKey(end.a, 1f) });
            return gradient;
        }
    }
}
