using Game.Pickup;
namespace Game.Traveler.Tests
{
    public sealed class TravelerTestRewardTarget : IPickupRewardTarget
    {
        public bool CanCollect(PickupIdentity identity) => false;
        public PickupRewardResult TryApply(PickupDefinition definition, PickupIdentity identity) => default;
    }
}
