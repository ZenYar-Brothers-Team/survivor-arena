using System;
using System.Linq;
using Game.Content;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Presentation.Tests
{
    public sealed class GenericPresentationTests
    {
        [Test]
        public void Adapter_HitProcPauseFadeReuse_PreservesRootAndRestoresRenderer()
        {
            var root = new GameObject("gameplay");
            SpritePresentationAdapter adapter = null;
            try
            {
                var collider = root.AddComponent<BoxCollider2D>();
                var child = new GameObject("VisualRoot"); child.transform.SetParent(root.transform, false);
                var renderer = child.AddComponent<SpriteRenderer>(); renderer.color = Color.cyan;
                adapter = new SpritePresentationAdapter(root.transform, renderer);
                var body = FixtureSpriteCatalog.CreateFor(new ContentId[] { "FIXTURE-CHARACTER-AGILE-VISUAL-BODY" })[0];
                var source = new FakePresentationSource { Velocity = Vector2.left * 3 };
                var feedback = new PresentationFeedbackProfile(.2f, .15f, .06f);
                var motion = FixtureSpriteMotionProfileCatalog.Create()[0];
                adapter.Initialize(body, SpriteRole.Body, source, feedback, motion);
                source.Emit(PresentationSignal.Hit); source.Emit(PresentationSignal.Proc);
                adapter.Tick(.025f);
                Assert.IsTrue(renderer.flipX);
                Assert.AreNotEqual(Vector3.one, child.transform.localScale);
                Assert.AreEqual(Vector3.zero, root.transform.position);
                Assert.AreEqual(Vector3.one, root.transform.localScale);
                Assert.AreEqual(Vector2.one, collider.size);
                source.IsRunning = false;
                var scale = child.transform.localScale; var color = renderer.color;
                adapter.Tick(10);
                Assert.AreEqual(scale, child.transform.localScale); Assert.AreEqual(color, renderer.color);
                source.IsRunning = true; source.Emit(PresentationSignal.Death);
                adapter.Tick(.1f); Assert.AreEqual(.5f, renderer.color.a, .0001f);
                adapter.Tick(.1f); Assert.IsTrue(adapter.IsComplete);
                var next = new FakePresentationSource();
                adapter.Initialize(body, SpriteRole.Body, next, feedback, motion);
                source.Emit(PresentationSignal.Death);
                adapter.Tick(.3f);
                Assert.AreEqual(1, renderer.color.a); Assert.IsFalse(adapter.IsComplete);
                adapter.Shutdown();
                Assert.AreEqual(Color.cyan, renderer.color); Assert.IsNull(renderer.sprite);
                Assert.AreEqual(Vector3.one, child.transform.localScale); Assert.IsFalse(renderer.flipX);
            }
            finally { adapter?.Dispose(); Object.DestroyImmediate(root); }
        }

        [Test]
        public void Kit_ReturnRentAndCollect_AllRolesResetWithoutStaleFade()
        {
            using var kit = new PresentationFixtureKit();
            Assert.AreEqual(6, kit.Actors.Count);
            var first = kit.Actors.ToArray();
            kit.Signal(PresentationSignal.Collect); kit.Tick(.3f);
            Assert.IsTrue(kit.Actors.All(a => a.Adapter.IsComplete));
            kit.Reset(1);
            CollectionAssert.AreEquivalent(first, kit.Actors);
            Assert.IsTrue(kit.Actors.All(a => a.Renderer.color.a > 0 && !a.Adapter.IsComplete));
            kit.SetRunning(false); kit.Reset(4); kit.Tick(20);
            Assert.AreEqual(24, kit.Actors.Count);
            Assert.IsTrue(kit.Actors.All(a => !a.IsRunning));
        }

        [Test]
        public void Adapter_RejectsGameplayRootAndPhysicsInVisualSubtree()
        {
            var root = new GameObject("root");
            try
            {
                Assert.Throws<InvalidOperationException>(() => new SpritePresentationAdapter(root.transform, root.AddComponent<SpriteRenderer>()));
                var child = new GameObject("child"); child.transform.SetParent(root.transform, false);
                var renderer = child.AddComponent<SpriteRenderer>(); child.AddComponent<BoxCollider2D>();
                Assert.Throws<InvalidOperationException>(() => new SpritePresentationAdapter(root.transform, renderer));
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void PortraitCrop_CentersPivotAndKeepsSourceTextureAndRejectsWrongRole()
        {
            var body = FixtureSpriteCatalog.CreateFor(new ContentId[] { "FIXTURE-CHARACTER-AGILE-VISUAL-BODY" })[0];
            using var crop = new SpritePortraitCrop("FIXTURE-UI-VISUAL-PORTRAIT", body, new Rect(.25f, .25f, .5f, .5f));
            Assert.AreSame(body.Sprite.texture, crop.Definition.Sprite.texture);
            Assert.AreEqual(new Vector2(128, 128), crop.Definition.Sprite.pivot);
            Assert.AreEqual(256, crop.Definition.Sprite.rect.width);
            Assert.Throws<InvalidOperationException>(() => crop.Definition.RequireRole(SpriteRole.Body));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SpritePortraitCrop("FIXTURE-INVALID", body, new Rect(.5f, 0, 1, 1)));
            Assert.Throws<InvalidOperationException>(() => FixtureSpriteCatalog.CreateFor(new ContentId[] { "CHAR-001-VISUAL-MISSING" }));
        }
    }
}
