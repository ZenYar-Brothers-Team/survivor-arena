using NUnit.Framework;

namespace Game.Presentation.Tests
{
    public sealed class ScreenShakeRequestGateTests
    {
        [Test]
        public void Request_OffOrPaused_IsSuppressedAndToggleIsReadForEachRequest()
        {
            var preference = new FakeScreenShakePreference();
            var gate = new ScreenShakeRequestGate(preference);
            var requests = 0;
            gate.Requested += (a, s) => { requests++; Assert.AreEqual(.1f, a); Assert.AreEqual(.2f, s); };
            gate.Request(true, .1f, .2f); Assert.AreEqual(0, requests);
            preference.ScreenShakeEnabled = true;
            gate.Request(false, .1f, .2f); Assert.AreEqual(0, requests);
            gate.Request(true, .1f, .2f); Assert.AreEqual(1, requests);
            preference.ScreenShakeEnabled = false;
            gate.Request(true, .1f, .2f); Assert.AreEqual(1, requests);
        }
    }
}
