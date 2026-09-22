using UnityEngine;
namespace Game.Pickup
{
    /// <summary>Pooled static text marker on VisualRoot. No raster, animation, physics or authoritative state in the child.</summary>
    public sealed class WorldPickupVisual : MonoBehaviour
    {
        private TextMesh _text;
        public PickupLife Life { get; private set; }
        public void Initialize(PickupLife life, Vector2 position)
        {
            Life = life;
            transform.position = position; transform.rotation = Quaternion.identity; transform.localScale = Vector3.one;
            if (_text == null)
            {
                var child = new GameObject("VisualRoot"); child.transform.SetParent(transform, false);
                _text = child.AddComponent<TextMesh>();
                _text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                _text.GetComponent<MeshRenderer>().sharedMaterial = _text.font.material;
                _text.anchor = TextAnchor.MiddleCenter; _text.alignment = TextAlignment.Center; _text.fontSize = 64;
                _text.GetComponent<MeshRenderer>().sortingOrder = 5;
            }
            _text.transform.localPosition = Vector3.zero; _text.transform.localRotation = Quaternion.identity; _text.transform.localScale = Vector3.one;
            _text.text = life.Definition.Marker; _text.color = life.Definition.Color; _text.characterSize = life.Definition.MarkerSize;
            gameObject.name = life.Definition.Id.ToString();
        }
        public void Shutdown()
        {
            Life = null;
            if (_text != null) { _text.text = ""; _text.color = Color.white; _text.transform.localScale = Vector3.one; }
            transform.localScale = Vector3.one; transform.localRotation = Quaternion.identity;
        }
        public static WorldPickupVisual CreateInstance() => new GameObject("World pickup").AddComponent<WorldPickupVisual>();
    }
}
