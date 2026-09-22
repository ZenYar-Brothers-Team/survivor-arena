using System;
namespace Game.UI
{
    public interface IMetaView
    {
        event Action ShopRequested;
        event Action CloseRequested;
        event Action RetryRequested;
        event Action SelectionRequested;
        event Action QuitRequested;
        event Action SaveRequested;
        event Action ResetRequested;
        event Action<string> CharacterRequested;
        event Action<MetaCardViewState> PurchaseRequested;
        void Render(MetaViewState state);
    }
}
