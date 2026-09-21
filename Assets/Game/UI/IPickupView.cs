using System;
using Game.Pickup;
namespace Game.UI
{
    public interface IPickupView
    {
        event Action PotionRequested;
        event Action BookRequested;
        void Render(PickupSnapshot snapshot, bool development);
    }
}
