using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
namespace Game.Progression.Tests
{
    public sealed class SetDraftPolicyTests
    {
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(5)]
        public void GlobalChecks_PrioritizeAllSuccessesInStableOrder(int successes)
        {
            var active = new BuildEntryDefinition("FIXTURE-SKILL", BuildEntryKind.ActiveSkill, "Skill");
            var sets = Enumerable.Range(0,5).Select(i => SetTestData.Define("FIXTURE-SET-"+i, "Set", new SetRecipeComponent(active.Id, BuildEntryKind.ActiveSkill, 1))).ToArray();
            var definitions = new List<BuildEntryDefinition>(sets.Reverse());
            definitions.Add(active); definitions.Add(new BuildEntryDefinition("FIXTURE-P1", BuildEntryKind.PassiveItem,"P1"));
            definitions.Add(new BuildEntryDefinition("FIXTURE-P2", BuildEntryKind.PassiveItem,"P2"));
            var rolls = Enumerable.Range(0,5).Select(i => i < successes ? 0.1f : 0.9f).Concat(new[]{0f,0f,0f}).ToArray();
            var pool = new DraftPool(definitions, setOffers: new FixtureSetDraftOfferProvider(0.5f));
            var options = pool.CreateOptions(SetTestData.Build(active),3,new SequenceDraftRandom(rolls));
            Assert.AreEqual(3,options.Count);
            for (var i=0; i<System.Math.Min(successes,3); i++) Assert.AreEqual(sets[i].Id,options[i].Definition.Id);
            Assert.AreEqual(System.Math.Min(successes,3),options.Count(o=>o.Definition.Kind==BuildEntryKind.Set));
        }
        [TestCase(0f, 0)] [TestCase(1f, 3)]
        public void GlobalChance_ExtremesApplyEquallyToEverySet(float chance,int expected)
        {
            var active = new BuildEntryDefinition("FIXTURE-SKILL",BuildEntryKind.ActiveSkill,"Skill");
            var definitions = new List<BuildEntryDefinition>{active,
                new BuildEntryDefinition("FIXTURE-P1",BuildEntryKind.PassiveItem,"P1"),
                new BuildEntryDefinition("FIXTURE-P2",BuildEntryKind.PassiveItem,"P2")};
            for(var i=0;i<4;i++) definitions.Add(SetTestData.Define("FIXTURE-SET-"+i,"Set",new SetRecipeComponent(active.Id,BuildEntryKind.ActiveSkill,1)));
            var result = new DraftPool(definitions,setOffers:new FixtureSetDraftOfferProvider(chance))
                .CreateOptions(SetTestData.Build(active),3,new FixedDraftRandom(0.4f));
            Assert.AreEqual(expected,result.Count(o=>o.Definition.Kind==BuildEntryKind.Set));
        }
    }
}
