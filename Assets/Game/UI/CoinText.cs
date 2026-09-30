namespace Game.UI
{
    /// <summary>Meta currency is called «монеты» everywhere in the UI (DECISION-0107); Russian plural agreement.</summary>
    public static class CoinText
    {
        public static string Amount(long amount) => amount.ToString("N0") + " " + Noun(amount);

        public static string Noun(long amount)
        {
            var value = amount < 0 ? -amount : amount;
            var lastTwo = value % 100;
            if (lastTwo >= 11 && lastTwo <= 14) return "монет";
            switch (value % 10)
            {
                case 1: return "монета";
                case 2: case 3: case 4: return "монеты";
                default: return "монет";
            }
        }
    }
}
