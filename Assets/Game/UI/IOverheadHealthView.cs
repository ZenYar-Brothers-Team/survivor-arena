using System.Collections.Generic;

namespace Game.UI
{
    public interface IOverheadHealthView
    {
        void Render(IReadOnlyList<OverheadHealthBarItem> bars);
    }
}
