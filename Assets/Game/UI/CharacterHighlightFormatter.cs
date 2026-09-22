using System.Globalization;
using Game.Character;
namespace Game.UI
{
    public static class CharacterHighlightFormatter
    {
        // DECISION-0026: presentation derives every number from actual stats and explicit baseline.
        public static string Format(CharacterBaseStats stats, CharacterBaseStats baseline, CharacterStatField field)
        {
            var value = CharacterStatValues.Get(stats, field);
            var reference = CharacterStatValues.Get(baseline, field);
            var fractional = field == CharacterStatField.DisappearingXpRecovery || field == CharacterStatField.KnockbackResistance;
            var delta = fractional ? (value - reference) * 100f : reference != 0f ? (value / reference - 1f) * 100f : value;
            var unit = fractional ? " pp" : reference != 0f ? "%" : string.Empty;
            var label = System.Text.RegularExpressions.Regex.Replace(field.ToString(), "([a-z])([A-Z])", "$1 $2");
            label = label.Replace("Multiplier", "").Trim();
            return label + ": " + delta.ToString("+0.##;-0.##;0", CultureInfo.InvariantCulture) + unit;
        }
    }
}
