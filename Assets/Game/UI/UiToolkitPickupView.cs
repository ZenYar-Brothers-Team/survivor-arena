using System;
using Game.Pickup;
using UnityEngine.UIElements;
namespace Game.UI
{
    public sealed class UiToolkitPickupView : IPickupView, IDisposable
    {
        private readonly Label _feedback, _summary;
        private readonly Button _potion, _book;
        public event Action PotionRequested;
        public event Action BookRequested;
        public UiToolkitPickupView(VisualElement root)
        {
            _feedback = root.Q<Label>(GameplayUiElementIds.PickupFeedback);
            _summary = root.Q<Label>(GameplayUiElementIds.PickupObservation);
            _potion = root.Q<Button>(GameplayUiElementIds.DropPotion);
            _book = root.Q<Button>(GameplayUiElementIds.DropBook);
            _potion.clicked += Potion; _book.clicked += Book;
        }
        public void Render(PickupSnapshot snapshot, bool development)
        {
            _feedback.text = snapshot.Feedback ?? "";
            _feedback.style.display = string.IsNullOrEmpty(snapshot.Feedback) ? DisplayStyle.None : DisplayStyle.Flex;
            _summary.style.display = _potion.style.display = _book.style.display = development ? DisplayStyle.Flex : DisplayStyle.None;
            if (development) _summary.text = $"Pickups: {snapshot.Active} active\nDropped {snapshot.Spawned} · collected {snapshot.Collected}\nExpired {snapshot.Expired} · cancelled {snapshot.Cancelled} · placement rejected {snapshot.Rejected}";
        }
        private void Potion() => PotionRequested?.Invoke();
        private void Book() => BookRequested?.Invoke();
        public void Dispose() { _potion.clicked -= Potion; _book.clicked -= Book; }
    }
}
