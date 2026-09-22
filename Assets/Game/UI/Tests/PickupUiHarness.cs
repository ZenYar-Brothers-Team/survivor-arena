using System;
using Game.Pickup;
namespace Game.UI.Tests
{
    public sealed class PickupUiHarness : IPickupRuntime, IPickupView
    {
        public event Action Changed;
        public event Action<PickupEvent> Resolved { add { } remove { } }
        public event Action PotionRequested;
        public event Action BookRequested;
        public PickupSnapshot Snapshot { get; set; }
        public PickupSnapshot Rendered;
        public bool Development;
        public int Drops;
        public void DropDevelopmentPickup(PickupRewardKind kind) => Drops++;
        public void Render(PickupSnapshot snapshot, bool development) { Rendered = snapshot; Development = development; }
        public void Change() => Changed?.Invoke();
        public void Potion() => PotionRequested?.Invoke();
        public void Book() => BookRequested?.Invoke();
    }
}
