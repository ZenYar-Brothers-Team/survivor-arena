using System;
using System.Collections.Generic;
using System.Globalization;
using Game.ActiveSkill;
using Game.Progression;

namespace Game.UI
{
    /// <summary>Presentation copy only. All upgrade numbers come from definition previews.</summary>
    public static class GameplayUiCopy
    {
        public static string SetEffect(SetDefinition set) => set?.Id.ToString() switch
        {
            "SET-001" => "Камни бьют сильнее, становятся крупнее и дальше отбрасывают врагов.",
            "SET-002" => "Бумеранг и диск быстрее и сильнее на обратном пути.",
            "SET-003" => "Молния достигает большего числа целей, прыгает дальше и слабее теряет силу.",
            "SET-004" => "Замедленных врагов проще отбросить. Импульсная волна шире и сильнее отталкивает.",
            "SET-005" => "Больше опыта и радиус его подбора. Возвращает исчезнувший опыт и лечит при повышении уровня.",
            "SET-006" => "Больше здоровья, регенерации и лечения. Зелья выпадают чаще.",
            "SET-007" => "Веер игл, Спираль осколков и Крест клинков бьют сильнее, шире и дальше.",
            "SET-008" => "Иногда выпускает тяжёлый мусор, взрывающийся при остановке. Взрывы компонентов шире.",
            "SET-009" => "Копьё и луч бьют сильнее, шире и дальше. Копьё пробивает больше врагов.",
            "SET-010" => "Орбита замедляет врагов. Клинки сильнее бьют по замедленным целям.",
            "SET-011" => "Камни, копьё и диск летят быстрее и дальше, бьют сильнее и лучше отбрасывают.",
            "SET-012" => "Больше здоровья, защиты и устойчивости. Импульсная волна шире и сильнее отталкивает.",
            "SET-013" => "Периодический электрический удар по ближайшему врагу расходится разрядами к соседним.",
            "SET-014" => "Орбитальные клинки, Спираль осколков и Крест клинков становятся крупнее, быстрее и сильнее.",
            "SET-015" => "Взрывы компонентов сильнее и шире. Подбор зелья вызывает мощный взрыв с перезарядкой.",
            "SET-016" => "Периодически выпускает огромный болт по направлению движения: пробивает и сильно отбрасывает.",
            "SET-017" => "Периодически обрушивает мощный удар большой площади после короткого предупреждения.",
            "SET-018" => "Периодически выпускает огромную сферу в случайном направлении. Она заканчивает путь мощным взрывом.",
            "SET-019" => "Периодически выпускает ледяное копьё по направлению движения: пробивает, замедляет и отбрасывает.",
            "SET-020" => "Периодически запускает тяжёлый валун в случайном направлении: пробивает и сильно отбрасывает.",
            "SET-021" => "Периодически бросает небольшой камешек по направлению движения: пробивает пару врагов.",
            "SET-022" => "Периодически бьёт коротким конусом по ближайшему врагу: сильно отбрасывает всех в конусе.",
            "SET-023" => "Немного больше здоровья и скорости движения, чуть меньше получаемого урона.",
            "SET-024" => "Активные умения немного сильнее и быстрее.",
            "SET-025" => "Больше радиус подбора опыта и немного регенерации здоровья.",
            "SET-026" => "Орбитальные клинки и Импульсная волна немного сильнее и крупнее.",
            "SET-027" => "Небесный удар и Веер игл немного сильнее и крупнее.",
            "SET-028" => "Ледяные осколки и Цепная молния сильнее. Осколки сильнее замедляют, молния прыгает дальше.",
            "SET-029" => "Рикошетный диск и Бросок камня сильнее, летят быстрее и лучше отбрасывают.",
            "SET-030" => "Бумеранг и Ветряное копьё бьют дальше и немного сильнее.",
            "SET-031" => "Бросок камня сильнее и летит быстрее.",
            "SET-032" => "Взрывы Взрывных сфер сильнее и шире.",
            "SET-033" => "Орбитальные клинки вращаются быстрее.",
            "SET-034" => "Ледяные осколки выпускаются в большем числе.",
            "SET-035" => "Иглы Веера игл пробивают на одну цель больше.",
            _ => set?.Description ?? string.Empty
        };

