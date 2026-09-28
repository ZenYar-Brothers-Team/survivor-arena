using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    /// <summary>Read-only build inspection. One scroll owns every set section.</summary>
    internal sealed class PauseBuildPanel : IDisposable
    {
        private readonly VisualElement _root;
        private readonly VisualElement _slots;
        private readonly ScrollView _sets;
        private readonly VisualElement _popup;
        private readonly Label _title;
        private readonly Label _effect;
        private readonly Button _close;
        private VisualElement _trigger;
        private int _consumedFrame = -1;
        public bool IsPopupOpen { get; private set; }

        public PauseBuildPanel(VisualElement root)
        {
            _root = root;
            _slots = root.Q(GameplayUiElementIds.PauseSlots);
            _sets = root.Q<ScrollView>(GameplayUiElementIds.PauseBuild);
            _popup = new VisualElement { name = GameplayUiElementIds.SetPopup };
            _popup.AddToClassList("set-popup");
            _title = new Label(); _title.AddToClassList("recipe-title");
            _effect = new Label { name = GameplayUiElementIds.SetPopupEffect }; _effect.AddToClassList("recipe-effect");
            _close = new Button(() => { MarkShortcutConsumed(); Close(); }) { text = "×", name = GameplayUiElementIds.SetPopupClose };
            _close.AddToClassList("set-popup-close");
            _popup.Add(_title); _popup.Add(_effect); _popup.Add(_close);
            _root.Add(_popup); _popup.style.display = DisplayStyle.None;
            _root.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            _root.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            _root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            _sets.verticalScroller.valueChanged += OnScroll;
        }

        public void MarkShortcutConsumed() => _consumedFrame = Time.frameCount;

        public bool ConsumePauseShortcut(bool space)
        {
            if (_consumedFrame == Time.frameCount) return true;
            // Space activates the focused control; it must not also toggle the run.
            // Check focus before the popup: Space may have just opened it this frame.
            if (space && _root.panel?.focusController.focusedElement is VisualElement focus &&
                focus.enabledInHierarchy && IsDisplayed(focus) && (focus is Button || focus is Toggle || focus is TextField)) return true;
            if (IsPopupOpen) { Close(); MarkShortcutConsumed(); return true; }
            return false;
        }

        private static bool IsDisplayed(VisualElement element)
        {
            for (var current = element; current != null; current = current.parent)
                if (current.resolvedStyle.display == DisplayStyle.None || current.resolvedStyle.visibility == Visibility.Hidden) return false;
            return true;
        }

        public void Render(BuildViewState state)
        {
            Close(false);
            _slots.Clear(); _sets.Clear();
            AddSlots("Умения", state.ActiveSlots, "pause-build-grid-active");
            AddSlots("Предметы", state.PassiveSlots, "pause-build-grid-passive");
            var received = Group("Получены", GameplayUiElementIds.ReceivedSets, "set-group-received");
            foreach (var set in state.Sets) received.Add(SetButton(set.Title, set.Icon, set.Detail, "pause-set-chip"));
            HideEmptyGroup(received);
            var recipes = Group("Собираются", GameplayUiElementIds.PauseRecipes, "set-group-progress");
            foreach (var recipe in state.SetRecipeProgress)
            {
                if (recipe.IsAcquired || recipe.IsMissed) continue;
                var card = SetButton(recipe.Title, recipe.Icon, recipe.Effect, "pause-recipe-card");
                var progress = new Label($"{recipe.FulfilledComponents}/{recipe.RequiredComponents} · " +
                    (recipe.IsEligible ? "Рецепт готов" : recipe.HasProgress ? "В процессе" : "Не начат")) { pickingMode = PickingMode.Ignore };
                progress.AddToClassList("recipe-progress"); card.Add(progress);
                var components = new Label(recipe.Components) { pickingMode = PickingMode.Ignore };
                components.AddToClassList("recipe-components"); card.Add(components); recipes.Add(card);
            }
            HideEmptyGroup(recipes);
            var missed = Group("Упущены", GameplayUiElementIds.MissedSets, "set-group-missed");
            foreach (var recipe in state.SetRecipeProgress)
                if (recipe.IsMissed) missed.Add(SetButton(recipe.Title, recipe.Icon, recipe.Effect, "pause-set-chip"));
            HideEmptyGroup(missed);
        }

        private void AddSlots(string title, IReadOnlyList<BuildSlotViewState> slots, string className)
        {
            var heading = new Label(title); heading.AddToClassList("pause-section-title"); _slots.Add(heading);
            var grid = new VisualElement(); grid.AddToClassList("pause-build-grid"); grid.AddToClassList(className); _slots.Add(grid);
            foreach (var slot in slots)
            {
                var row = new VisualElement { tooltip = slot.IsOccupied ? slot.Detail : "" }; row.AddToClassList("pause-slot"); row.EnableInClassList("pause-slot-empty", !slot.IsOccupied);
                var icon = new Image { sprite = slot.Icon, pickingMode = PickingMode.Ignore }; icon.AddToClassList("pause-slot-icon");
                var label = new Label(slot.IsOccupied ? slot.Title : "—") { name = GameplayUiElementIds.CardTitle }; label.AddToClassList("pause-slot-name");
                row.Add(icon); row.Add(label); row.Add(new Label(slot.IsOccupied ? slot.Level.ToString() : "")); grid.Add(row);
            }
        }

        private VisualElement Group(string title, string id, string className)
        {
            var section = new VisualElement(); section.AddToClassList(className); _sets.Add(section);
            var heading = new Label(title); heading.AddToClassList("pause-section-title"); section.Add(heading);
            var contents = new VisualElement { name = id }; contents.AddToClassList("set-group-items"); section.Add(contents); return contents;
        }

        private static void HideEmptyGroup(VisualElement group)
        {
            if (group.childCount == 0) group.parent.style.display = DisplayStyle.None;
        }

        private Button SetButton(string title, Sprite sprite, string effect, string className)
        {
            var button = new Button(); button.AddToClassList(className);
            var header = new VisualElement(); header.AddToClassList("set-name-row");
            var icon = new Image { sprite = sprite, pickingMode = PickingMode.Ignore }; icon.AddToClassList("set-icon");
            var label = new Label(title) { name = GameplayUiElementIds.CardTitle, pickingMode = PickingMode.Ignore };
            label.AddToClassList("set-name"); header.Add(icon); header.Add(label); button.Add(header);
            button.clicked += () => Open(button, title, effect); return button;
        }

        private void Open(VisualElement trigger, string title, string effect)
        {
            _trigger = trigger; _title.text = title; _effect.text = effect;
            IsPopupOpen = true; _popup.style.display = DisplayStyle.Flex; _popup.BringToFront();
            // Root-local overlay above the footer; no scroll geometry is changed.
            var width = Mathf.Min(360, _root.layout.width - 32);
            _popup.style.width = width;
            var local = _root.WorldToLocal(trigger.worldBound.position);
            var footer = _root.Q(GameplayUiElementIds.PauseFooter);
            var bottom = _root.WorldToLocal(footer.worldBound.position).y - 12;
            _popup.style.left = Mathf.Clamp(local.x, 16, Mathf.Max(16, _root.layout.width - width - 16));
            _popup.style.top = Mathf.Clamp(local.y, 16, Mathf.Max(16, bottom - 170)); _close.Focus();
        }

        public void Close(bool restoreFocus = true)
        {
            if (!IsPopupOpen) return;
            IsPopupOpen = false; _popup.style.display = DisplayStyle.None;
            if (restoreFocus && _trigger?.panel != null) _trigger.Focus(); _trigger = null;
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!IsPopupOpen) return;
            if (evt.button == 1 || evt.target is VisualElement target && !_popup.Contains(target))
            { Close(); MarkShortcutConsumed(); evt.StopImmediatePropagation(); }
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (!IsPopupOpen || evt.keyCode != KeyCode.Escape) return;
            Close(); MarkShortcutConsumed(); evt.StopImmediatePropagation();
        }

        private void OnScroll(float _) => Close();
        private void OnGeometryChanged(GeometryChangedEvent _) => Close();
        public void Dispose()
        {
            Close(false);
            _root.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            _root.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            _root.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            _sets.verticalScroller.valueChanged -= OnScroll; _popup.RemoveFromHierarchy();
        }
    }
}
