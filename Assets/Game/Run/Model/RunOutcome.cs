using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Game.Run
{
    /// <summary>Release-safe, once-per-session result. Absent/failed contributors never imply zero.</summary>
    public sealed class RunOutcome
    {
        public Guid RunId { get; }
        public RunCompletionReason Reason { get; }
        public float ElapsedSeconds { get; }
        public float DurationSeconds { get; }
        public RunSelectionSnapshot Selection { get; }
        public IReadOnlyDictionary<string, RunOutcomeContribution> Contributions { get; }
        public IReadOnlyList<string> FailedContributors { get; }

        internal RunOutcome(Guid runId, RunCompletionReason reason, float elapsed, float duration,
            IDictionary<string, RunOutcomeContribution> contributions, IList<string> failedContributors,
            RunSelectionSnapshot selection = null)
        {
            RunId = runId;
            Reason = reason;
            ElapsedSeconds = elapsed;
            DurationSeconds = duration;
            Selection = selection;
            Contributions = new ReadOnlyDictionary<string, RunOutcomeContribution>(
                new Dictionary<string, RunOutcomeContribution>(contributions, StringComparer.Ordinal));
            FailedContributors = new List<string>(failedContributors).AsReadOnly();
        }
    }
}
