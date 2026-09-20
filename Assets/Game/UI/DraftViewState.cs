using System;
using System.Collections.Generic;

namespace Game.UI
{
    public readonly struct DraftViewState
    {
        public bool IsVisible { get; }
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
            string heading = "LEVEL UP", string queueDetail = "")
        {
            IsVisible = isVisible;
            Revision = revision;
            Heading = heading;
            QueueDetail = queueDetail;
            RemainingRerolls = remainingRerolls;
            RemainingBanishes = remainingBanishes;
            Options = new List<DraftOptionViewState>(options ?? throw new ArgumentNullException(nameof(options))).AsReadOnly();
        }
    }
}
