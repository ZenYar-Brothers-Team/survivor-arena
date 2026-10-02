using Game.Zones.Json;
using NUnit.Framework;
using UnityEngine;

namespace Game.Zones.Tests
{
    /// <summary>Remaining fractions behind the player's duration bars for shrine rewards that outlast the zone.</summary>
    public sealed class TimedEffectFractionTests
    {
        private const float Dt = .1f;
        private static readonly Rect View = new Rect(-20f, -12f, 40f, 24f);

        private static ZoneRuntime Build(ZoneEffectData data, FakeZonePlayer player)
        {
            var layout = new ZoneLayoutDefinition(ZoneTestData.Layout(new[] { data }, (data.Id, 1)));
            var zone = new ZonePlacement(0, layout.Effects[new Game.Content.ContentId(data.Id)], Vector2.zero);
            return new ZoneRuntime(new[] { zone }, new ZonePlacementRules(layout, 120f, Vector2.zero, null), 1, player,
                new FakeZoneEnemies(), () => View);
        }

        private static void Run(ZoneRuntime runtime, float seconds)
        {
            for (var t = 0f; t < seconds - 1e-4f; t += Dt) runtime.Tick(Dt);
        }

        [Test]
        public void ExperienceBuff_FractionStartsFullAfterFiring_DrainsToZero_AndNeverShowsOtherBars()
        {
            var player = new FakeZonePlayer { Position = new Vector2(1f, 0f) };
            using var runtime = Build(ZoneTestData.ExperienceShrine(), player);
            Assert.AreEqual(0f, runtime.ExperienceBuffRemaining01);
            Run(runtime, 4.3f);
            Assert.AreEqual(1f, runtime.ExperienceBuffRemaining01, .02f);
            player.Position = new Vector2(40f, 0f);
            Run(runtime, 15f);
            Assert.AreEqual(.5f, runtime.ExperienceBuffRemaining01, .03f, "Linear drain over the 30 s reward.");
            Assert.AreEqual(0f, runtime.ShieldRemaining01); Assert.AreEqual(0f, runtime.PowerBuffRemaining01);
            Run(runtime, 16f);
            Assert.AreEqual(0f, runtime.ExperienceBuffRemaining01);
        }

        [Test]
        public void ShieldAndSpeedRewards_ShowTheirOwnFractions_AndMovementOnlyBuffHasNoPowerBar()
        {
            var player = new FakeZonePlayer { Position = new Vector2(1f, 0f) };
            using var runtime = Build(ZoneTestData.Shrine(), player);
            Run(runtime, 10.3f);
            Assert.AreEqual(1f, runtime.ShieldRemaining01, .1f, "The 5 s shield was just granted.");
            Assert.Greater(runtime.SpeedBuffRemaining01, .5f);
            Assert.AreEqual(0f, runtime.PowerBuffRemaining01, "The shrine buff is movement only.");
            player.Position = new Vector2(40f, 0f);
            Run(runtime, 3f);
            Assert.That(runtime.ShieldRemaining01, Is.InRange(.2f, .5f));
            Run(runtime, 3f);
            Assert.AreEqual(0f, runtime.ShieldRemaining01);
        }

        [Test]
        public void PowerBuff_SkillOrActionRewardHasItsOwnBar_AndDeathClearsEveryBar()
        {
            var data = ZoneTestData.Shrine();
            data.RewardMovementBonus = null; data.RewardSkillDamageBonus = .5f;
            var player = new FakeZonePlayer { Position = new Vector2(1f, 0f) };
            using var runtime = Build(data, player);
            Run(runtime, 10.3f);
            Assert.Greater(runtime.PowerBuffRemaining01, .5f);
            Assert.AreEqual(0f, runtime.SpeedBuffRemaining01, "No movement bonus, so no speed bar.");
            Assert.Greater(runtime.ShieldRemaining01, 0f);
            player.IsAlive = false;
            runtime.Tick(Dt);
            Assert.AreEqual(0f, runtime.PowerBuffRemaining01); Assert.AreEqual(0f, runtime.ShieldRemaining01);
        }
    }
}
