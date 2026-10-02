using Game.Zones;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Brief red fracture at the feet on actual zone damage. Cached on the visual rig; no gameplay changes.</summary>
    public sealed class ZoneRiftHitPresentationRuntime : MonoBehaviour
    {
        private SpritePresentationRuntime _presentation;
        private ZoneSealPresentationProfile _profile;
        private MeshRenderer _renderer;
        private Mesh _mesh;
        private Material _material;
        private float _until;
        public bool IsShowing => _renderer != null && _renderer.enabled;

        public void Show(SpritePresentationRuntime presentation, ZoneSealPresentationProfile profile, float runSeconds)
        {
            if (presentation == null || !presentation.IsInitialized) return;
            _presentation = presentation; _profile = profile;
            if (_renderer == null)
            {
                var child = new GameObject("Rift hit"); child.transform.SetParent(presentation.Rig.transform, false);
                _mesh = new ZoneSealMeshBuilder(profile.StrokeFraction).Glyph(ZoneEffectKind.Rift);
                child.AddComponent<MeshFilter>().sharedMesh = _mesh;
                _renderer = child.AddComponent<MeshRenderer>();
                _material = new Material(Resources.Load<Shader>("Shaders/SpriteSolidColor"))
                    { name = "Rift fracture", hideFlags = HideFlags.HideAndDontSave };
                _renderer.sharedMaterial = _material;
            }
            _until = runSeconds + profile.RiftHitSeconds;
            Tick(runSeconds);
        }

        public void Tick(float runSeconds)
        {
            if (!isActiveAndEnabled || _presentation == null || !_presentation.IsInitialized || runSeconds >= _until)
            { Clear(); return; }
            var body = _presentation.Rig.BodyRenderer;
            var bounds = body.bounds;
            var color = _profile.RiftHitColor;
            color.a = Mathf.Clamp01((_until - runSeconds) / _profile.RiftHitSeconds);
            _material.SetColor("_Color", color);
            _renderer.sortingLayerID = body.sortingLayerID; _renderer.sortingOrder = body.sortingOrder + 2;
            _renderer.transform.position = new Vector3(bounds.center.x, bounds.min.y + bounds.size.y * .15f, body.transform.position.z);
            var scale = _renderer.transform.parent.lossyScale;
            var size = bounds.size.x * _profile.RiftHitScale;
            _renderer.transform.localScale = new Vector3(size / Mathf.Max(.0001f, Mathf.Abs(scale.x)),
                size / Mathf.Max(.0001f, Mathf.Abs(scale.y)), 1f);
            _renderer.enabled = true;
        }

        public void Clear() { if (_renderer != null) _renderer.enabled = false; _until = 0f; }
        private void OnDisable() => Clear();

        private void OnDestroy()
        {
            Clear();
            if (_renderer != null) Release(_renderer.gameObject);
            if (_mesh != null) Release(_mesh);
            if (_material != null) Release(_material);
        }

        private static void Release(UnityEngine.Object value)
        {
            if (Application.isPlaying) Destroy(value); else DestroyImmediate(value);
        }
    }
}
