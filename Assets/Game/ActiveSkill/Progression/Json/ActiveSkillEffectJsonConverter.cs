using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Game.ActiveSkill.Json
{
    // Reads the "kind" discriminator on each effect object and deserializes
    // into the matching concrete *EffectData DTO. Writing isn't needed: config
    // is authored as JSON directly, not round-tripped from C#.
    public sealed class ActiveSkillEffectJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IActiveSkillEffectData);
        }

        public override bool CanWrite => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var kindToken = obj["kind"];
            if (kindToken == null)
                throw new JsonSerializationException("Active-skill effect entry is missing a \"kind\" field.");

            var kind = kindToken.ToObject<ActiveSkillEffectKind>(serializer);
            // A switch expression over an enum gives a compiler warning if a case
            // is missing — the point of using an enum here instead of a raw string.
            var targetType = kind switch
            {
                ActiveSkillEffectKind.ProjectileBurst => typeof(ProjectileBurstEffectData),
                ActiveSkillEffectKind.Beam => typeof(BeamEffectData),
                ActiveSkillEffectKind.Orbit => typeof(OrbitEffectData),
                ActiveSkillEffectKind.Boomerang => typeof(BoomerangEffectData),
                ActiveSkillEffectKind.Chain => typeof(ChainEffectData),
                ActiveSkillEffectKind.Area => typeof(AreaEffectData),
                ActiveSkillEffectKind.Mine => typeof(MineEffectData),
                _ => throw new JsonSerializationException($"Unknown active-skill effect kind '{kind}'.")
            };

            return obj.ToObject(targetType, serializer);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException("Writing active-skill effect config back to JSON is not supported.");
        }
    }
}
