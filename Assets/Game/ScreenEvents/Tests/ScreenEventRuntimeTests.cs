using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Game.ScreenEvents.Tests
{
    public sealed class ScreenEventRuntimeTests
    {
        private static readonly Rect View = ScreenEventTestData.View;

        private static ScreenEventRuntime Create(FakeScreenEventPlayer player, int seed = 1, ScreenEventDefinitionSet set = null)
            => new ScreenEventRuntime(ScreenEventTestData.Definition(set?.Events, set?.Stages), player, seed);

        private sealed class ScreenEventDefinitionSet
        {
            public Game.ScreenEvents.Json.ScreenEventData[] Events;
            public Game.ScreenEvents.Json.ScreenEventStageData[] Stages;
        }

        private static void Run(ScreenEventRuntime runtime, float seconds, bool suspended = false, float step = .05f)
        {
            for (var t = 0f; t < seconds; t += step) runtime.Tick(step, View, suspended);
        }

        [Test]
        public void StrikeStarts_LargeTickCrossesAllBoundaries_OnceEachWithoutPlayerContact()
        {
            var player = new FakeScreenEventPlayer { Position = new Vector2(100f, 100f) };
            var runtime = Create(player);
            var starts = new List<ScreenHazard>();
            runtime.HazardStrikeStarted += (_, hazard) => starts.Add(hazard);
            Assert.IsTrue(runtime.TryStart("T-PILLARS", View));
            var hazards = runtime.Active.Hazards.ToArray();
            runtime.Tick(0f, View, false);
            Assert.IsEmpty(starts, "Pause cannot emit a sound.");
            runtime.Tick(runtime.Active.Duration + .1f, View, false);
            CollectionAssert.AreEquivalent(hazards, starts);
            Assert.AreEqual(starts.Count, starts.Distinct().Count());
            Assert.AreEqual(0, runtime.PlayerHitCount, "Strike sounds do not require player contact.");
            runtime.Tick(.1f, View, false);
            Assert.AreEqual(hazards.Length, starts.Count);
        }

        [Test]
        public void Tick_FirstEventStartsAfterTheFirstDelay_NotBefore()
        {
            var runtime = Create(new FakeScreenEventPlayer());
            Run(runtime, 4.8f);
            Assert.IsNull(runtime.Active, "The first delay is five seconds.");
            Run(runtime, .4f);
            Assert.IsNotNull(runtime.Active);
        }

        [Test]
        public void Tick_ZeroDelta_AdvancesNothing_LikePause()
        {
            var runtime = Create(new FakeScreenEventPlayer());
            for (var i = 0; i < 1000; i++) runtime.Tick(0f, View, false);
            Assert.AreEqual(0f, runtime.Elapsed);
            Assert.IsNull(runtime.Active);
            Assert.AreEqual(5f, runtime.SecondsUntilNext);
        }

        [Test]
        public void Tick_OnlyOneEventRunsAtATime_AndThePauseStartsWhenItEnds()
        {
            var runtime = Create(new FakeScreenEventPlayer { Position = new Vector2(100, 100) });
            var started = 0;
            var finished = 0;
            var overlapped = false;
            runtime.EventStarted += _ => { started++; if (started - finished > 1) overlapped = true; };
            runtime.EventFinished += _ => finished++;
            Run(runtime, 400f);
            Assert.IsFalse(overlapped);
            Assert.GreaterOrEqual(finished, 8);
            Assert.LessOrEqual(started - finished, 1);
        }

        [Test]
        public void Tick_PauseBetweenEventsShrinksAsIntensityRises()
        {
            // The pause comes from the stage as Lerp(max, min, intensity): a calm valley waits long, a peak waits short.
            var player = new FakeScreenEventPlayer { Position = new Vector2(100, 100) };
            var calm = new List<float>();
            var busy = new List<float>();
            for (var seed = 0; seed < 25; seed++)
            {
                var runtime = Create(player, seed);
                runtime.EventFinished += _ =>
                {
                    var phase = runtime.Elapsed % 100f;
                    if (phase < 15f || phase > 85f) calm.Add(runtime.SecondsUntilNext);
                    else if (phase > 35f && phase < 65f) busy.Add(runtime.SecondsUntilNext);
                };
                Run(runtime, 290f, step: .1f);
            }
            Assert.IsNotEmpty(calm);
            Assert.IsNotEmpty(busy);
            Assert.Greater(calm.Average(), busy.Average() + 1.5f);
        }

        [Test]
        public void Tick_HeavyEventsClusterNearPeaks_AndLightOnesNearValleys()
        {
            var player = new FakeScreenEventPlayer { Position = new Vector2(100, 100) };
            var atValley = new List<float>();
            var atPeak = new List<float>();
            for (var seed = 0; seed < 25; seed++)
            {
                var runtime = Create(player, seed);
                runtime.EventStarted += instance =>
                {
                    var intensity = runtime.Intensity;
                    if (intensity < .25f) atValley.Add(instance.Definition.Intensity);
                    else if (intensity > .4f) atPeak.Add(instance.Definition.Intensity);
                };
                Run(runtime, 290f, step: .1f);
            }
            Assert.IsNotEmpty(atValley);
            Assert.IsNotEmpty(atPeak);
            Assert.Greater(atPeak.Average(), atValley.Average() + .05f);
        }

        [Test]
        public void Tick_RareEventsOnlyStartHighOnTheWave_AndRespectTheMinimumInterval()
        {
            var player = new FakeScreenEventPlayer { Position = new Vector2(100, 100) };
            var rareStarts = new List<(float time, float intensity)>();
            for (var seed = 0; seed < 12; seed++)
            {
                var runtime = Create(player, seed);
                var previous = float.NegativeInfinity;
                runtime.EventStarted += instance =>
                {
                    if (!instance.Definition.Rare) return;
                    rareStarts.Add((runtime.Elapsed, runtime.Intensity));
                    Assert.GreaterOrEqual(runtime.Elapsed - previous, 100f, "The minimum interval counts from the end of the last rare event.");
                };
                runtime.EventFinished += instance => { if (instance.Definition.Rare) previous = runtime.Elapsed; };
                Run(runtime, 900f, step: .1f);
            }
            Assert.IsNotEmpty(rareStarts, "Rare events do appear.");
            foreach (var start in rareStarts) Assert.GreaterOrEqual(start.intensity, .8f - 1e-3f);
        }

        [Test]
        public void Tick_WhileSuspended_NoNewEventStartsAndTheTimerWaits()
        {
            var runtime = Create(new FakeScreenEventPlayer());
            Run(runtime, 30f, suspended: true);
            Assert.IsNull(runtime.Active);
            Assert.AreEqual(5f, runtime.SecondsUntilNext, 1e-3f, "The timer does not run while a boss is alive.");
            Run(runtime, 6f);
            Assert.IsNotNull(runtime.Active, "It resumes afterwards.");
        }

        [Test]
        public void Tick_SuspendingDuringAnEvent_LetsThatEventFinish()
        {
            var runtime = Create(new FakeScreenEventPlayer { Position = new Vector2(100, 100) });
            Run(runtime, 5.5f);
            var instance = runtime.Active;
            Assert.IsNotNull(instance);
            Run(runtime, instance.Duration + 1f, suspended: true);
            Assert.IsNull(runtime.Active);
        }

        [Test]
        public void Tick_DeadPlayer_StartsNothing()
        {
            var runtime = Create(new FakeScreenEventPlayer { IsAlive = false });
            Run(runtime, 60f);
            Assert.IsNull(runtime.Active);
        }

        [Test]
        public void Hazard_HurtsAPlayerStandingInItExactlyOnce_ByItsShareOfMaxHealth()
        {
            var player = new FakeScreenEventPlayer();
            var events = new[] { ScreenEventTestData.Half() };
            var stages = new[] { ScreenEventTestData.Stage(0, .1f, .5f, 10, 20, ("T-HALF", 1f)) };
            var set = new ScreenEventDefinitionSet { Events = events, Stages = stages };
            var hurt = 0;
            for (var seed = 0; seed < 30 && hurt == 0; seed++)
            {
                player.Hits.Clear();
                var runtime = Create(player, seed, set);
                // A player who does not move: the half either covers them or not; whenever it does the hit happens once.
                Run(runtime, 5f);
                var instance = runtime.Active;
                if (instance == null) continue;
                Run(runtime, instance.Duration + 1f);
                Assert.LessOrEqual(player.Hits.Count, 1);
                if (player.Hits.Count == 1)
                {
                    hurt++;
                    Assert.AreEqual(.22f, player.Hits[0].fraction, 1e-5f);
                    Assert.AreEqual("T-HALF", player.Hits[0].source);
                    Assert.AreEqual(1, runtime.PlayerHitCount);
                }
            }
            Assert.Greater(hurt, 0, "A standing player is hurt by some placement within 30 seeds.");
        }

        [Test]
        public void TryStart_StartsTheNamedEventNow_UnlessOneIsRunning()
        {
            var runtime = Create(new FakeScreenEventPlayer { Position = new Vector2(100, 100) });
            Assert.IsFalse(runtime.TryStart("T-NOPE", View));
            Assert.IsTrue(runtime.TryStart("T-RING", View));
            Assert.AreEqual("T-RING", runtime.Active.Definition.Id.ToString());
            Assert.IsFalse(runtime.TryStart("T-SPEAR", View), "One event at a time.");
        }

        [Test]
        public void TryQueue_StartsTheEventAfterTheDelay_AndNoScheduledEventStartsBefore()
        {
            var runtime = Create(new FakeScreenEventPlayer { Position = new Vector2(100, 100) });
            Assert.IsTrue(runtime.TryQueue("T-RING", 2f));
            Assert.AreEqual("T-RING", runtime.QueuedEventId);
            Run(runtime, 1.9f);
            Assert.IsNull(runtime.Active, "Not before the delay.");
            Assert.AreEqual(.1f, runtime.QueuedInSeconds, .06f);
            Run(runtime, .3f);
            Assert.AreEqual("T-RING", runtime.Active.Definition.Id.ToString());
            Assert.IsNull(runtime.QueuedEventId);
        }

        [Test]
        public void TryQueue_BlocksTheScheduleWhileWaiting()
        {
            var runtime = Create(new FakeScreenEventPlayer { Position = new Vector2(100, 100) });
            Assert.IsTrue(runtime.TryQueue("T-CROSS", 30f));
            Run(runtime, 29f);
            Assert.IsNull(runtime.Active, "A scheduled event must not jump ahead of a queued one.");
            Run(runtime, 1.5f);
            Assert.AreEqual("T-CROSS", runtime.Active.Definition.Id.ToString());
        }

        [Test]
        public void TryQueue_PauseAdvancesNothing()
        {
            var runtime = Create(new FakeScreenEventPlayer { Position = new Vector2(100, 100) });
            runtime.TryQueue("T-RING", 2f);
            for (var i = 0; i < 500; i++) runtime.Tick(0f, View, false);
            Assert.AreEqual(2f, runtime.QueuedInSeconds);
        }

        [Test]
        public void TryQueue_RejectsUnknownEventsAndAWaitingOrRunningOne()
        {
            var runtime = Create(new FakeScreenEventPlayer { Position = new Vector2(100, 100) });
            Assert.IsFalse(runtime.TryQueue("T-NOPE", 1f));
            Assert.IsTrue(runtime.TryQueue("T-RING", 1f));
            Assert.IsFalse(runtime.TryQueue("T-CROSS", 1f), "One waiting event at a time.");
            Run(runtime, 1.2f);
            Assert.IsNotNull(runtime.Active);
            Assert.IsFalse(runtime.TryQueue("T-CROSS", 1f), "One running event at a time.");
        }

        [Test]
        public void Clear_AlsoDropsAQueuedEvent()
        {
            var runtime = Create(new FakeScreenEventPlayer { Position = new Vector2(100, 100) });
            runtime.TryQueue("T-RING", 5f);
            runtime.Clear();
            Assert.IsNull(runtime.QueuedEventId);
        }

        [Test]
        public void Clear_DropsTheRunningEvent()
        {
            var runtime = Create(new FakeScreenEventPlayer { Position = new Vector2(100, 100) });
            runtime.TryStart("T-CROSS", View);
            runtime.Clear();
            Assert.IsNull(runtime.Active);
        }

        [Test]
        public void SameSeed_ReplaysTheSameSchedule()
        {
            var first = new List<string>();
            var second = new List<string>();
            foreach (var log in new[] { first, second })
            {
                var runtime = Create(new FakeScreenEventPlayer { Position = new Vector2(100, 100) }, 42);
                runtime.EventStarted += instance => log.Add($"{runtime.Elapsed:F1}:{instance.Definition.Id}");
                Run(runtime, 300f);
            }
            CollectionAssert.AreEqual(first, second);
            Assert.IsNotEmpty(first);
        }

        [Test]
        public void EveryEvent_IsFairAtEveryTestedPlayerPosition_WithinTheAttemptBudget()
        {
            var player = new FakeScreenEventPlayer();
            var unfair = new List<string>();
            foreach (var data in ScreenEventTestData.AllLayouts())
            {
                var set = new ScreenEventDefinitionSet
                {
                    Events = new[] { data, ScreenEventTestData.Spear("T-FILLER") },
                    Stages = new[] { ScreenEventTestData.Stage(0, .1f, .5f, 10, 20, (data.Id, 1f), ("T-FILLER", 1f)) }
                };
                foreach (var position in ScreenEventTestData.PlayerGrid(View, 6, 4))
                {
                    player.Position = position;
                    var runtime = Create(player, 5, set);
                    runtime.TryStart(data.Id, View);
                    if (runtime.UnfairStartCount > 0) unfair.Add($"{data.Id}@{position}");
                }
            }
            Assert.IsEmpty(unfair, string.Join(", ", unfair));
        }
    }
}
