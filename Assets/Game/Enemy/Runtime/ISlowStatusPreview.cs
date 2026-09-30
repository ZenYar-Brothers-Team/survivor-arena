using System;
using Game.Presentation;

namespace Game.Enemy
{
    /// <summary>Development-panel access to the slow-status look comparison (DECISION-0108).</summary>
    public interface ISlowStatusPreview
    {
        SlowStatusStyle Style { get; }
        int ShownCount { get; }
        event Action Changed;
        void SetStyle(SlowStatusStyle style);
        int SlowAllForPreview();
    }
}
