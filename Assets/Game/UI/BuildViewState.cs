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
            ActiveSlots = new List<BuildSlotViewState>(activeSlots ?? throw new ArgumentNullException(nameof(activeSlots))).AsReadOnly();
            PassiveSlots = new List<BuildSlotViewState>(passiveSlots ?? throw new ArgumentNullException(nameof(passiveSlots))).AsReadOnly();
            Sets = new List<SetBuildViewState>(sets ?? throw new ArgumentNullException(nameof(sets))).AsReadOnly();
            SetRecipeProgress = new List<SetRecipeProgressViewState>(setRecipeProgress ?? throw new ArgumentNullException(nameof(setRecipeProgress))).AsReadOnly();
        }
    }
}
