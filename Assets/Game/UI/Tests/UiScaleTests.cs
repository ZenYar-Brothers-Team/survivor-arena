using NUnit.Framework;
namespace Game.UI.Tests
{
    public sealed class UiScaleTests
    {
        [TestCase(720f, 1f)]
        [TestCase(1080f, 1f)]
        [TestCase(1440f, 1.25f)]
        [TestCase(1620f, 1.5f)]
        [TestCase(2160f, 1.5f)]
        public void Resolve_Auto_PicksLargestStepThatFitsScreenHeight(float height, float expected) =>
            Assert.AreEqual(expected, UiScale.Resolve(0f, height));
        [Test]
        public void Resolve_ExplicitStep_IgnoresScreenHeight() => Assert.AreEqual(1.5f, UiScale.Resolve(1.5f, 720f));
        [Test]
        public void Label_AutoAndPercent() { Assert.AreEqual("Авто", UiScale.Label(0f)); Assert.AreEqual("125%", UiScale.Label(1.25f)); }
    }
}
