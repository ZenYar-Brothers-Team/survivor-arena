using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Content.Json
{
    // Shared entry point for loading config content as JSON: read the resource
    // text once here, deserialize with settings every loader in the project
    // should agree on (string enums, so config stays human/AI-readable).
    public static class JsonContentFile
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() },
            MissingMemberHandling = MissingMemberHandling.Error,
        };

        public static string ReadText(string resourcePath)
        {
            var asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null)
                throw new InvalidOperationException($"Missing JSON content resource 'Resources/{resourcePath}.json'.");

            return asset.text;
        }

        public static T Load<T>(string resourcePath, JsonSerializerSettings settings = null)
        {
            return JsonConvert.DeserializeObject<T>(ReadText(resourcePath), settings ?? Settings);
        }
    }
}
