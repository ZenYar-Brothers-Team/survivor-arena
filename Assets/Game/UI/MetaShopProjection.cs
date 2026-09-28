using System.Globalization;
using Game.Meta;
namespace Game.UI
{
    public static class MetaShopProjection
    {
        public static string Bonus(MetaUpgrade upgrade, int level)
        {
            var amount = upgrade.Bonus * level;
            if (upgrade.Stat == "regeneration") return "+" + amount.ToString("0.##", CultureInfo.GetCultureInfo("ru-RU")) + " HP/с";
            if (upgrade.Stat == "rerolls" || upgrade.Stat == "banishes") return "+" + amount.ToString("0") + " на забег";
            return "+" + (amount * 100).ToString("0") + (upgrade.Stat == "damageReduction" ? " п.п." : "%");
        }
        public static string Reason(string value) => value switch
        {
            "Save the profile first" => "Дождитесь сохранения",
            "Available between runs" => "Доступно между забегами",
            "Choose an unlocked character" => "Выберите открытого героя",
            "Maximum level" => "Максимум",
            "Not enough currency" => "Не хватает золота",
            "Already unlocked" => "Открыто",
            "Unlocked by achievement" => "За достижение",
            _ => value
        };
        public static string Condition(MetaUnlock rule, MetaCatalog catalog)
        {
            var field = rule.RequiredId != null && catalog.Unlocks.TryGetValue(rule.RequiredId, out var parent) ? parent.Name : "";
            return rule.Condition switch
            {
                "initial" => "Доступно с начала",
                "fieldClear" => "Выжить 15:00 · " + field,
                "access" => "Открыть карту · " + field,
                "firstRun" => "Завершить первый забег",
                _ => rule.Description
            };
        }
    }
}
