using Game.Content;

namespace Game.Progression
{
    // Level-up draft parameters for a run (offer size, RNG seed, reroll/banish budget).
    // Content, not code: loaded from Resources/Content/Run/*.json.
    public sealed class DraftSettings
    {
        public int OfferCount { get; }
        public int Seed { get; }
        public int InitialRerolls { get; }
        public int InitialBanishes { get; }

        public DraftSettings(int offerCount, int seed, int initialRerolls, int initialBanishes)
        {
            NumericValidation.ValidateCount(offerCount, nameof(offerCount), "Draft offer count must be greater than zero.");
            NumericValidation.ValidateNonNegative(initialRerolls, nameof(initialRerolls), "Draft control counts cannot be negative.");
            NumericValidation.ValidateNonNegative(initialBanishes, nameof(initialBanishes), "Draft control counts cannot be negative.");
            OfferCount = offerCount;
            Seed = seed;
            InitialRerolls = initialRerolls;
            InitialBanishes = initialBanishes;
        }
    }
}