        public static string DraftEffect(BuildEntryDefinition definition, DraftOptionPreview preview)
        {
            if (definition is SetDefinition set) return SetEffect(set);
            var lines = new List<string>();
            var active = definition is ActiveSkillProgressionDefinition;
            if (active && preview.CurrentLevel == 0) lines.Add(SkillEffect(definition.Id.ToString()));
            foreach (var value in preview.Values)
            {
                if (active && IsUnchangedTravelRange(value, preview.Values)) continue;
                var line = Describe(value, preview.CurrentLevel == 0, active);
                if (!string.IsNullOrEmpty(line) && !lines.Contains(line)) lines.Add(line);
            }
            return string.Join("\n", lines);
        }

        public static string Describe(DraftValueChange value, bool isNew, bool active)
        {
            if (!isNew && value.Current == value.Next) return "";
            var key = value.Label;
            var separator = key.LastIndexOf(": ", StringComparison.Ordinal);
            if (separator >= 0) key = key.Substring(separator + 2);
            if (key == "Base damage") return !isNew && value.Current > 0 ? "Урон " + Percent(value.Current, value.Next) : "";
            if (key == "damage retention") return "Повторные удары слабее теряют силу";
            if (key == "ricochets") return $"Рикошеты: +{Number(value.Next - value.Current)}. Повторный удар слабее";
            if (key == "pierce" && value.Next < 0) return "Сквозное пробивание без лимита целей";
            if (key == "return damage") return "Урон на возврате " + SignedPercent((value.Next - 1) * 100) + "%";
            // User 2026-09-30: never show cooldown seconds; only the relative use-speed change on upgrades.
            if (key == "Base cooldown") return isNew ? "" : "Скорость использования " + Percent(value.Next, value.Current);
            if (key == "Action speed bonus") return "Скорость использования " + (isNew ? SignedPercent(value.Next * 100) + "%" : Percent(1 + value.Current, 1 + value.Next));
            if (key == "Waves" && isNew) return "";
            var label = key switch
            {
                "Max HP" => "Здоровье", "Move speed" => "Скорость", "Damage" => "Урон",
                "Action speed" => "Темп", "Action speed bonus" => "Темп", "Damage reduction" => "Защита",
                "Healing" => "Лечение", "Regeneration" => "Регенерация", "Expired XP recovery" => "Возврат опыта",
                "Picked-up XP" => "Опыт", "XP lifetime" => "Время жизни опыта", "Knockback resistance" => "Устойчивость",
                "Outgoing knockback" => "Отбрасывание", "Pickup radius" => "Радиус подбора", "Effect size" => "Размер",
                "Effect range" => "Дальность", "Potion drops" => "Шанс зелья", "Max low-HP damage" => "Урон при низком HP: до",
                "Targeting radius" => "Радиус поиска", "Waves" => "Волны", "delay" => "Задержка", "knockback" => "Отбрасывание",
                "slow" => "Замедление", "projectiles" => "Снаряды", "speed" => "Скорость полёта", "lifetime" => "Время действия",
                "projectile radius" => "Размер снаряда", "pierce" => "Пробивание", "explosion radius" => "Радиус взрыва",
                "time to stop" => "Время полёта", "range" => "Дальность", "width" => "Ширина", "duration" => "Длительность",
                "blades" => "Клинки", "orbit radius" => "Радиус орбиты", "blade radius" => "Размер клинка",
                "rotation speed" => "Скорость вращения", "targets" => "Цели", "jump range" => "Дальность прыжка",
                "radius" => "Радиус", "telegraph" => "Задержка удара", "blast radius" => "Радиус взрыва", "mine limit" => "Лимит мин",
                _ => ""
            };
            if (label.Length == 0) return "";
            var prefix = value.Label.StartsWith("Wave ", StringComparison.Ordinal) && !value.Label.StartsWith("Wave 1:", StringComparison.Ordinal)
                ? "Повтор: " : "";
            if (key == "slow")
                return prefix + label + " " + (isNew ? SignedPercent(value.Next * 100) : SignedPercent(value.Current * 100) + "% → " + SignedPercent(value.Next * 100)) + "%";
            if (!active)
            {
                var unit = value.Unit.Replace(" HP/s", "/с").Replace(" s", " с");
                if (unit == "%")
                    return label + " " + (isNew ? SignedPercent(value.Next) : SignedPercent(value.Current) + "% → " + SignedPercent(value.Next)) + "%";
                return label + " " + (isNew ? Signed(value.Next) : Signed(value.Current) + unit + " → " + Signed(value.Next)) + unit;
            }
            if (key == "projectiles" || key == "blades" || key == "targets" || key == "pierce" || key == "mine limit" || key == "Waves")
                return prefix + label + ": " + (isNew ? Number(value.Next) : Number(value.Current) + " → " + Number(value.Next));
            if (value.Current > 0 && !isNew) return prefix + label + " " + Percent(value.Current, value.Next);
            var seconds = key == "duration" || key == "delay" || key == "telegraph" || key == "lifetime" || key == "time to stop";
            return prefix + label + " " + Number(value.Next) + (seconds ? " с" : "");
        }

