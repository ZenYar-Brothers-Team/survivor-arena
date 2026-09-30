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
        public int MinCount { get; }
        public int MaxCount => MinCount + CountProbabilities.Count - 1;
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
            MinCount = data.MinCount ?? throw new ArgumentException("minCount required.");
            NumericValidation.ValidateNonNegative(MinCount, nameof(MinCount));
            if (probabilities.Length == 0 || Math.Abs(probabilities.Sum() - 1) > .00001f) throw new ArgumentException("Normalized count probabilities required.");
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
        /// <summary>
        /// Draws the count (MinCount…MaxCount) and the Travelers (DECISION-0122). Types are the roles of <paramref name="roleOf"/>
        /// (without it every Traveler is its own type). The draw runs in groups of one Traveler per type in a random type order, so every
        /// full group has all types once and a last, shorter group has distinct types: 5 with three roles = 3 + 2, 8 = 3 + 3 + 2.
        /// Inside a type, Travelers come from a shuffled bag and are refilled only when it is empty, so a Traveler does not repeat
        /// before all of its type were seen. Works for any count.
        /// </summary>
        public IReadOnlyList<TravelerScheduleEntry> Draw(float duration, Random random, Func<ContentId, TravelerRole> roleOf = null)
        {
            Scale(0, duration);
            var draw = random.NextDouble(); var cumulative = 0f; var count = MaxCount;
            for (var i = 0; i < CountProbabilities.Count; i++) { cumulative += CountProbabilities[i]; if (draw < cumulative) { count = MinCount + i; break; } }
            var typeOf = new Dictionary<ContentId, int>();
            for (var i = 0; i < TravelerIds.Count; i++) typeOf[TravelerIds[i]] = roleOf != null ? (int)roleOf(TravelerIds[i]) : i;
            var types = typeOf.Values.Distinct().ToList();
            var bags = types.ToDictionary(type => type, type => new List<ContentId>());
            var lastPicked = new Dictionary<int, ContentId>();
            var group = new List<int>(); var entries = new List<TravelerScheduleEntry>();
            for (var i = 0; i < count; i++)
            {
                if (group.Count == 0) { group.AddRange(types); Shuffle(group, random); }
                var type = group[group.Count - 1]; group.RemoveAt(group.Count - 1);
                var bag = bags[type];
                if (bag.Count == 0)
                {
                    bag.AddRange(TravelerIds.Where(id => typeOf[id] == type)); Shuffle(bag, random);
                    // the last pick of the previous bag is taken from the end, so keep it from coming straight back
                    if (bag.Count > 1 && lastPicked.TryGetValue(type, out var previous) && bag[bag.Count - 1] == previous)
                    { var last = bag.Count - 1; var swap = bag[0]; bag[0] = bag[last]; bag[last] = swap; }
                }
                var id = bag[bag.Count - 1]; bag.RemoveAt(bag.Count - 1); lastPicked[type] = id;
                var time = (float)random.NextDouble() * (duration - EndBufferSeconds);
                entries.Add(new TravelerScheduleEntry(id, time, i, Scale(time, duration)));
            }
            return entries.OrderBy(item => item.Time).ThenBy(item => item.Sequence).ToList().AsReadOnly();
        }
        private static void Shuffle<T>(IList<T> list, Random random)
        {
            for (var i = list.Count - 1; i > 0; i--)
            { var j = random.Next(i + 1); var swap = list[i]; list[i] = list[j]; list[j] = swap; }
        }
        public IEnumerable<ContentReference> GetReferencedContent() => TravelerIds.Select(id => new ContentRef<TravelerDefinition>(id).ToReference());
    }
}
