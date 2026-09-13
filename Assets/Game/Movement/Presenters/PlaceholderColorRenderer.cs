using UnityEngine;

namespace Game.Movement
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlaceholderColorRenderer : MonoBehaviour
    {
        [SerializeField]
        private Color color = Color.white;

        private void Awake()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = PlaceholderSprite.Shared;
            spriteRenderer.color = color;
        }
    }
}
