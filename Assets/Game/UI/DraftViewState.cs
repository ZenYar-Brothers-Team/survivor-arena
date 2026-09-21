using System;
using System.Collections.Generic;

namespace Game.UI
{
    public readonly struct DraftViewState
    {
        public bool IsVisible { get; }
        public bool IsBanishMode { get; }
        public bool CanReroll => IsVisible && !IsBanishMode && RemainingRerolls > 0;
        public bool CanBanish => IsVisible && RemainingBanishes > 0;
        public string ControlHint => IsBanishMode ? "Choose a card to banish, or cancel." :
            RemainingRerolls == 0 && RemainingBanishes == 0 ? "No rerolls or banishes remaining." :
            RemainingRerolls == 0 ? "No rerolls remaining." :
            RemainingBanishes == 0 ? "No banishes remaining." : "Choose a card to acquire or upgrade.";
        public Guid Revision { get; }
        public string Heading { get; }
        public string QueueDetail { get; }
        public int RemainingRerolls { get; }
        public int RemainingBanishes { get; }
        public IReadOnlyList<DraftOptionViewState> Options { get; }

        public DraftViewState(
            bool isVisible,
            int remainingRerolls,
            int remainingBanishes,
            IReadOnlyList<DraftOptionViewState> options, Guid revision = default,
            string heading = "LEVEL UP", string queueDetail = "", bool isBanishMode = false)
        {
            IsVisible = isVisible;
            IsBanishMode = isBanishMode;
            Revision = revision;
            Heading = heading;
            QueueDetail = queueDetail;
            RemainingRerolls = remainingRerolls;
            RemainingBanishes = remainingBanishes;
            Options = new List<DraftOptionViewState>(options ?? throw new ArgumentNullException(nameof(options))).AsReadOnly();
        }
    }
}
