using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    internal sealed class MenuAtmosphereElement : VisualElement
    {
        private readonly MenuArtProfile _profile;
        private readonly bool _dust;
        public float Seconds { get; set; }
        public MenuAtmosphereElement(MenuArtProfile profile, bool dust)
        {
            _profile = profile; _dust = dust;
            pickingMode = PickingMode.Ignore;
            AddToClassList("entry-art-layer");
            generateVisualContent += Draw;
        }
        private void Draw(MeshGenerationContext context)
        {
            var w = contentRect.width; var h = contentRect.height;
            if (w < 1 || h < 1) return;
            var p = context.painter2D;
            if (_dust)
            {
                for (var i = 0; i < _profile.dustCount; i++)
                {
                    var cycle = _profile.DustCycle(i, Seconds);
                    var position = _profile.DustPosition(i, Seconds);
                    var x = w * position.x;
                    var y = h * position.y;
                    var radius = Mathf.Lerp(_profile.dustMinSize, _profile.dustMaxSize, (i % 7) / 6f) * w / 1920f / 2;
                    var alpha = Mathf.Sin(cycle * Mathf.PI) * _profile.dustOpacity;
                    for (var ring = 3; ring > 0; ring--)
                    {
                        p.fillColor = new Color(1, .937f, .77f, alpha * .22f);
                        p.BeginPath(); p.Arc(new Vector2(x, y), radius * ring / 2, 0, 360); p.Fill();
                    }
                }
                return;
            }
            for (var i = 0; i < 3; i++)
            {
                var phase = _profile.RayPhase(i, Seconds);
                var center = w * (.63f + i * .14f);
                var width = w * (i == 0 ? .129f : i == 1 ? .062f : .095f);
                var shift = Mathf.Tan((-15 + phase * _profile.rayAngle) * Mathf.Deg2Rad) * h;
                for (var band = 0; band < 16; band++)
                {
                    var u = band / 16f; var a = Mathf.Sin(u * Mathf.PI) * .18f * (.725f + phase * .125f);
                    p.fillColor = new Color(1, .937f, .77f, a);
                    var x = center + (u - .5f) * width + phase * width * .08f;
                    p.BeginPath(); p.MoveTo(new Vector2(x, 0)); p.LineTo(new Vector2(x + width / 16, 0));
                    p.LineTo(new Vector2(x + width / 16 + shift, h)); p.LineTo(new Vector2(x + shift, h)); p.ClosePath(); p.Fill();
                }
            }
        }
    }
}
