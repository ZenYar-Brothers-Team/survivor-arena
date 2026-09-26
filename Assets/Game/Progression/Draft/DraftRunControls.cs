using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Progression
{
    public sealed class DraftRunControls
    {
        private readonly HashSet<ContentId> _banishedIds = new HashSet<ContentId>();

        public int InitialRerolls { get; }
        public int InitialBanishes { get; }
        public int RemainingRerolls { get; private set; }
        public int RemainingBanishes { get; private set; }
        public IReadOnlyCollection<ContentId> BanishedIds => _banishedIds;

        public DraftRunControls(int initialRerolls, int initialBanishes)
        {
            if (initialRerolls < 0)
                throw new ArgumentOutOfRangeException(nameof(initialRerolls));
            if (initialBanishes < 0)
                throw new ArgumentOutOfRangeException(nameof(initialBanishes));

            InitialRerolls = initialRerolls;
            InitialBanishes = initialBanishes;
            Reset();
        }

        public bool TryConsumeReroll()
        {
            if (RemainingRerolls <= 0)
                return false;

            RemainingRerolls--;
            return true;
        }

        /// <summary>Development command: adds rerolls for the current run only (not saved, reset restores the initial value).</summary>
        public void GrantRerolls(int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            RemainingRerolls = (int)Math.Min(int.MaxValue, (long)RemainingRerolls + count);
        }

        public bool TryBanish(ContentId id)
        {
            if (!id.IsValid || RemainingBanishes <= 0 || !_banishedIds.Add(id))
                return false;

            RemainingBanishes--;
            return true;
        }

        public bool IsBanished(ContentId id)
        {
            return _banishedIds.Contains(id);
        }

        public void Reset()
        {
            RemainingRerolls = InitialRerolls;
            RemainingBanishes = InitialBanishes;
            _banishedIds.Clear();
        }
    }
}
