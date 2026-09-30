using System.Globalization;
using Game.Meta;
using Game.Content;
using Game.Presentation;
using UnityEngine;
namespace Game.UI
{
    public static class MetaShopProjection
    {
        public static Sprite UpgradeIcon(string ownerId, ContentRegistry registry)
        {
            if (registry == null || !registry.TryGet<SpriteDefinition>(new ContentId(ownerId + "-VISUAL-ICON"), out var icon)) return null;
            icon.RequireRole(SpriteRole.Icon);
            return icon.Sprite;
        }
        public static int UpgradeOrder(string stat) => stat switch
        {
            "damage" => 0, "actionSpeed" => 1, "size" => 2,
            "health" => 3, "damageReduction" => 4, "regeneration" => 5, "healing" => 6,
            "movement" => 7, "pickupRadius" => 8, "experience" => 9,
            "rerolls" => 10, "banishes" => 11, _ => 12
        };
        public static string Kind(string kind) => kind switch
        { "character" => "Персонаж", "field" => "Карта", "skill" => "Активное", "passive" => "Пассивное", "set" => "Сет", _ => "" };
        public static bool MatchesUnlock(MetaCardViewState card, string kind, int state) =>
            (kind == "all" || card.Kind == kind || kind == "ability" && (card.Kind == "skill" || card.Kind == "passive")) &&
            (state == 0 || state == 1 && !card.Owned || state == 2 && card.CanBuy || state == 3 && card.Owned);
        public static string Bonus(MetaUpgrade upgrade, int level)
        {
            var amount = upgrade.Bonus * level;
            if (upgrade.Stat == "regeneration") return "+" + amount.ToString("0.##", CultureInfo.GetCultureInfo("ru-RU")) + " HP/с";
            if (upgrade.Stat == "rerolls" || upgrade.Stat == "banishes") return "+" + amount.ToString("0") + " на забег";
            // DECISION-0093: bonuses such as +4.5% keep one decimal instead of rounding to +5%.
            return "+" + (amount * 100).ToString("0.#", CultureInfo.GetCultureInfo("ru-RU")) + (upgrade.Stat == "damageReduction" ? " п.п." : "%");
        }
        public static string Reason(string value) => value switch
        {
            "Save the profile first" => "Дождитесь сохранения",
            "Available between runs" => "Доступно между забегами",
            "Choose an unlocked character" => "Выберите открытого героя",
            "Maximum level" => "Максимум",
            "Not enough currency" => "Не хватает монет",
            "Already unlocked" => "Открыто",
            "Unlocked by achievement" => "За достижение",
            _ => value
        };
        public static string Condition(MetaUnlock rule, MetaCatalog catalog, long progress = 0)
        {
            var field = rule.RequiredId != null && catalog.Unlocks.TryGetValue(rule.RequiredId, out var parent) ? parent.Name : "";
            var target = rule.TargetId != null && catalog.Unlocks.TryGetValue(rule.TargetId, out var targetRule) ? targetRule.Name : rule.TargetId;
            var count = System.Math.Min(progress, rule.TargetCount);
            var achievement = rule.Metric switch
            {
                "ordinaryKills" => "Убить врагов",
                "earnedGold" => "Заработать монет",
                "activeDamage" => "Нанести урон умениями",
                "characterDamage" => "Нанести урон за " + target,
                "killsById" => "Победить " + target,
                "skillDamage" => "Нанести урон · " + target,
                _ => "Достижение"
            };
            return rule.Condition switch
            {
                "initial" => "Доступно с начала",
                "fieldClear" => "Выжить 15:00 · " + field,
                "access" => "Открыть карту · " + field,
                "firstRun" => "Завершить первый забег",
                "achievement" => achievement + " на " + field + " · " + count + "/" + rule.TargetCount,
                "fieldClearOrAchievement" => "Выжить 15:00 на " + field + " или " + achievement.ToLowerInvariant() +
                    " · " + count + "/" + rule.TargetCount,
                _ => rule.Description
            };
        }
    }
}
