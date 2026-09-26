using Newtonsoft.Json.Linq;
namespace Game.Meta
{
    /// <summary>DECISION-0064: v2 adds the player's "permanent upgrades disabled" choice; old profiles keep upgrades active.</summary>
    public sealed class ProfileMigrationV1ToV2 : IProfileMigration
    {
        public int FromVersion => 1;
        public string Migrate(string json)
        {
            var data = JObject.Parse(json);
            if (data["upgradesDisabled"] == null) data["upgradesDisabled"] = false;
            data["schemaVersion"] = 2;
            return data.ToString(Newtonsoft.Json.Formatting.None);
        }
    }
}
