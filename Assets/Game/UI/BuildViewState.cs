using System;
using System.Collections.Generic;

namespace Game.UI
{
    public readonly struct BuildViewState
    {
        public IReadOnlyList<BuildSlotViewState> ActiveSlots { get; }
        public IReadOnlyList<BuildSlotViewState> PassiveSlots { get; }
        public IReadOnlyList<SetBuildViewState> Sets { get; }
        public IReadOnlyList<SetRecipeProgressViewState> SetRecipeProgress { get; }

        public BuildViewState(
            IReadOnlyList<BuildSlotViewState> activeSlots,
            IReadOnlyList<BuildSlotViewState> passiveSlots,
            IReadOnlyList<SetBuildViewState> sets,
            IReadOnlyList<SetRecipeProgressViewState> setRecipeProgress)
        {
            ActiveSlots = activeSlots ?? throw new ArgumentNullException(nameof(activeSlots));
            PassiveSlots = passiveSlots ?? throw new ArgumentNullException(nameof(passiveSlots));
            Sets = sets ?? throw new ArgumentNullException(nameof(sets));
            SetRecipeProgress = setRecipeProgress ?? throw new ArgumentNullException(nameof(setRecipeProgress));
        }
    }
}
