using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
using Game.Field;
using Game.Traveler.Json;
namespace Game.Traveler
{
    public sealed class TravelerScheduleDefinition : FieldTravelerScheduleDefinition, IReferencesContent
    {
        public IReadOnlyList<ContentId> TravelerIds { get; }
        public IReadOnlyList<float> CountProbabilities { get; }
        public int Seed { get; }
        public int FieldRank { get; }
        public int PlacementAttempts { get; }
        public float EndBufferSeconds { get; }
        public float SpawnScreenHeights { get; }
        public float FieldGrowth { get; }
        public float TimeGrowth { get; }
        public TravelerScheduleDefinition(TravelerScheduleData data) : base(data.Id)
        {
            TravelerIds = (data.TravelerIds ?? throw new ArgumentException("travelerIds required.")).Select(id => new ContentId(id)).ToList().AsReadOnly();
            if (TravelerIds.Count < 3 || TravelerIds.Distinct().Count() != TravelerIds.Count) throw new ArgumentException("At least three unique traveler IDs required.");
            var probabilities = data.CountProbabilities ?? throw new ArgumentException("countProbabilities required.");
            if (probabilities.Length != 4 || Math.Abs(probabilities.Sum() - 1) > .00001f) throw new ArgumentException("Four normalized count probabilities required.");
            foreach (var value in probabilities) NumericValidation.ValidateRange(value, 0, 1, "probability");
            CountProbabilities = Array.AsReadOnly((float[])probabilities.Clone());
            Seed = data.Seed ?? throw new ArgumentException("seed required.");
            FieldRank = data.FieldRank ?? throw new ArgumentException("fieldRank required.");
            NumericValidation.ValidateRange(FieldRank, 1, 10, nameof(FieldRank));
            PlacementAttempts = data.PlacementAttempts ?? throw new ArgumentException("placementAttempts required.");
            NumericValidation.ValidateCount(PlacementAttempts, nameof(PlacementAttempts));
            EndBufferSeconds = data.EndBufferSeconds ?? throw new ArgumentException("endBufferSeconds required.");
            SpawnScreenHeights = data.SpawnScreenHeights ?? throw new ArgumentException("spawnScreenHeights required.");
            NumericValidation.ValidatePositive(EndBufferSeconds, nameof(EndBufferSeconds));
            NumericValidation.ValidatePositive(SpawnScreenHeights, nameof(SpawnScreenHeights));
            FieldGrowth = data.FieldGrowth ?? throw new ArgumentException("fieldGrowth required.");
            TimeGrowth = data.TimeGrowth ?? throw new ArgumentException("timeGrowth required.");
            NumericValidation.ValidateRange(FieldGrowth, 0, 1, nameof(FieldGrowth));
            NumericValidation.ValidateRange(TimeGrowth, 0, 1, nameof(TimeGrowth));
        }
        public float Scale(float spawnTime, float duration)
        {
            NumericValidation.ValidateNonNegative(spawnTime, nameof(spawnTime));
            NumericValidation.ValidatePositive(duration, nameof(duration));
            if (duration <= EndBufferSeconds) throw new ArgumentException("Run must exceed end buffer.");
            return (1 + FieldGrowth * (FieldRank - 1)) * (1 + TimeGrowth * Math.Min(1, spawnTime / (duration - EndBufferSeconds)));
        }
        public IReadOnlyList<TravelerScheduleEntry> Draw(float duration, Random random)
        {
            Scale(0, duration);
            var draw = random.NextDouble(); var cumulative = 0f; var count = 3;
            for (var i = 0; i < 4; i++) { cumulative += CountProbabilities[i]; if (draw < cumulative) { count = i; break; } }
            var pool = TravelerIds.ToList(); var entries = new List<TravelerScheduleEntry>();
            for (var i = 0; i < count; i++)
            {
                var index = random.Next(pool.Count); var id = pool[index]; pool.RemoveAt(index);
                var time = (float)random.NextDouble() * (duration - EndBufferSeconds);
                entries.Add(new TravelerScheduleEntry(id, time, i, Scale(time, duration)));
            }
            return entries.OrderBy(item => item.Time).ThenBy(item => item.Sequence).ToList().AsReadOnly();
        }
        public IEnumerable<ContentReference> GetReferencedContent() => TravelerIds.Select(id => new ContentRef<TravelerDefinition>(id).ToReference());
    }
}
