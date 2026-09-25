using System;
using System.Collections.Generic;
using Game.Content;
using Game.Content.Json;
using Game.Presentation.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Game.Presentation
{
    public static class SkillWorldEffectCatalog
    {
        public const string ResourcePath = "Content/Presentation/SkillWorldEffects";

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Converters = { new StringEnumConverter() },
            MissingMemberHandling = MissingMemberHandling.Error,
        };

        public static IReadOnlyDictionary<ContentId, SkillWorldEffectProfile> Create() =>
            FromJson(JsonContentFile.ReadText(ResourcePath));

        public static IReadOnlyDictionary<ContentId, SkillWorldEffectProfile> FromJson(string json)
        {
            var data = JsonConvert.DeserializeObject<SkillWorldEffectProfileData[]>(json, Settings)
                       ?? throw new InvalidOperationException("Skill world effects must be a JSON array.");
            var result = new Dictionary<ContentId, SkillWorldEffectProfile>();
            foreach (var entry in data)
            {
                if (entry == null) throw new InvalidOperationException("Skill world effects cannot contain null.");
                var profile = new SkillWorldEffectProfile(new ContentId(entry.SkillId), entry.Kind,
                    ToColor(entry.Color, entry.SkillId), ToColor(entry.ImpactColor, entry.SkillId), entry.Thickness, entry.FadeSeconds,
                    entry.PillarWidth, entry.PillarHeight);
                if (!result.TryAdd(profile.SkillId, profile))
                    throw new InvalidOperationException($"Duplicate skill world effect '{entry.SkillId}'.");
            }
            return result;
        }

        private static Color ToColor(float[] rgba, string owner)
        {
            if (rgba == null || rgba.Length != 4)
                throw new InvalidOperationException($"Skill world effect '{owner}' colors require exactly four RGBA values.");
            return new Color(rgba[0], rgba[1], rgba[2], rgba[3]);
        }
    }
}
