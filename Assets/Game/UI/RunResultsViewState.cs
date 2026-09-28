using System.Collections.Generic;

namespace Game.UI
{
    public sealed class RunResultsViewState
    {
        public string Outcome { get; }
        public string Selection { get; }
        public string Time { get; }
        public int? Level { get; }
        public int? Kills { get; }
        public long? LevelReward { get; }
        public long? BookReward { get; }
        public long? Total { get; }
        public bool IsVictory { get; }
        public bool CanRetrySave { get; }
        public string SaveStatus { get; }
        public IReadOnlyList<ResultContentViewState> Sets { get; }
        public IReadOnlyList<ResultContentViewState> Unlocks { get; }
        public RunResultsViewState(string outcome, string selection, string time, int? level, int? kills,
            long? levelReward, long? bookReward, long? total, bool victory, bool canRetrySave, string saveStatus,
            IEnumerable<ResultContentViewState> sets, IEnumerable<ResultContentViewState> unlocks)
        {
            Outcome = outcome; Selection = selection; Time = time; Level = level; Kills = kills;
            LevelReward = levelReward; BookReward = bookReward; Total = total; IsVictory = victory;
            CanRetrySave = canRetrySave; SaveStatus = saveStatus;
            Sets = new List<ResultContentViewState>(sets).AsReadOnly();
            Unlocks = new List<ResultContentViewState>(unlocks).AsReadOnly();
        }
    }
}
