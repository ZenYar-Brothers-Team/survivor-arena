using System;
using Game.Pickup;
namespace Game.UI
{
    public sealed class PickupPresenter : IDisposable
    {
        private readonly IPickupRuntime _model;
        private readonly IPickupView _view;
        private readonly bool _development;
        public PickupPresenter(IPickupRuntime model, IPickupView view, bool development)
        {
            _model = model; _view = view; _development = development;
            if (_model != null) _model.Changed += Refresh;
            _view.PotionRequested += Potion; _view.BookRequested += Book;
            Refresh();
        }
        public void Refresh() => _view.Render(_model?.Snapshot ?? default, _development && _model != null);
        private void Potion() { if (_development) _model?.DropDevelopmentPickup(PickupRewardKind.Potion); }
        private void Book() { if (_development) _model?.DropDevelopmentPickup(PickupRewardKind.Book); }
        public void Dispose()
        {
            if (_model != null) _model.Changed -= Refresh;
            _view.PotionRequested -= Potion; _view.BookRequested -= Book;
        }
    }
}
