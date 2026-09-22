using System;
using System.Collections.Generic;
using Game.Content;
using Game.Presentation;

namespace Game.Progression
{
    public sealed class SetDefinition : BuildEntryDefinition, IReferencesContent
    {
        private readonly SetRecipeComponent[] _recipe;
        public string Description { get; }
        public IReadOnlyList<SetEffectDefinition> Effects { get; }
        public ContentRef<SpriteDefinition> Icon { get; }

        public IReadOnlyList<SetRecipeComponent> Recipe => _recipe;
        public override int LevelCap => 1;

        public SetDefinition(
            ContentId id,
            string displayName,
            params SetRecipeComponent[] recipe)
            : this(id, displayName, "", Array.Empty<SetEffectDefinition>(), default, recipe) { }

        public SetDefinition(ContentId id, string displayName, string description, IEnumerable<SetEffectDefinition> effects,
            params SetRecipeComponent[] recipe)
            : this(id, displayName, description, effects, default, recipe) { }

        public SetDefinition(ContentId id, string displayName, string description, IEnumerable<SetEffectDefinition> effects,
            ContentRef<SpriteDefinition> icon, params SetRecipeComponent[] recipe) : base(id, BuildEntryKind.Set, displayName)
        {
            if (recipe == null || recipe.Length < 3 || recipe.Length > 6)
                throw new ArgumentException("Set recipe requires 3-6 components.", nameof(recipe));

            var ids = new HashSet<ContentId>();
            for (var i = 0; i < recipe.Length; i++)
            {
                if (recipe[i] == null)
                    throw new ArgumentException("Set recipe cannot contain null components.", nameof(recipe));
                if (!ids.Add(recipe[i].Id))
                    throw new ArgumentException($"Set recipe contains duplicate component '{recipe[i].Id}'.", nameof(recipe));
            }

            _recipe = (SetRecipeComponent[])recipe.Clone();
            Description = description ?? throw new ArgumentNullException(nameof(description));
            var list = new List<SetEffectDefinition>(effects ?? throw new ArgumentNullException(nameof(effects)));
            if (list.Exists(effect => effect == null)) throw new ArgumentException("Null set effect.", nameof(effects));
            Effects = list.AsReadOnly();
            Icon = icon;
        }

        public bool IsRecipeFulfilled(PlayerBuild build)
        {
            return GetFulfilledComponentCount(build) == _recipe.Length;
        }

        public int GetFulfilledComponentCount(PlayerBuild build)
        {
            if (build == null)
                throw new ArgumentNullException(nameof(build));

            var fulfilled = 0;
            for (var i = 0; i < _recipe.Length; i++)
            {
                if (_recipe[i].IsFulfilled(build))
                    fulfilled++;
            }
            return fulfilled;
        }

        public IEnumerable<ContentReference> GetReferencedContent()
        {
            if (Icon.Id.IsValid)
                yield return Icon.ToReference();

            foreach (var effect in Effects)
            {
                if (effect.Skill.HasValue) yield return new ContentReference(effect.Skill.Value, typeof(BuildEntryDefinition));
                if (effect.AttackTemplate.HasValue) yield return new ContentReference(effect.AttackTemplate.Value, typeof(BuildEntryDefinition));
            }
            for (var i = 0; i < _recipe.Length; i++)
                yield return new ContentReference(_recipe[i].Id, typeof(BuildEntryDefinition));
        }
    }
}
