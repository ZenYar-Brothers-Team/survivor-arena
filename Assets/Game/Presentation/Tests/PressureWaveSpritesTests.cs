using System;
using Game.Content;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Presentation.Tests
{
    public sealed class PressureWaveSpritesTests
    {
        [Test]
        public void Catalog_PressureDetailRejectsIncompleteOrUnsupportedProfiles()
        {
            Assert.Throws<ArgumentException>(() => new SkillWorldEffectProfile(new ContentId("FIXTURE-WAVE"),
                SkillWorldEffectKind.ExpandingRing, Color.white, Color.white, .1f, .2f, bandFraction: .2f));
            Assert.Throws<ArgumentException>(() => new SkillWorldEffectProfile(new ContentId("FIXTURE-WAVE"),
                SkillWorldEffectKind.Beam, Color.white, Color.white, .1f, .2f, bandFraction: .2f, tailFadePower: 2f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SkillWorldEffectProfile(new ContentId("FIXTURE-WAVE"),
                SkillWorldEffectKind.ExpandingRing, Color.white, Color.white, .1f, .2f, bandFraction: float.NaN));
        }

        [TestCase("SKILL-004", 360f)]
        [TestCase("SET-022-ATTACK", 60f)]
        public void PressureMask_LeavesCenterClearAndInkInsideHitGeometry(string id, float arc)
        {
            var profile = SkillWorldEffectCatalog.Create()[new ContentId(id)];
            foreach (var tail in new[] { true, false })
            {
                var sprite = PressureWaveSprites.Create(profile, arc, tail, keepReadable: true);
                var readable = sprite.texture;
                try
                {
                    var pixels = readable.GetPixels32();
                    var nonzero = 0;
                    for (var y = 0; y < readable.height; y++)
                    for (var x = 0; x < readable.width; x++)
                    {
                        if (pixels[y * readable.width + x].a == 0) continue;
                        nonzero++;
                        var p = new Vector2((x + .5f) / readable.width * 2f - 1f,
                            (y + .5f) / readable.height * 2f - 1f);
                        Assert.Less(p.magnitude, 1f);
                        Assert.Greater(p.magnitude, .4f, "A pressure wave never becomes a filled disc.");
                        if (arc < 360f) Assert.Less(Mathf.Abs(Mathf.Atan2(p.y, p.x)) * Mathf.Rad2Deg, arc * .5f);
                    }
                    Assert.Greater(nonzero, 100);
                    if (tail)
                    {
                        Assert.Greater(readable.GetPixelBilinear(.96f, .5f).a,
                            readable.GetPixelBilinear(.91f, .5f).a, "Tail grows denser toward the front.");
                    }
                }
                finally
                {
                    Object.DestroyImmediate(sprite.texture);
                    Object.DestroyImmediate(sprite);
                }
            }
        }

        [Test]
        public void ProductionProfiles_SeparateWarmClapFromCoolWave()
        {
            var profiles = SkillWorldEffectCatalog.Create();
            var ring = profiles[new ContentId("SKILL-004")];
            var clap = profiles[new ContentId("SET-022-ATTACK")];
            Assert.Greater(ring.Color.g, ring.Color.r);
            Assert.Greater(clap.Color.r, clap.Color.b);
            Assert.Greater(ring.TailFadePower, 1f);
            Assert.Greater(clap.TailFadePower, 1f);
            Assert.AreEqual(5, clap.AccentCount);
        }

    }
}
