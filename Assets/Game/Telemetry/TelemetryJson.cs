using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Game.Telemetry
{
    /// <summary>Camel-case field names, preserving case-sensitive content IDs and contributor keys.</summary>
    public static class TelemetryJson
    {
        public static JsonSerializer CreateSerializer() => JsonSerializer.Create(new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy { ProcessDictionaryKeys = false }
            },
            Converters = { new StringEnumConverter() }
        });
    }
}
