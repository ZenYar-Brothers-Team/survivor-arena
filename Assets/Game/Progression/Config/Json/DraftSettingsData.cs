namespace Game.Progression.Json
{
    public sealed class DraftSettingsData
    {
        public float? SetDraftChance { get; set; }
        public int? OfferCount { get; set; }
        public int? Seed { get; set; }
        public int? InitialRerolls { get; set; }
        public int? InitialBanishes { get; set; }
        public int? EmptyBookCurrency { get; set; }
    }
}
