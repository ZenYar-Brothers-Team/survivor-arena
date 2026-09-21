using System;
using Game.Content;
namespace Game.Meta
{
    public sealed class MetaUpgrade
    {
        public string Id { get; }
        public string Name { get; }
        public bool Personal { get; }
        public string Stat { get; }
        public int Cap { get; }
        public int PriceCoefficient { get; }
        public float Bonus { get; }
        public MetaUpgrade(MetaUpgradeData data)
        {
            Id = new ContentId(data.Id).ToString();
            Name = !string.IsNullOrWhiteSpace(data.Name) ? data.Name : throw new ArgumentException("Upgrade name required.");
            Personal = data.Personal ?? throw new ArgumentException("personal required.");
            Stat = data.Stat;
            if (Stat != "health" && Stat != "damage") throw new ArgumentException("Unknown meta stat.");
            Cap = data.Cap ?? throw new ArgumentException("cap required.");
            PriceCoefficient = data.PriceCoefficient ?? throw new ArgumentException("priceCoefficient required.");
            Bonus = data.Bonus ?? throw new ArgumentException("bonus required.");
            NumericValidation.ValidateCount(Cap, nameof(Cap));
            NumericValidation.ValidateCount(PriceCoefficient, nameof(PriceCoefficient));
            NumericValidation.ValidateNonNegativeFinite(Bonus, nameof(Bonus));
            if (Cap > 5 || Bonus > (Stat == "health" ? .05f : .03f)) throw new ArgumentException("Meta upgrade outside approved tuning range.");
        }
        public long Price(int currentLevel) => checked((long)PriceCoefficient * (currentLevel + 1));
        public string Key(string character) => Personal ? Id + ":" + new ContentId(character) : Id;
    }
}
