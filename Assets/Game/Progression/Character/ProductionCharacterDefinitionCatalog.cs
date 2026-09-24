using System;
using Game.Character.Json;
using Game.Content.Json;

namespace Game.Progression
{
    /// <summary>
    /// Production characters (IP-22). FIELD-001 startup packet F1-03 ships CHAR-001 only; later
    /// characters stay unlock metadata in MetaEconomy.json (DECISION-0050/0051). Access comes from
    /// the persistent profile, never from the JSON "initiallyUnlocked" flag.
    /// </summary>
    public static class ProductionCharacterDefinitionCatalog
    {
        public const string ResourcePath = "Content/Characters/ProductionCharacters";
        public const string BaselinePath = "Content/Characters/ProductionCharacterBaseline";

        public static CharacterDefinition[] CreateDefinitions()
        {
            var definitions = FixtureCharacterDefinitionCatalog.LoadDefinitions(
                JsonContentFile.Load<CharacterDefinitionData[]>(ResourcePath));
            foreach (var definition in definitions)
                if (definition.Id.ToString().StartsWith("FIXTURE-", StringComparison.Ordinal))
                    throw new InvalidOperationException($"Production roster cannot contain fixture character '{definition.Id}'.");
            return definitions;
        }

        public static CharacterRoster Create(ICharacterAccessProvider access) =>
            new CharacterRoster(CreateDefinitions(), access ?? throw new ArgumentNullException(nameof(access)));

        public static CharacterComparisonBaseline CreateBaseline() => FixtureCharacterDefinitionCatalog.LoadBaseline(BaselinePath);
    }
}
