namespace Game.Pickup
{
    public interface IPickupRewardTarget
    {
        bool CanCollect(PickupIdentity identity);
        PickupRewardResult TryApply(PickupDefinition definition, PickupIdentity identity);
    }
}
