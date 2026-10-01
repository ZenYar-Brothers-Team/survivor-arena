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
            if (recipe == null || recipe.Length < 2 || recipe.Length > 6)
                throw new ArgumentException("Set recipe requires 2-6 components (2-4 for low-tier sets, DECISION-0138; 3-6 otherwise).", nameof(recipe));

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

        /// <summary>
        /// False when the recipe can no longer be fulfilled in this run (DECISION-0073): a missing component cannot be
        /// offered any more (banished, locked, zero weight) or the free active/passive slots cannot hold the missing
        /// components. Owned components below their threshold stay reachable through upgrades.
        /// </summary>
        public bool CanStillBeFulfilled(PlayerBuild build, Func<ContentId, bool> canBeOffered)
        {
            if (build == null) throw new ArgumentNullException(nameof(build));
            if (canBeOffered == null) throw new ArgumentNullException(nameof(canBeOffered));
            var missingActive = 0;
            var missingPassive = 0;
            for (var i = 0; i < _recipe.Length; i++)
            {
                var component = _recipe[i];
                if (component.IsFulfilled(build)) continue;
                if (!canBeOffered(component.Id)) return false;
                if (build.TryGetEntry(component.Id, out _)) continue;
                if (component.Kind == BuildEntryKind.ActiveSkill) missingActive++;
                else missingPassive++;
            }
            return missingActive <= PlayerBuild.ActiveSlotCapacity - build.ActiveCount &&
                   missingPassive <= PlayerBuild.PassiveSlotCapacity - build.PassiveCount;
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
