using System;
using Game.Presentation;

namespace Game.UI
{
    public interface ISlowStatusView
    {
        event Action<SlowStatusStyle> StyleRequested;
        event Action SlowAllRequested;
        void Render(SlowStatusStyle style, bool development, string summary);
    }
}
