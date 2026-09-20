using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Progression
{
    public sealed class SetDefinition : BuildEntryDefinition, IReferencesContent
    {
        private readonly SetRecipeComponent[] _recipe;

        public IReadOnlyList<SetRecipeComponent> Recipe => _recipe;
        public override int LevelCap => 1;
        public override float DraftChance { get; }

        public SetDefinition(
            ContentId id,
            string displayName,
            float draftChance,
            params SetRecipeComponent[] recipe)
            : base(id, BuildEntryKind.Set, displayName)
        {
            NumericValidation.ValidateRange(draftChance, 0f, 1f, nameof(draftChance));
            if (recipe == null || recipe.Length == 0)
                throw new ArgumentException("Set recipe requires at least one component.", nameof(recipe));

            var ids = new HashSet<ContentId>();
            for (var i = 0; i < recipe.Length; i++)
            {
                if (recipe[i] == null)
                    throw new ArgumentException("Set recipe cannot contain null components.", nameof(recipe));
                if (!ids.Add(recipe[i].Id))
                    throw new ArgumentException($"Set recipe contains duplicate component '{recipe[i].Id}'.", nameof(recipe));
            }

            DraftChance = draftChance;
            _recipe = (SetRecipeComponent[])recipe.Clone();
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
            for (var i = 0; i < _recipe.Length; i++)
                yield return new ContentReference(_recipe[i].Id, typeof(BuildEntryDefinition));
        }
    }
}
