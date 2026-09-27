using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Game.Enemy.Tests
{
    /// <summary>bosses-v1 (DECISION-0066): BOSS-003…010 and MIDBOSS-003…010 production encounters.</summary>
    public sealed class ProductionLateBossCatalogTests
    {
        private static readonly Dictionary<string, BossEncounterDefinition> Catalog =
            ProductionBossCatalog.Create().ToDictionary(b => b.Id.ToString());

        // id → (HP, XP, phase count, signature kind); cards and packet formulas 150 + 30·(N−1), 60 + 15·(N−1).
        private static readonly Dictionary<string, (float hp, float xp, int phases, BossSpecialKind signature)> Expected =
            new Dictionary<string, (float, float, int, BossSpecialKind)>
            {
                ["BOSS-003"] = (7200, 210, 2, BossSpecialKind.Zone), ["BOSS-004"] = (8500, 240, 2, BossSpecialKind.Zone),
                ["BOSS-005"] = (10000, 270, 2, BossSpecialKind.Zone), ["BOSS-006"] = (12000, 300, 2, BossSpecialKind.Beam),
                ["BOSS-007"] = (14500, 330, 2, BossSpecialKind.Zone), ["BOSS-008"] = (17500, 360, 2, BossSpecialKind.Summon),
                ["BOSS-009"] = (21000, 390, 2, BossSpecialKind.Beam), ["BOSS-010"] = (26000, 420, 3, BossSpecialKind.Zone),
                ["MIDBOSS-003"] = (1900, 90, 1, BossSpecialKind.Summon), ["MIDBOSS-004"] = (2300, 105, 1, BossSpecialKind.Zone),
                ["MIDBOSS-005"] = (2800, 120, 1, BossSpecialKind.Summon), ["MIDBOSS-006"] = (3300, 135, 1, BossSpecialKind.Zone),
                ["MIDBOSS-007"] = (3900, 150, 1, BossSpecialKind.Zone), ["MIDBOSS-008"] = (4600, 165, 1, BossSpecialKind.Zone),
                ["MIDBOSS-009"] = (5400, 180, 1, BossSpecialKind.Beam), ["MIDBOSS-010"] = (6500, 195, 1, BossSpecialKind.Beam),
            };

        private static IEnumerable<BossSpecialKind> Signatures(BossEncounterDefinition boss) =>
            boss.Phases.SelectMany(p => p.Specials).Where(s => s != null).Select(s => s.Kind)
                .Concat(boss.Body.DashVolley?.Entries.Where(e => e.Zone != null).Select(_ => BossSpecialKind.Zone) ??
                        Enumerable.Empty<BossSpecialKind>());

        [Test]
        public void Catalog_DefinesAllSixteen_WithCardBodiesPacketXpAndOneSignatureFamily()
        {
            foreach (var pair in Expected)
            {
                var boss = Catalog[pair.Key];
                var final = pair.Key.StartsWith("BOSS");
                Assert.AreEqual(final ? WaveHookKind.FinalBoss : WaveHookKind.MidBoss, boss.Hook, pair.Key);
                Assert.AreEqual(pair.Value.hp, boss.Body.MaxHealth, pair.Key);
                Assert.AreEqual(pair.Value.xp, boss.Body.ExperienceReward, pair.Key);
                Assert.AreEqual(pair.Value.phases, boss.Phases.Count, pair.Key);
                Assert.IsTrue(boss.StrictHealthThreshold && boss.KeepAttackOrderOnPhaseChange, pair.Key);
                Assert.AreEqual(final, boss.Teleport != null, $"{pair.Key}: every final boss teleports, no mid-boss does.");
                CollectionAssert.Contains(Signatures(boss).ToList(), pair.Value.signature, pair.Key);
            }
        }

        [Test]
        public void FinalBoss_TeleportImpactGrowsTwoPerField_AndStaysUnavoidable()
        {
            for (var n = 3; n <= 10; n++)
            {
                var teleport = Catalog[$"BOSS-{n:000}"].Teleport;
                Assert.AreEqual(20 + 2 * (n - 2), teleport.ImpactDamage, 1e-5f);
                Assert.Less(teleport.LandingDistance + 3f * teleport.TelegraphSeconds, teleport.ImpactRadius);
            }
        }

        [Test]
        public void Signatures_MatchThePacket()
        {
            var traps = Catalog["BOSS-003"].Phases[1].Specials.Single(s => s != null).Zone;
            Assert.AreEqual(BossZonePlacement.AroundPlayer, traps.Placement);
            Assert.AreEqual(5, traps.Count, "Five traps in the enraged phase.");
            Assert.AreEqual(26f, traps.Damage);
            Assert.AreEqual(0.5f, traps.Controls.SlowFraction, 1e-5f);

            var bonfires = Catalog["BOSS-005"].Phases[0].Specials.Single(s => s != null).Zone;
            Assert.AreEqual(BossZonePlacement.Trail, bonfires.Placement);
            Assert.AreEqual(4, bonfires.Count);
            Assert.AreEqual(13f, bonfires.LingerDamagePerSecond);

            var sweep = Catalog["BOSS-006"].Phases[1].Specials.Single(s => s != null).Beam;
            Assert.AreEqual(60f, sweep.SweepDegrees);
            Assert.AreEqual(36f, sweep.Damage);

            var duelists = Catalog["BOSS-008"].Phases[0].Specials.Single(s => s != null).Summon;
            Assert.AreEqual("ENEMY-017", duelists.Enemy.Id.ToString());
            Assert.AreEqual(4, duelists.Count);
            Assert.AreEqual(8, duelists.MaxAlive);

            var spears = Catalog["BOSS-009"].Phases[0].Specials.Single(s => s != null).Beam;
            CollectionAssert.AreEqual(new[] { -30f, 0f, 30f }, spears.AnglesDegrees.ToArray());

            var judgement = Catalog["BOSS-010"].Phases[0].Specials.Single(s => s != null).Zone;
            Assert.AreEqual(BossZonePlacement.SafeCircles, judgement.Placement);
            Assert.AreEqual(50f, judgement.Damage);
            Assert.LessOrEqual(judgement.ScatterRadius - judgement.Radius, 3f * judgement.FillSeconds * .5f,
                "A base-speed player reaches a safe circle in half the fill time.");

            Assert.AreEqual("ENEMY-007", Catalog["MIDBOSS-003"].Phases[0].Specials.Single(s => s != null).Summon.Enemy.Id.ToString());
            var shields = Catalog["MIDBOSS-005"].Phases[0];
            Assert.AreEqual(3, shields.Attacks.Count, "Cross, cross, shield-bearers.");
            Assert.AreEqual("ENEMY-009", shields.Specials[2].Summon.Enemy.Id.ToString());
            CollectionAssert.AreEqual(new[] { 0f, 90f, 180f, 270f },
                Catalog["MIDBOSS-010"].Phases[0].Specials.Single(s => s != null).Beam.AnglesDegrees.ToArray());
        }

        [Test]
        public void Extensions_E1ToE6_AreWiredFromData()
        {
            var ring = Catalog["BOSS-010"].Phases[1].Attacks.Single(a => a.Id.ToString() == "BOSS-010-P2-RING16").Attack;
            Assert.AreEqual(3, ring.FollowUps.Count, "E1: double ring plus its 0.4 s repeat.");
            var fan = Catalog["BOSS-007"].Body.DashVolley.ReplacementEntries.Single().Attack;
            Assert.AreEqual(EnemyProjectilePattern.Explosive, fan.Pattern);
            Assert.AreEqual(3, fan.ProjectileCount, "E2: explosive fan below 45 %.");
            Assert.AreEqual(0.45f, Catalog["BOSS-007"].Body.DashVolley.ReplacementBelowHealthFraction, 1e-5f);
            Assert.AreEqual(2, Catalog["BOSS-004"].Phases[1].MovementOverride.DashCount, "E3: two dashes below 50 %.");
            Assert.AreEqual(2, Catalog["BOSS-009"].Phases[1].MovementOverride.DashCount);
            var rear = Catalog["MIDBOSS-007"].Body.DashVolley.Entries;
            Assert.AreEqual(EnemyDashVolleyOrientation.AwayFromDash, rear[0].Orientation, "E4: rear fan.");
            Assert.AreEqual(BossZonePlacement.AroundSelf, rear[1].Zone.Placement);
            Assert.AreEqual(0.4f, Catalog["MIDBOSS-010"].Body.DashVolley.Entries[1].DelaySeconds, 1e-5f, "E4: cross 0.4 s after the ring.");
            Assert.IsTrue(Catalog["BOSS-009"].HoldAttacksDuringDash, "E5.");
            Assert.IsTrue(Catalog["BOSS-007"].HoldAttacksDuringDash);
            Assert.IsFalse(Catalog["BOSS-005"].HoldAttacksDuringDash);
            Assert.IsTrue(Catalog["MIDBOSS-009"].Phases[0].Attacks.Where(a => a.Attack != null)
                .All(a => a.Attack.WindupMovementMultiplier == 0.4f), "E6.");
            Assert.AreEqual(3, Catalog["MIDBOSS-004"].Body.Movement.DashCount, "Three short dashes per series.");
        }

        [Test]
        public void EveryProjectileStep_UsesTheStartToStartCadenceAndHasAWarning()
        {
            foreach (var boss in Expected.Keys.Select(id => Catalog[id]))
                foreach (var phase in boss.Phases)
                    foreach (var step in phase.Attacks.Where(a => a.Attack != null))
                    {
                        Assert.AreEqual(EnemyAttackCadence.WindupStartToStart, step.Attack.Cadence, step.Id.ToString());
                        Assert.Greater(step.Attack.TelegraphSeconds, 0f, step.Id.ToString());
                    }
        }
    }
}
