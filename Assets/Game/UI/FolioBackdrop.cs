using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    /// <summary>Quiet procedural folio surface shared by non-gameplay screens.</summary>
    internal sealed class FolioBackdrop : VisualElement
    {
        public FolioBackdrop()
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList("folio-backdrop-visual");
            generateVisualContent += Draw;
        }

        public static void Attach(VisualElement parent)
        {
            if (parent == null || parent.Q<FolioBackdrop>() != null) return;
            parent.Insert(0, new FolioBackdrop());
        }

        private void Draw(MeshGenerationContext context)
        {
            var width = contentRect.width;
            var height = contentRect.height;
            if (width < 1f || height < 1f) return;

            var painter = context.painter2D;
            painter.fillGradient = FillGradient.MakeLinearGradient(
                new Color32(55, 45, 59, 255), new Color32(31, 25, 37, 255),
                Vector2.zero, new Vector2(width, height), AddressMode.Clamp);
            FillRect(painter, width, height);

            painter.fillGradient = FillGradient.MakeRadialGradient(
                BuildGradient(new Color(0.50f, 0.32f, 0.20f, 0.17f), new Color(0.16f, 0.11f, 0.18f, 0f)),
                new Vector2(width * 0.16f, height * 0.08f), Mathf.Max(width, height) * 0.72f,
                new Vector2(width * 0.11f, height * 0.04f), AddressMode.Clamp);
            FillRect(painter, width, height);

            DrawFrame(painter, width, height);
            DrawGrain(painter, width, height);
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

        private static void DrawFrame(Painter2D painter, float width, float height)
        {
            const float inset = 18f;
            const float corner = 72f;
            painter.strokeColor = new Color(0.75f, 0.57f, 0.34f, 0.19f);
            painter.lineWidth = 1.25f;
            painter.BeginPath();
            painter.MoveTo(new Vector2(inset, inset + corner));
            painter.LineTo(new Vector2(inset, inset));
            painter.LineTo(new Vector2(inset + corner, inset));
            painter.MoveTo(new Vector2(width - inset - corner, inset));
            painter.LineTo(new Vector2(width - inset, inset));
            painter.LineTo(new Vector2(width - inset, inset + corner));
            painter.MoveTo(new Vector2(inset, height - inset - corner));
            painter.LineTo(new Vector2(inset, height - inset));
            painter.LineTo(new Vector2(inset + corner, height - inset));
            painter.MoveTo(new Vector2(width - inset - corner, height - inset));
            painter.LineTo(new Vector2(width - inset, height - inset));
            painter.LineTo(new Vector2(width - inset, height - inset - corner));
            painter.Stroke();

            painter.strokeColor = new Color(0.89f, 0.78f, 0.60f, 0.08f);
            painter.lineWidth = 1f;
            painter.BeginPath();
            painter.MoveTo(new Vector2(width * 0.28f, 30f));
            painter.LineTo(new Vector2(width * 0.72f, 30f));
            painter.MoveTo(new Vector2(width * 0.28f, height - 30f));
            painter.LineTo(new Vector2(width * 0.72f, height - 30f));
            painter.Stroke();
        }

        private static void DrawGrain(Painter2D painter, float width, float height)
        {
            painter.strokeColor = new Color(0.90f, 0.80f, 0.67f, 0.075f);
            painter.lineWidth = 1f;
            painter.BeginPath();
            for (var i = 0; i < 54; i++)
            {
                var x = 32f + ((i * 197) % Mathf.Max(1, (int)(width - 64f)));
                var y = 28f + ((i * 113) % Mathf.Max(1, (int)(height - 56f)));
                var length = 1f + i % 3;
                painter.MoveTo(new Vector2(x, y));
                painter.LineTo(new Vector2(x + length, y + (i % 2 == 0 ? 0.5f : -0.5f)));
            }
            painter.Stroke();
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
