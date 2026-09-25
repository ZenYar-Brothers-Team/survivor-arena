using System.Collections.Generic;
using System.Linq;
using Game.Content.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Game.Enemy.Tests
{
    /// <summary>Art Production rule (DECISION-0055, playtest 2026-09-24_e1e04fc4 OBS-06): hostile projectiles stay readable.</summary>
    public sealed class HostileProjectileReadabilityTests
    {
        private static readonly string[] HostileContent =
        {
            "Content/Enemies/ProductionEnemies", "Content/Enemies/FixtureEnemies",
            "Content/Bosses/ProductionBosses", "Content/Bosses/FixtureBosses",
            "Content/Travelers/ProductionTravelers"
        };

        [Test]
        public void HostileProjectiles_AllHaveThreatHalo()
        {
            var hostileVisuals = new HashSet<string>();
            foreach (var path in HostileContent)
                foreach (var token in JToken.Parse(JsonContentFile.ReadText(path)).SelectTokens("..projectileVisualId"))
                    if (!string.IsNullOrEmpty((string)token)) hostileVisuals.Add((string)token);
            Assert.IsNotEmpty(hostileVisuals);

            var sprites = JArray.Parse(JsonContentFile.ReadText("Content/Presentation/FixtureSprites"))
                .ToDictionary(sprite => (string)sprite["id"]);
            foreach (var id in hostileVisuals)
            {
                Assert.IsTrue(sprites.TryGetValue(id, out var sprite), id);
                var halo = sprite["projectile"]?["threatHalo"];
                Assert.IsNotNull(halo, $"{id} needs a threat halo.");
                Assert.GreaterOrEqual((float)halo["scale"], 2.5f, id);
                Assert.Greater((float)halo["red"], (float)halo["green"], $"{id} halo is coral-red.");
            }
        }
    }
}
