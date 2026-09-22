using System;
namespace Game.UI.Tests
{
    public sealed class FakeMetaView : IMetaView
    {
        public MetaViewState State;
        public event Action ShopRequested, CloseRequested, RetryRequested, SelectionRequested, QuitRequested, SaveRequested, ResetRequested;
        public event Action<string> CharacterRequested;
        public event Action<MetaCardViewState> PurchaseRequested;
        public void Render(MetaViewState state) => State = state;
        public void Shop() => ShopRequested?.Invoke();
        public void Buy(MetaCardViewState card) => PurchaseRequested?.Invoke(card);
        public void Select(string id) => CharacterRequested?.Invoke(id);
        public void Close() => CloseRequested?.Invoke();
        public void Retry() => RetryRequested?.Invoke();
        public void Selection() => SelectionRequested?.Invoke();
        public void Quit() => QuitRequested?.Invoke();
        public void Save() => SaveRequested?.Invoke();
        public void Reset() => ResetRequested?.Invoke();
    }
}
