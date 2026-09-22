using System;
using System.Collections.Generic;
using Game.Content;
namespace Game.UI
{
    public interface IFieldSelectView
    {
        event Action<ContentId> Selected;
        event Action StartRequested;
        event Action BackRequested;
        void Render(IReadOnlyList<FieldSelectCardViewState> cards, bool canStart);
    }
}
