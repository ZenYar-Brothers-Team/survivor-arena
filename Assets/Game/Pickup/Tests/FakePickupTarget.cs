using System;
namespace Game.Pickup.Tests
{
    public sealed class FakePickupTarget : IPickupRewardTarget
    {
        public bool Running = true, Accept = true;
        public int Attempts;
        public Action Applying;
        public bool CanCollect(PickupIdentity identity) => Running;
        public PickupRewardResult TryApply(PickupDefinition definition, PickupIdentity identity)
        { Attempts++; Applying?.Invoke(); return new PickupRewardResult(Accept); }
    }
}
