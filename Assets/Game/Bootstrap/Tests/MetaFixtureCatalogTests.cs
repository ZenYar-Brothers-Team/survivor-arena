using Game.Meta;
using NUnit.Framework;
namespace Game.Bootstrap.Tests
{
    public sealed class MetaFixtureCatalogTests
    {
        [Test] public void FixtureAccess_AllBuildCharacterFieldIdsHaveRules()
        {
            var meta=MetaCatalog.Load(true);var runtime=FixtureRuntimeContentCatalog.Create();
            foreach(var item in runtime.BuildEntries)Assert.IsTrue(meta.Unlocks.ContainsKey(item.Id.ToString()),item.Id.ToString());
            foreach(var item in runtime.Characters.AllCharacters)Assert.IsTrue(meta.Unlocks.ContainsKey(item.Id.ToString()),item.Id.ToString());
            foreach(var item in runtime.Fields.Roster.AllFields)Assert.IsTrue(meta.Unlocks.ContainsKey(item.Id.ToString()),item.Id.ToString());
            foreach(var item in meta.Unlocks.Values)
                Assert.IsTrue(System.Linq.Enumerable.Any(runtime.BuildEntries,entry=>entry.Id.ToString()==item.Id)||
                    System.Linq.Enumerable.Any(runtime.Characters.AllCharacters,entry=>entry.Id.ToString()==item.Id)||
                    System.Linq.Enumerable.Any(runtime.Fields.Roster.AllFields,entry=>entry.Id.ToString()==item.Id),item.Id);
        }
    }
}
