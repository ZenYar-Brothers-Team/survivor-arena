using Game.Content;
using Game.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    /// <summary>Matte material surface for broad folio panels.</summary>
    internal sealed class FolioPanelTexture : VisualElement
    {
        private static readonly ContentId SurfaceId = "UI-FOLIO-SURFACE-VISUAL-BACKGROUND";
        private static Sprite _surface;

        public FolioPanelTexture()
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList("folio-panel-texture");
            if (_surface == null)
            {
                var definition = FixtureSpriteCatalog.CreateFor(new[] { SurfaceId })[0];
                definition.RequireRole(SpriteRole.Background);
                _surface = definition.Sprite;
            }
            style.backgroundImage = new StyleBackground(_surface);
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
