using Newtonsoft.Json.Linq;

namespace Game.Meta
{
    /// <summary>Adds earned achievement counters without inventing facts about old runs.</summary>
    public sealed class ProfileMigrationV2ToV3 : IProfileMigration
    {
        public int FromVersion => 2;
        public string Migrate(string json)
        {
            var data = JObject.Parse(json);
            if (data["achievementProgress"] == null) data["achievementProgress"] = new JObject();
            data["schemaVersion"] = 3;
            return data.ToString(Newtonsoft.Json.Formatting.None);
        }
    }
}
