using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
using Game.Content.Json;
using Game.Traveler.Json;
using Newtonsoft.Json;
namespace Game.Traveler
{
    public sealed class FixtureTravelerCatalog
    {
        public IReadOnlyDictionary<ContentId, TravelerDefinition> Definitions { get; }
        public IReadOnlyList<TravelerScheduleDefinition> Schedules { get; }
        private FixtureTravelerCatalog(TravelerCatalogData data)
        {
            if (data?.Travelers == null || data.Schedules == null) throw new ArgumentException("Traveler catalog arrays required.");
            Definitions = new System.Collections.ObjectModel.ReadOnlyDictionary<ContentId, TravelerDefinition>(data.Travelers.Select(d => new TravelerDefinition(d)).ToDictionary(d => d.Id));
            Schedules = data.Schedules.Select(d => new TravelerScheduleDefinition(d)).ToList().AsReadOnly();
            var ids = new HashSet<ContentId>(Definitions.Keys);
            foreach (var schedule in Schedules)
            {
                if (!ids.Add(schedule.Id)) throw new ArgumentException("Duplicate schedule/content id.");
                foreach (var id in schedule.TravelerIds) if (!Definitions.ContainsKey(id)) throw new ArgumentException("Missing Traveler: " + id);
            }
        }
        public static FixtureTravelerCatalog Create() => FromJson(JsonContentFile.ReadText("Content/Travelers/FixtureTravelers"));
        /// <summary>Production TRAVELER-001/002/005 and the FIELD-001 schedule (F1-07, baseline v1).</summary>
        public static FixtureTravelerCatalog CreateProduction() => FromJson(JsonContentFile.ReadText("Content/Travelers/ProductionTravelers"));
        public static FixtureTravelerCatalog FromJson(string json) => new FixtureTravelerCatalog(JsonConvert.DeserializeObject<TravelerCatalogData>(json, JsonContentFile.Settings));
    }
}
