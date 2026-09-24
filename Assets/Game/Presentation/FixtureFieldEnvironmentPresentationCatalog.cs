using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
using Game.Content.Json;
using Game.Presentation.Json;
using Newtonsoft.Json;

namespace Game.Presentation
{
    public static class FixtureFieldEnvironmentPresentationCatalog
    {
        private const string ResourcePath = "Content/Presentation/FixtureFieldEnvironmentPresentation";

        public static IReadOnlyDictionary<ContentId, FieldEnvironmentPresentationDefinition> Create() => Load(ResourcePath);

        /// <summary>Loads fixture or production field presentation (Content/Presentation/ProductionFieldEnvironmentPresentation).</summary>
        public static IReadOnlyDictionary<ContentId, FieldEnvironmentPresentationDefinition> Load(string resourcePath)
        {
            var data = JsonConvert.DeserializeObject<FieldEnvironmentPresentationData[]>(
                JsonContentFile.ReadText(resourcePath), JsonContentFile.Settings)
                ?? throw new ArgumentException("Field environment presentation config is required.");
            var definitions = data.Select(entry => new FieldEnvironmentPresentationDefinition(entry)).ToList();
            if (definitions.Count == 0 || definitions.Select(item => item.EnvironmentId).Distinct().Count() != definitions.Count)
                throw new ArgumentException("Field environment presentation requires unique environment bindings.");
            return definitions.ToDictionary(item => item.EnvironmentId);
        }
    }
}
