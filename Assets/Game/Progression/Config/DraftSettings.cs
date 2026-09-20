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
        public int? EmptyBookCurrency { get; }

        public DraftSettings(int offerCount, int seed, int initialRerolls, int initialBanishes, int? emptyBookCurrency = null)
        {
            NumericValidation.ValidateRange(offerCount, 1, 3, nameof(offerCount));
            NumericValidation.ValidateNonNegative(initialRerolls, nameof(initialRerolls), "Draft control counts cannot be negative.");
            NumericValidation.ValidateNonNegative(initialBanishes, nameof(initialBanishes), "Draft control counts cannot be negative.");
            if (emptyBookCurrency.HasValue) NumericValidation.ValidateCount(emptyBookCurrency.Value, nameof(emptyBookCurrency));
            EmptyBookCurrency = emptyBookCurrency;
            OfferCount = offerCount;
            Seed = seed;
            InitialRerolls = initialRerolls;
            InitialBanishes = initialBanishes;
        }
    }
}
