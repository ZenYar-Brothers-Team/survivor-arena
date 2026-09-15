using System;
using System.Collections.Generic;
using Game.Content.Json;
using Game.Progression.Json;

namespace Game.Progression
{
    public static class FixtureSetCatalog
    {
        private const string ResourcePath = "Content/Sets/FixtureSets";

        public static IReadOnlyList<SetDefinition> Create()
        {
            var data = JsonContentFile.Load<SetDefinitionData[]>(ResourcePath);
            if (data == null || data.Length == 0)
                throw new InvalidOperationException("Fixture set catalog cannot be empty.");

            var definitions = new SetDefinition[data.Length];
            for (var i = 0; i < data.Length; i++)
                definitions[i] = ToDefinition(data[i]);
            return definitions;
        }

        private static SetDefinition ToDefinition(SetDefinitionData data)
        {
            if (data == null)
                throw new InvalidOperationException("Fixture set data cannot contain null entries.");
            if (data.Recipe == null)
                throw new InvalidOperationException($"Fixture set '{data.Id}' is missing its recipe.");

            var recipe = new SetRecipeComponent[data.Recipe.Length];
            for (var i = 0; i < recipe.Length; i++)
            {
                var component = data.Recipe[i] ??
                    throw new InvalidOperationException($"Fixture set '{data.Id}' contains a null recipe component.");
                recipe[i] = new SetRecipeComponent(component.Id, component.Kind, component.MinimumLevel);
            }

            return new SetDefinition(data.Id, data.DisplayName, data.DraftChance, recipe);
        }
    }
}
