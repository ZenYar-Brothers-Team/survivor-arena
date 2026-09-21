using Game.Content;
using UnityEngine;
namespace Game.UI
{
    public sealed class CharacterSelectCardViewState
    {
        public ContentId Id { get; }
        public ContentCardViewState Card { get; }
        public Sprite Crop { get; }
        public CharacterSelectCardViewState(ContentId id, ContentCardViewState card, Sprite crop)
        {
            Id = id; Card = card; Crop = crop;
        }
    }
}
