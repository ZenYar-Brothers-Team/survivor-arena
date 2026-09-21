using System.Collections;
using System.Linq;
using Game.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Bootstrap.PlayModeTests
{
    public sealed class PresentationAdapterSmokeTests
    {
        [UnityTest]
        public IEnumerator FixtureKit_PauseDisableAndReuse_RestoresEveryRole()
        {
            using var kit = new PresentationFixtureKit();
            kit.SetVelocity(Vector2.left);
            kit.Signal(PresentationSignal.Hit);
            kit.Signal(PresentationSignal.Proc);
            kit.Tick(.02f);
            yield return null;
            var scales = kit.Actors.Select(a => a.Renderer.transform.localScale).ToArray();
            kit.SetRunning(false); kit.Tick(1);
            CollectionAssert.AreEqual(scales, kit.Actors.Select(a => a.Renderer.transform.localScale));
            kit.SetRunning(true); kit.Signal(PresentationSignal.Death); kit.Tick(.3f);
            Assert.IsTrue(kit.Actors.All(a => a.Adapter.IsComplete));
            kit.Reset(4); yield return null;
            Assert.AreEqual(24, kit.Actors.Count);
            Assert.IsTrue(kit.Actors.All(a => a.Adapter.IsInitialized && !a.Adapter.IsComplete));
            var actor = kit.Actors[0];
            actor.gameObject.SetActive(false);
            Assert.IsNull(actor.Adapter);
            Assert.IsNull(actor.Renderer.sprite);
            actor.Signal(PresentationSignal.Proc); // stale source has no listener after pool/disable cleanup
            kit.Reset(1);
            Assert.IsTrue(kit.Actors.All(a => a.gameObject.activeSelf && a.Adapter.IsInitialized));
        }
    }
}
