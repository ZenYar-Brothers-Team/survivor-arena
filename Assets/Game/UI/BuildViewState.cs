using System;
using System.Collections.Generic;

namespace Game.UI
{
    public readonly struct BuildViewState
    {
        public IReadOnlyList<BuildSlotViewState> ActiveSlots { get; }
        public IReadOnlyList<BuildSlotViewState> PassiveSlots { get; }

        public BuildViewState(
            IReadOnlyList<BuildSlotViewState> activeSlots,
            IReadOnlyList<BuildSlotViewState> passiveSlots)
        {
            ActiveSlots = activeSlots ?? throw new ArgumentNullException(nameof(activeSlots));
            PassiveSlots = passiveSlots ?? throw new ArgumentNullException(nameof(passiveSlots));
        }
    }
}
