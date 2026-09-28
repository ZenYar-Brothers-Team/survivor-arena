using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    internal sealed class RunResultsPanel
    {
        public VisualElement Root { get; }
        public RunResultsPanel()
        { Root = Resources.Load<VisualTreeAsset>("UI/RunResults").CloneTree(); Root.AddToClassList("results-content"); }
        public void Render(RunResultsViewState state)
        {
            Root.EnableInClassList("results-hidden", state == null);
            if (state == null) return;
            Text(GameplayUiElementIds.ResultsOutcome, state.Outcome);
            Text(GameplayUiElementIds.ResultsSelection, state.Selection);
            Text(GameplayUiElementIds.ResultsTime, state.Time);
            Text(GameplayUiElementIds.ResultsLevel, state.Level?.ToString() ?? "—");
            Text(GameplayUiElementIds.ResultsKills, state.Kills?.ToString("N0") ?? "—");
            Text(GameplayUiElementIds.ResultsRewardCaption, state.Total.HasValue ? "Получено монет" : "Награда ожидает сохранения");
            Text(GameplayUiElementIds.ResultsTotal, state.Total?.ToString("N0") ?? "—");
            Text(GameplayUiElementIds.ResultsLevelReward, state.LevelReward?.ToString("N0") ?? "—");
            Text(GameplayUiElementIds.ResultsBookReward, state.BookReward?.ToString("N0") ?? "—");
            Root.Q(GameplayUiElementIds.ResultsUnlocksGroup).EnableInClassList("results-hidden", state.Unlocks.Count == 0);
            Root.Q(GameplayUiElementIds.ResultsEmpty).EnableInClassList("results-hidden", state.Sets.Count != 0);
            Rows(GameplayUiElementIds.ResultsUnlocks, state.Unlocks, true);
            Rows(GameplayUiElementIds.ResultsSets, state.Sets, false);
        }
        private void Text(string id, string text) => Root.Q<Label>(id).text = text;
        private void Rows(string id, IReadOnlyList<ResultContentViewState> entries, bool showKind)
        {
            var list = Root.Q(id); list.Clear();
            foreach (var entry in entries)
            {
                var row = EntryUi.Box("results-item");
                row.Add(EntryUi.Image(entry.Icon, "results-icon"));
                var copy = EntryUi.Box("results-item-copy");
                copy.Add(EntryUi.Label(entry.Name, "results-item-name"));
                if (showKind) copy.Add(EntryUi.Label(entry.Kind, "results-item-kind"));
                row.Add(copy); list.Add(row);
            }
        }
    }
}
