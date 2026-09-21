using UnityEngine;
namespace Game.Pickup
{
    public interface IPickupPlacement
    {
        bool TryPlace(Vector2 requested, out Vector2 reachable);
    }
}
