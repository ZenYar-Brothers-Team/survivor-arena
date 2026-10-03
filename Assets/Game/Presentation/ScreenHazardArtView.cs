using Game.ScreenEvents;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Pooled, visual-only screen hazard; Apply uses the authoritative paused event clock and dimensions.</summary>
    public sealed class ScreenHazardArtView : MonoBehaviour
    {
        private readonly MeshRenderer[] _layers = new MeshRenderer[4];
        private MaterialPropertyBlock _properties;
        private ScreenEventArtResources _resources;
        private ScreenHazard _hazard;
        private string _sweepKey;
        public ScreenHazard Hazard => _hazard;
        public ScreenHazardPhase Phase { get; private set; }

        public void Initialize(ScreenEventArtResources resources, ScreenHazard hazard, string sweepKey)
        {
            Shutdown();
            _properties ??= new MaterialPropertyBlock();
            _resources = resources;
            _hazard = hazard;
            _sweepKey = sweepKey;
            gameObject.name = "Hazard-" + hazard.Kind;
            for (var i = 0; i < _layers.Length; i++)
            {
                if (_layers[i] == null)
                {
                    var child = new GameObject(i == 0 ? "ArtZone" : i == 1 ? "ArtBody" : i == 2 ? "SafeRim" : "GapCorridor");
                    child.transform.SetParent(transform, false);
                    child.AddComponent<MeshFilter>().sharedMesh = resources.Quad;
                    _layers[i] = child.AddComponent<MeshRenderer>();
                }
                _layers[i].GetComponent<MeshFilter>().sharedMesh = resources.Quad;
                _layers[i].sortingOrder = resources.Profile.SortingOrder + (i == 1 || i == 2 ? 1 : 0);
            }
        }

        public void Apply(float eventSeconds, Rect screen)
        {
            Hide();
            if (_hazard == null) return;
            var h = _hazard;
            Phase = h.PhaseAt(eventSeconds);
            if (Phase != ScreenHazardPhase.Telegraph && Phase != ScreenHazardPhase.Strike) return;
            var profile = _resources.Profile;
            var striking = Phase == ScreenHazardPhase.Strike;
            var progress = h.TelegraphProgressAt(eventSeconds);
            var alpha = striking ? profile.StrikeAlpha : Mathf.Lerp(profile.WarningAlphaStart, profile.WarningAlphaEnd, progress);
            var fill = striking ? profile.StrikeColor : profile.WarningColor;
            fill.a = striking ? profile.StrikeFillAlpha : Mathf.Lerp(profile.WarningFillStart, profile.WarningFillEnd, progress);
            var strikeTime = h.StrikeTimeAt(eventSeconds);
            switch (h.Kind)
            {
                case ScreenHazardKind.Sweep:
                {
                    // Only the approaching part of the lane remains red after the head passes.
                    var head = striking ? h.SweepHeadAt(strikeTime) : 0f;
                    var warningFill = profile.WarningColor;
                    warningFill.a = Mathf.Lerp(profile.WarningFillStart, profile.WarningFillEnd, progress);
                    Draw(0, "warningStrip", h.Origin + h.Direction * (h.Length * .5f), h.Direction,
                        new Vector2(h.Length, h.Width), 0f, Mathf.Lerp(profile.WarningAlphaStart, profile.WarningAlphaEnd, progress), warningFill, laneMode: 2f, trimBefore: head);
                    if (striking)
                        Draw(1, _sweepKey, h.Origin + h.Direction * (head - h.BodyLength * .5f), h.Direction,
                            new Vector2(h.BodyLength, h.Width), 0f, alpha, Color.clear, sliced: false, laneMode: 1f);
                    break;
                }
                case ScreenHazardKind.Ring:
                {
                    var radius = h.RingRadiusAt(striking ? strikeTime : 0f);
                    var diameter = radius * 2f + h.Thickness;
                    Draw(0, striking ? "strikeStrip" : "warningStrip", h.Origin, Vector2.right,
                        Vector2.one * diameter, 3f, alpha, fill, radius);
                    var reach = h.Radius + h.Speed * h.StrikeSeconds;
                    var safeFill = profile.SafeColor;
                    safeFill.a = profile.CorridorAlpha;
                    Draw(3, "safeRing", h.Origin + h.Direction * (reach * .5f), h.Direction,
                        new Vector2(reach, h.GapWidth), 4f, profile.SafeAlpha, safeFill);
                    break;
                }
                default:
                    if (h.Shape == ScreenBurstShape.Rect)
                        Draw(0, striking ? "strikeStrip" : "warningStrip", h.Origin, h.Direction,
                            new Vector2(h.Length, h.Width), 0f, alpha, fill);
                    else if (h.Shape == ScreenBurstShape.Circle)
                        Draw(0, striking ? "strikeCircle" : "warningCircle", h.Origin, Vector2.right,
                            Vector2.one * h.Radius * 2f, 1f, alpha, fill, h.Radius, sliced: false);
                    else
                    {
                        // Rect-sized drawing, analytic safe-circle cutout: no giant 400-unit fill and no leaking tiles into safety.
                        Draw(0, striking ? "strikeStrip" : "warningStrip", screen.center, Vector2.right,
                            screen.size, 2f, alpha, fill, h.Radius);
                        Draw(2, "safeRing", h.Origin, Vector2.right, Vector2.one * h.Radius * 2f,
                            1f, profile.SafeAlpha, Color.clear, h.Radius, sliced: false);
                    }
                    break;
            }
        }

        private void Draw(int layer, string key, Vector2 center, Vector2 direction, Vector2 size, float shape,
            float alpha, Color fill, float radius = 0f, bool sliced = true, float laneMode = 0f, float trimBefore = 0f)
        {
            var renderer = _layers[layer];
            renderer.enabled = true;
            renderer.sharedMaterial = _resources.MaterialFor(key);
            var target = renderer.transform;
            target.localPosition = new Vector3(center.x, center.y, 0f);
            target.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            target.localScale = new Vector3(size.x, size.y, 1f);
            _properties.Clear();
            _properties.SetVector("_Size", size);
            _properties.SetVector("_Origin", _hazard.Origin);
            _properties.SetVector("_Direction", _hazard.Direction);
            _properties.SetVector("_Ring", new Vector4(radius, _hazard.Thickness, _hazard.GapWidth, 0f));
            _properties.SetVector("_Lane", new Vector4(_hazard.Length, laneMode, trimBefore, 0f));
            _properties.SetFloat("_Shape", shape);
            _properties.SetFloat("_Sliced", sliced ? 1f : 0f);
            _properties.SetColor("_Color", new Color(1f, 1f, 1f, alpha));
            _properties.SetColor("_FillColor", fill);
            renderer.SetPropertyBlock(_properties);
        }

        private void Hide()
        {
            foreach (var layer in _layers) if (layer != null) layer.enabled = false;
        }

        public void Shutdown()
        {
            Hide();
            foreach (var layer in _layers)
            {
                if (layer == null) continue;
                layer.sharedMaterial = null;
                layer.SetPropertyBlock(null);
                layer.transform.localPosition = Vector3.zero;
                layer.transform.localRotation = Quaternion.identity;
                layer.transform.localScale = Vector3.one;
            }
            _hazard = null;
            _resources = null;
            _sweepKey = null;
        }
    }
}
