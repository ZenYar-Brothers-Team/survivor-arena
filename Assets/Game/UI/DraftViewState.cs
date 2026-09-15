using System;
using System.Collections.Generic;

namespace Game.UI
{
    public readonly struct DraftViewState
    {
        public bool IsVisible { get; }
        public int RemainingRerolls { get; }
        public int RemainingBanishes { get; }
        public IReadOnlyList<DraftOptionViewState> Options { get; }

        public DraftViewState(
            bool isVisible,
            int remainingRerolls,
            int remainingBanishes,
            IReadOnlyList<DraftOptionViewState> options)
        {
            IsVisible = isVisible;
            RemainingRerolls = remainingRerolls;
            RemainingBanishes = remainingBanishes;
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }
    }
}
