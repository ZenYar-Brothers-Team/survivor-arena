using Newtonsoft.Json.Linq;
namespace Game.Meta.Tests
{
    public sealed class SyntheticProfileMigration : IProfileMigration
    {
        public int FromVersion => 0;
        public string Migrate(string json) { var data = JObject.Parse(json); data["schemaVersion"] = 1; return data.ToString(); }
    }
}
