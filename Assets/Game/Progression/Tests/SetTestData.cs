using System.Collections.Generic;
namespace Game.Progression.Tests
{
    internal static class SetTestData
    {
        private static BuildEntryDefinition Component(int i) => new BuildEntryDefinition("FIXTURE-RECIPE-" + i, BuildEntryKind.PassiveItem, "Recipe " + i);
        public static SetDefinition Define(string id, string name, params SetRecipeComponent[] recipe)
        {
            var components = new List<SetRecipeComponent>(recipe);
            for (var i = 0; components.Count < 3; i++) components.Add(new SetRecipeComponent(Component(i).Id, BuildEntryKind.PassiveItem, 1));
            return new SetDefinition(id, name, components.ToArray());
        }
        public static PlayerBuild Build(BuildEntryDefinition active)
        {
            var build = new PlayerBuild(active);
            AddComponents(build);
            return build;
        }
        public static void AddComponents(PlayerBuild build)
        {
            for (var i = 0; i < 2; i++) if (!build.TryGetEntry(Component(i).Id, out _)) build.Apply(Component(i));
        }
    }
}
