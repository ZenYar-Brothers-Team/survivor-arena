using System;
using System.Collections.Generic;
using Game.Content;
namespace Game.UI
{
    public interface ICharacterSelectView
    {
        event Action<ContentId> Selected;
        event Action StartRequested;
        void Render(IReadOnlyList<CharacterSelectCardViewState> cards, bool canStart);
    }
}
