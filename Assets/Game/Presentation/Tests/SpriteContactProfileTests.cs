using System;
using System.IO;
using System.Collections.Generic;
using Game.Content;
using NUnit.Framework;
using UnityEngine;

namespace Game.Presentation.Tests
{
    public sealed class SpriteContactProfileTests
    {
        [TestCase("FIXTURE-CHARACTER-AGILE-VISUAL-BODY", "Characters/fixture-character-agile/fixture-character-agile-body")]
        [TestCase("FIXTURE-ENEMY-SEEKER-VISUAL", "Enemies/enemy-001/enemy-001-body")]
        public void Circle_IsInscribedInOuterEnvelope_WithoutUnusedRadialMargin(string id, string path)
        {
            var visual = FixtureSpriteCatalog.CreateFor(new ContentId[] { id })[0];
            var profile = visual.Contact;
            Assert.IsNotNull(profile);
            var texture = new Texture2D(2, 2);
            try
            {
                texture.LoadImage(File.ReadAllBytes("Assets/Resources/Art/Sprites/" + path + ".png"));
                var center = visual.Sprite.pivot + Vector2.up * profile.CenterY * visual.Sprite.pixelsPerUnit;
                Assert.GreaterOrEqual(profile.Radius, .24f, "The original tiny body circles must not return.");
                var outline = new List<Vector2>();
                for (var y = 0; y < texture.height; y++)
                {
                    var left = -1;
                    var right = -1;
                    for (var x = 0; x < texture.width; x++)
                        if (texture.GetPixel(x, y).a >= 230f / 255)
                        {
                            if (left < 0) left = x;
                            right = x;
                        }
                    if (left >= 0) { outline.Add(new Vector2(left, y)); outline.Add(new Vector2(right, y)); }
                }
                // Support planes of the filled outer hull ignore arm/body and leg gaps.
                var smallestGap = float.PositiveInfinity;
                for (var a = 0; a < 360; a++)
                {
                    var angle = a * Mathf.Deg2Rad;
                    var normal = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    var support = float.NegativeInfinity;
                    foreach (var point in outline) support = Mathf.Max(support, Vector2.Dot(point, normal));
                    smallestGap = Mathf.Min(smallestGap, support - Vector2.Dot(center, normal) -
                        profile.Radius * visual.Sprite.pixelsPerUnit);
                    Assert.LessOrEqual(Vector2.Dot(center, normal) + profile.Radius * visual.Sprite.pixelsPerUnit,
                        support + 1f, id + " angle " + a);
                }
                Assert.Less(smallestGap, 1f, "Circle must reach the outer envelope within pixel/sampling tolerance.");
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }

        [Test]
        public void Circle_RejectsInvalidValues_AndUsesFixedRootCenter()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SpriteContactProfile(0, .5f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SpriteContactProfile(.1f, float.NaN));
            var root = new GameObject("contact test");
            try
            {
                var circle = root.AddComponent<CircleCollider2D>();
                new SpriteContactProfile(.2f, .5f).Apply(circle, 2);
                Assert.AreEqual(.1f, circle.radius);
                Assert.AreEqual(Vector2.zero, circle.offset);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
    }
}
