using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    /// <summary>Quiet gradient surface shared by non-gameplay screens.</summary>
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
