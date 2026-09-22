using System;
using System.Collections.Generic;
using Game.Content.Json;
using Game.Content;
using Game.Character;
using Game.Progression.Json;
using Game.Presentation;

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

            if (data.Effects == null || data.Effects.Length == 0) throw new InvalidOperationException("Set requires effects.");
            var effects = new SetEffectDefinition[data.Effects.Length];
            for (var i = 0; i < effects.Length; i++)
            {
                var e = data.Effects[i] ?? throw new InvalidOperationException("Null set effect.");
                if (!e.Kind.HasValue) throw new InvalidOperationException("Set effect kind is required.");
                if ((e.Kind == SetEffectKind.StatBuff || e.Kind == SetEffectKind.SkillTransform) && e.Modifier == null)
                    throw new InvalidOperationException("Set modifier required.");
                effects[i] = new SetEffectDefinition(e.Kind.Value,
                    e.Modifier == null ? default : FixturePassiveCatalog.ToModifier(e.Modifier),
                    e.Skill == null ? (ContentId?)null : new ContentId(e.Skill),
                    e.AttackTemplate == null ? (ContentId?)null : new ContentId(e.AttackTemplate),
                    e.CooldownSeconds ?? 0, e.ActivationCount ?? 0, e.HealFraction ?? 0, e.BuffSeconds ?? 0);
            }
            var icon = string.IsNullOrEmpty(data.IconVisualId)
                ? default
                : new ContentRef<SpriteDefinition>(data.IconVisualId);
            return new SetDefinition(data.Id, data.DisplayName, data.Description, effects, icon, recipe);
        }
    }
}
