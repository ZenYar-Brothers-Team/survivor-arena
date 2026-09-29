using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    internal static class EntryUi
    {
        public static void Configure(VisualElement root, PanelSettings panel)
        {
            panel.scaleMode = PanelScaleMode.ConstantPixelSize;
            root.styleSheets.Add(Resources.Load<StyleSheet>("UI/FolioChromeStyles"));
            root.styleSheets.Add(Resources.Load<StyleSheet>("UI/EntryStyles"));
            root.AddToClassList("entry-root");
            root.RegisterCallback<GeometryChangedEvent>(e => root.EnableInClassList("entry-compact", e.newRect.width < 1500));
        }

        public static Label Label(string text, string style, string name = null)
        {
            var label = new Label(text) { name = name, pickingMode = PickingMode.Ignore };
            label.AddToClassList(style);
            return label;
        }

        public static VisualElement Box(string style)
        {
            var box = new VisualElement(); box.AddToClassList(style); return box;
        }

        public static Image Image(Sprite sprite, string style, string name = null)
        {
            var image = new Image { sprite = sprite, name = name, scaleMode = ScaleMode.ScaleToFit, pickingMode = PickingMode.Ignore };
            image.AddToClassList(style); return image;
        }

        public static void Choice(Button button, ContentCardViewState state)
        {
            button.EnableInClassList("entry-selected", state.IsSelected);
            button.EnableInClassList("entry-locked", state.IsLocked);
            button.SetEnabled(state.IsEnabled);
        }

        public static string Readable(string text) => (text ?? "")
            .Replace("Active Skill Cooldown", "Перезарядка умений").Replace("Incoming Damage", "Входящий урон")
            .Replace("Health Restoration", "Восстановление здоровья").Replace("Health Regeneration Per Second", "Регенерация в секунду")
            .Replace("Disappearing Xp Recovery", "Возврат исчезающего опыта").Replace("Picked Up Xp", "Получаемый опыт")
            .Replace("Xp Drop Lifetime Bonus Seconds", "Время жизни опыта, с").Replace("Knockback Resistance", "Сопротивление отбрасыванию")
            .Replace("Outgoing Knockback Bonus", "Отбрасывание").Replace("Potion Drop", "Выпадение зелий")
            .Replace("Low Health Damage Max Bonus", "Урон при низком здоровье")
            .Replace("Survive 15:00 on ", "Выживи 15:00 на ").Replace(" currency", " монет")
            .Replace("Finish your first run (including Quit)", "Заверши первый забег (в том числе выходом)")
            .Replace("Available from the start", "Доступно с начала").Replace("Unavailable content", "Недоступно").Replace("Unlock ", "Открой ")
            .Replace("Movement Speed", "Скорость движения").Replace("Max Health", "Здоровье")
            .Replace("Active Skill Damage", "Урон умений").Replace("Action Speed", "Скорость использования")
            .Replace("Effect Size", "Размер эффекта").Replace("Effect Range", "Дальность")
            .Replace("Experience Gain", "Опыт").Replace("Pickup Radius", "Радиус подбора")
            .Replace("Health Regeneration", "Регенерация").Replace("damage", "урон")
            .Replace("action speed", "скорость использования").Replace("size", "размер").Replace("range", "дальность")
            .Replace("Permanent bonuses: disabled in Meta progression", "Постоянные улучшения отключены")
            .Replace("Permanent bonuses: HP", "Постоянные улучшения: здоровье");
    }
}
