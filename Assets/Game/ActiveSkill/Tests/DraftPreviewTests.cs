using Game.Progression;
using NUnit.Framework;

namespace Game.ActiveSkill.Tests
{
    public sealed class DraftPreviewTests
    {
        [Test]
        public void SkillPreview_SnapshotsCurrentAndNextBaseValuesWithoutApplyingUpgrade()
        {
            var levels = new ActiveSkillLevelDefinition[6];
            for (var i = 0; i < levels.Length; i++)
                levels[i] = new ActiveSkillLevelDefinition(10f + i, 6f - i,
                    ActiveSkillTargetingMode.NearestEnemy,
                    new ActiveSkillActivationWave(0f, 0f, 1f, new AreaEffect(2f)));
            var definition = new ActiveSkillProgressionDefinition("FIXTURE-PREVIEW", "Preview", levels);
            var build = new PlayerBuild(definition);
            var option = new DraftPool(new[] { definition }).CreateOptions(build, 3)[0];
            Assert.AreEqual(1, option.Preview.CurrentLevel);
            Assert.AreEqual(2, option.Preview.NextLevel);
            Assert.AreEqual(10f, option.Preview.Values[0].Current);
            Assert.AreEqual(11f, option.Preview.Values[0].Next);
            Assert.AreEqual(6f, option.Preview.Values[1].Current);
            Assert.AreEqual(5f, option.Preview.Values[1].Next);
            Assert.IsTrue(build.TryGetEntry(definition.Id, out var entry));
            Assert.AreEqual(1, entry.Level);
            build.Apply(definition);
            Assert.AreEqual(1, option.Preview.CurrentLevel);
            Assert.AreEqual(10f, option.Preview.Values[0].Current);
        }
    }
}