        private static bool IsUnchangedTravelRange(DraftValueChange value, IReadOnlyList<DraftValueChange> values)
        {
            const string suffix = "lifetime";
            if (!value.Label.EndsWith(suffix, StringComparison.Ordinal) || value.Current <= 0 || value.Next <= 0) return false;
            var speedLabel = value.Label.Substring(0, value.Label.Length - suffix.Length) + "speed";
            foreach (var speed in values)
                if (speed.Label == speedLabel && speed.Current > 0 && speed.Next > 0)
                {
                    var before = value.Current * speed.Current;
                    var after = value.Next * speed.Next;
                    return Math.Abs(after - before) <= before * .0001f;
                }
            return false;
        }

        private static string Percent(float before, float after) => SignedPercent((after / before - 1) * 100) + "%";
        public static string SignedPercent(float value)
        {
            var rounded = Math.Round(value, 0, MidpointRounding.AwayFromZero);
            return (rounded >= 0 ? "+" : "") + rounded.ToString("0", CultureInfo.InvariantCulture);
        }
        private static string Number(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);
        private static string Signed(float value) => (value >= 0 ? "+" : "") + Number(value);
        private static string SkillEffect(string id) => id switch
        {
            "SKILL-001" => "Бросает камни в ближайших врагов.",
            "SKILL-002" => "Выпускает веер игл в сторону ближайшего врага.",
            "SKILL-003" => "Клинки вращаются вокруг персонажа и режут врагов.",
            "SKILL-004" => "Круговая волна отбрасывает врагов.",
            "SKILL-005" => "Выпускает копьё по направлению движения.",
            "SKILL-006" => "Бумеранг летит к врагу и возвращается к персонажу.",
            "SKILL-007" => "Молния перескакивает между врагами, слабея с каждым ударом.",
            "SKILL-008" => "Диск рикошетит между ближайшими врагами.",
            "SKILL-009" => "Оставляет мину. Она взрывается при приближении врага или со временем.",
            "SKILL-010" => "Обрушивает удар по отмеченной области.",
            "SKILL-011" => "Выпускает осколки по кругу. Следующий залп немного повёрнут.",
            "SKILL-012" => "Короткий луч поражает врагов на линии к ближайшей цели.",
            "SKILL-013" => "Веер ледяных осколков замедляет врагов.",
            "SKILL-014" => "Сферы летят в случайных направлениях и взрываются при попадании или со временем.",
            "SKILL-015" => "Выпускает режущие волны крестом вокруг персонажа.",
            "SKILL-016" => "Часто разбрасывает замедляющиеся снаряды в случайных направлениях.",
            _ => "Автоматическая атака."
        };
    }
}
