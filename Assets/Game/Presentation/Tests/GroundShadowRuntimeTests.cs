using NUnit.Framework;
using UnityEngine;

namespace Game.Presentation.Tests
{
    public sealed class GroundShadowRuntimeTests
    {
        [Test]
        public void Initialize_TwoActors_ReuseMaskAndAlignWithContactGroundPoint()
        {
            var first = new GameObject("first");
            var second = new GameObject("second");
            try
            {
                var firstBody = first.AddComponent<SpriteRenderer>();
                var secondBody = second.AddComponent<SpriteRenderer>();
                var profile = new GroundShadowPresentationProfile(.86f, 1.2f, .24f, .02f, .5f,
                    new Color(.1f, .08f, .15f, .3f));
                var firstContact = new SpriteContactProfile(.3f, .45f);
                var secondContact = new SpriteContactProfile(.4f, .45f);
                var firstShadow = first.AddComponent<GroundShadowRuntime>();
                var secondShadow = second.AddComponent<GroundShadowRuntime>();

                firstShadow.Initialize(profile, firstContact, 1f, firstBody);
                secondShadow.Initialize(profile, secondContact, .8f, secondBody);

                Assert.AreSame(firstShadow.Renderer.sprite, secondShadow.Renderer.sprite);
                Assert.AreEqual(-.43f, firstShadow.Renderer.transform.localPosition.y, .0001f);
                Assert.AreEqual(.72f, firstShadow.Renderer.transform.localScale.x, .0001f);
                Assert.AreEqual(1.2f, secondShadow.Renderer.transform.localScale.x, .0001f);
                Assert.AreEqual(.3f, secondShadow.Renderer.transform.localScale.y, .0001f);
                Assert.AreEqual(-1, firstShadow.Renderer.sortingOrder);
                Assert.IsEmpty(firstShadow.GetComponentsInChildren<Collider2D>());
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }
    }
}
