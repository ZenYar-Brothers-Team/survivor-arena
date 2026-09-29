using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace Game.Enemy.Tests
{
    /// <summary>Development-only comparison of ENEMY-001 movement using the real controller and Physics2D.</summary>
    public sealed class EnemyBlobSweepTests
    {
        private const int EnemyCount = 49;
        private const float StepSeconds = 0.05f;
        private const int Steps = 320;

        [Test]
        public void MovementMixes_CirclingPlayer_ReportCrowdingAndContact()
        {
            var definition = ProductionEnemyCatalog.Create().Single(enemy => enemy.Id.ToString() == "ENEMY-001");
            // Keep the pre-tune profiles fixed so a production switch cannot rewrite the comparison.
            var standard = new[]
            {
                EnemyMovementProfile.Seek,
                new EnemyMovementProfile(EnemyMovementKind.OffsetPursuit, preferredDistance: 2f,
                    distanceTolerance: .25f, cycleSeconds: 3f, directPursuitSeconds: 1.5f),
                new EnemyMovementProfile(EnemyMovementKind.CommittedPursuit, cycleSeconds: 4f),
                new EnemyMovementProfile(EnemyMovementKind.BlockedSidestep, preferredDistance: .5f,
                    lateralStrength: 3f, blockedTriggerSeconds: .25f, blockedProgressFraction: .95f,
                    sidestepSeconds: 1.8f, sidestepCooldownSeconds: 1.2f,
                    sidestepNearDistance: 2.2f, sidestepNearSeconds: 1.2f),
                new EnemyMovementProfile(EnemyMovementKind.ArcPassPursuit, preferredDistance: 3f,
                    lateralStrength: 1.2f, cycleSeconds: 2.2f, directPursuitSeconds: .8f),
                new EnemyMovementProfile(EnemyMovementKind.InertialPursuit, turnResponseSeconds: 1.2f)
            };
            var boosted = new[]
            {
                standard[0],
                new EnemyMovementProfile(EnemyMovementKind.OffsetPursuit, preferredDistance: 3.5f,
                    distanceTolerance: .25f, cycleSeconds: 3f, directPursuitSeconds: 1.2f),
                new EnemyMovementProfile(EnemyMovementKind.CommittedPursuit, cycleSeconds: 6f),
                new EnemyMovementProfile(EnemyMovementKind.BlockedSidestep, preferredDistance: .5f,
                    lateralStrength: 4f, blockedTriggerSeconds: .2f, blockedProgressFraction: .95f,
                    sidestepSeconds: 2.4f, sidestepCooldownSeconds: 1f,
                    sidestepNearDistance: 3f, sidestepNearSeconds: .8f),
                new EnemyMovementProfile(EnemyMovementKind.ArcPassPursuit, preferredDistance: 4f,
                    lateralStrength: 2f, cycleSeconds: 2.5f, directPursuitSeconds: .6f),
                new EnemyMovementProfile(EnemyMovementKind.InertialPursuit, turnResponseSeconds: 2f)
            };
            var candidates = new List<(string label, EnemyMovementProfile[] profiles, float[] weights)>();
            var names = new[] { "seek", "offset", "committed", "blocked", "arc", "inertial" };
            for (var i = 0; i < names.Length; i++)
            {
                var pure = new float[names.Length];
                pure[i] = 1f;
                candidates.Add((names[i], standard, pure));
                if (i > 0) candidates.Add((names[i] + "-boosted", boosted, pure));
            }
            candidates.Add(("previous-40-20-40", standard, new[] { 0f, 0f, 0f, .4f, .2f, .4f }));
            candidates.Add(("equal-six", standard, new[] { 1f / 6, 1f / 6, 1f / 6, 1f / 6, 1f / 6, 1f / 6 }));
            candidates.Add(("equal-six-boosted", boosted, new[] { 1f / 6, 1f / 6, 1f / 6, 1f / 6, 1f / 6, 1f / 6 }));
            candidates.Add(("blocked-heavy-50", boosted, new[] { .1f, .1f, .1f, .5f, .1f, .1f }));
            candidates.Add(("blocked-heavy-60", boosted, new[] { .05f, .05f, .05f, .6f, .15f, .1f }));
            candidates.Add(("blocked-heavy-75", boosted, new[] { .05f, .05f, .05f, .75f, .05f, .05f }));
            candidates.Add(("blocked-arc-80-20", boosted, new[] { 0f, 0f, 0f, .8f, .2f, 0f }));
            var mixtureRandom = new System.Random(20260929);
            for (var i = 0; i < 20; i++)
            {
                var weights = new float[names.Length];
                var total = 0f;
                for (var j = 0; j < weights.Length; j++)
                {
                    weights[j] = (float)mixtureRandom.NextDouble();
                    total += weights[j];
                }
                for (var j = 0; j < weights.Length; j++) weights[j] /= total;
                candidates.Add(($"mix-{i:00}", i % 2 == 0 ? standard : boosted, weights));
            }

            var oldMode = Physics2D.simulationMode;
            var output = new StringBuilder("scenario,seed,candidate,seek,offset,committed,blocked,arc,inertial,crowdedFraction,largestClusterFraction,contactFraction,meanDistance\n");
            try
            {
                Physics2D.simulationMode = SimulationMode2D.Script;
                foreach (var scenario in new[] { "dense", "ring" })
                {
                    for (var seed = 11; seed <= 13; seed++)
                        foreach (var candidate in candidates)
                            Run(definition, seed, scenario, candidate.label, candidate.profiles,
                                candidate.weights, output);
                    foreach (var candidate in candidates.Where(item =>
                                 item.label == "previous-40-20-40" || item.label == "blocked-boosted" ||
                                 item.label == "mix-19" || item.label == "mix-03" ||
                                 item.label == "equal-six-boosted" || item.label.StartsWith("blocked-heavy-") ||
                                 item.label == "blocked-arc-80-20"))
                        for (var seed = 14; seed <= 30; seed++)
                            Run(definition, seed, scenario, candidate.label, candidate.profiles,
                                candidate.weights, output);
                }

                var path = Path.GetFullPath(Path.Combine(Application.dataPath, "../TestResults/anti-blob-sweep.csv"));
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, output.ToString());
                Debug.Log($"Anti-blob sweep: {candidates.Count} candidates, 2 scenarios, 3 seeds; shortlisted candidates extended to 20 seeds. CSV: {path}");
                Assert.Greater(output.Length, 500);
            }
            finally
            {
                Physics2D.simulationMode = oldMode;
            }
        }

        private static void Run(EnemyDefinition definition, int seed, string scenario, string label,
            EnemyMovementProfile[] profiles, float[] weights, StringBuilder output)
        {
            var random = new System.Random(seed);
            var spawnRandom = new System.Random(seed * 31);
            var objects = new GameObject[EnemyCount + 1];
            var bodies = new Rigidbody2D[EnemyCount];
            var controllers = new EnemyMovementController[EnemyCount];
            var crowded = 0f;
            var largest = 0f;
            var contact = 0f;
            var distance = 0f;
            var sampleCount = 0;
            try
            {
                var player = new GameObject("BlobSweepPlayer");
                objects[EnemyCount] = player;
                var target = player.AddComponent<Rigidbody2D>();
                target.bodyType = RigidbodyType2D.Kinematic;
                target.position = new Vector2(3.2f, 0f);
                player.AddComponent<CircleCollider2D>().radius = .4f;

                for (var i = 0; i < EnemyCount; i++)
                {
                    var obj = new GameObject("BlobSweepEnemy");
                    objects[i] = obj;
                    var body = obj.AddComponent<Rigidbody2D>();
                    body.gravityScale = 0f;
                    body.constraints = RigidbodyConstraints2D.FreezeRotation;
                    body.position = new Vector2(-7f + i % 7 * .95f, -3f + i / 7 * .95f);
                    body.simulated = scenario == "dense";
                    obj.AddComponent<CircleCollider2D>().radius = definition.CollisionSize * .5f;
                    bodies[i] = body;
                    var roll = (float)random.NextDouble();
                    var kindIndex = weights.Length - 1;
                    for (var j = 0; j < weights.Length; j++)
                    {
                        roll -= weights[j];
                        if (roll >= 0f) continue;
                        kindIndex = j;
                        break;
                    }
                    var profile = profiles[kindIndex];
                    controllers[i] = new EnemyMovementController(profile, new System.Random(random.Next()));
                }
                Physics2D.SyncTransforms();

                for (var step = 0; step < Steps; step++)
                {
                    var time = step * StepSeconds;
                    var angle = time * .65f;
                    target.position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 3.2f;
                    if (scenario == "ring")
                        for (var i = 0; i < EnemyCount; i++)
                            if (step == i * 2)
                            {
                                var spawnAngle = (float)(spawnRandom.NextDouble() * Math.PI * 2d);
                                bodies[i].position = target.position +
                                    new Vector2(Mathf.Cos(spawnAngle), Mathf.Sin(spawnAngle)) * 6.5f;
                                bodies[i].simulated = true;
                            }
                    for (var i = 0; i < EnemyCount; i++)
                    {
                        if (!bodies[i].simulated) continue;
                        bodies[i].linearVelocity = controllers[i].Tick(bodies[i].position, target.position,
                            definition.MovementSpeed, StepSeconds, true).Velocity;
                    }
                    Assert.IsTrue(Physics2D.Simulate(StepSeconds));
                    if (time < 5f || step % 10 != 0) continue;
                    Sample(bodies, target.position, ref crowded, ref largest, ref contact, ref distance);
                    sampleCount++;
                }
                Assert.Greater(sampleCount, 0);
                output.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
                    "{0},{1},{2},{3:0.000},{4:0.000},{5:0.000},{6:0.000},{7:0.000},{8:0.000},{9:0.000},{10:0.000},{11:0.000},{12:0.000}\n",
                    scenario, seed, label, weights[0], weights[1], weights[2], weights[3], weights[4], weights[5],
                    crowded / sampleCount, largest / sampleCount, contact / sampleCount, distance / sampleCount);
            }
            finally
            {
                foreach (var obj in objects)
                    if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }
        }

        private static void Sample(Rigidbody2D[] bodies, Vector2 player, ref float crowded,
            ref float largest, ref float contact, ref float meanDistance)
        {
            var parent = new int[bodies.Length];
            var neighbours = new int[bodies.Length];
            for (var i = 0; i < parent.Length; i++) parent[i] = i;
            for (var i = 0; i < bodies.Length; i++)
                for (var j = i + 1; j < bodies.Length; j++)
                    if ((bodies[i].position - bodies[j].position).sqrMagnitude <= 1.5f * 1.5f)
                    {
                        neighbours[i]++;
                        neighbours[j]++;
                        var a = Root(parent, i);
                        var b = Root(parent, j);
                        parent[a] = b;
                    }
            var localCrowded = 0;
            var localContact = 0;
            var localDistance = 0f;
            var componentSizes = new int[bodies.Length];
            for (var i = 0; i < bodies.Length; i++)
            {
                if (neighbours[i] >= 3) localCrowded++;
                var d = Vector2.Distance(bodies[i].position, player);
                if (d <= 1.2f) localContact++;
                localDistance += d;
                componentSizes[Root(parent, i)]++;
            }
            crowded += (float)localCrowded / bodies.Length;
            largest += (float)componentSizes.Max() / bodies.Length;
            contact += (float)localContact / bodies.Length;
            meanDistance += localDistance / bodies.Length;
        }

        private static int Root(int[] parent, int index)
        {
            while (parent[index] != index)
            {
                parent[index] = parent[parent[index]];
                index = parent[index];
            }
            return index;
        }
    }
}
