using UnityEngine;
using Game.Presentation;
namespace Game.Pickup
{
    /// <summary>Pooled sprite presentation with a text fallback; all motion stays on a non-authoritative child.</summary>
    public sealed class WorldPickupVisual : MonoBehaviour
    {
        private TextMesh _text;
        private PickupSpritePresentation _sprite;
        public PickupLife Life { get; private set; }
        public void Initialize(PickupLife life, Vector2 position, SpriteDefinition visual)
        {
            Life = life;
            transform.position = position; transform.rotation = Quaternion.identity; transform.localScale = Vector3.one;
            if (_text == null)
            {
                var child = new GameObject("FallbackText"); child.transform.SetParent(transform, false);
                _text = child.AddComponent<TextMesh>();
                _text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                _text.GetComponent<MeshRenderer>().sharedMaterial = _text.font.material;
                _text.anchor = TextAnchor.MiddleCenter; _text.alignment = TextAlignment.Center; _text.fontSize = 64;
                _text.GetComponent<MeshRenderer>().sortingOrder = 5;
            }
            _text.transform.localPosition = Vector3.zero; _text.transform.localRotation = Quaternion.identity; _text.transform.localScale = Vector3.one;
            if (visual != null)
            {
                visual.RequireRole(SpriteRole.Pickup);
                _sprite ??= new PickupSpritePresentation(transform, 5);
                _sprite.Initialize(visual.Sprite, life.Definition.VisualScale, Color.white,
                    Mathf.Repeat(position.x * 1.7f + position.y * 2.3f, Mathf.PI * 2f));
                _text.text = ""; _text.GetComponent<MeshRenderer>().enabled = false;
            }
            else
            {
                _sprite?.Shutdown();
                _text.GetComponent<MeshRenderer>().enabled = true;
                _text.text = life.Definition.Marker; _text.color = life.Definition.Color; _text.characterSize = life.Definition.MarkerSize;
            }
            gameObject.name = life.Definition.Id.ToString();
        }
        public void TickPresentation(float deltaTime) => _sprite?.Tick(deltaTime);
        public void Shutdown()
        {
            Life = null;
            if (_text != null) { _text.text = ""; _text.color = Color.white; _text.transform.localScale = Vector3.one; }
            _sprite?.Shutdown();
            transform.localScale = Vector3.one; transform.localRotation = Quaternion.identity;
        }
        public static WorldPickupVisual CreateInstance() => new GameObject("World pickup").AddComponent<WorldPickupVisual>();
    }
}
