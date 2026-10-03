using System.Collections.Generic;
using System.Linq;
using Game.ScreenEvents.Json;
using UnityEngine;

namespace Game.ScreenEvents.Tests
{
    /// <summary>Builders for screen-event content in tests; the numbers are test values, not the production packet.</summary>
    internal static class ScreenEventTestData
    {
        public static readonly Rect View = new Rect(-8.9f, -5f, 17.8f, 10f);

        public static ScreenEventData Spear(string id = "T-SPEAR") => new ScreenEventData
        {
            Id = id, Name = "Spear", Layout = ScreenEventLayout.Strips, Rare = false, Intensity = .2f, DamagePercent = 20,
            TelegraphSeconds = 1.3f, Motion = ScreenStripMotion.Sweep, DirectionsDegrees = new[] { 270f }, CountMin = 1, CountMax = 3,
            Ordered = true, StaggerSeconds = .5f, WidthH = .22f, MinGapH = .12f, SpeedH = 1.8f, BodyLengthH = .45f
        };

        public static ScreenEventData Pillars(string id = "T-PILLARS") => new ScreenEventData
        {
            Id = id, Name = "Pillars", Layout = ScreenEventLayout.Strips, Rare = false, Intensity = .5f, DamagePercent = 15,
            TelegraphSeconds = 1f, StrikeSeconds = .25f, Motion = ScreenStripMotion.Burst, DirectionsDegrees = new[] { 270f },
            CountMin = 5, CountMax = 5, Ordered = true, StaggerSeconds = .3f, WidthH = .13f, MinGapH = .14f
        };

        public static ScreenEventData Cross(string id = "T-CROSS") => new ScreenEventData
        {
            Id = id, Name = "Cross", Layout = ScreenEventLayout.Cross, Rare = false, Intensity = .45f, DamagePercent = 18,
            TelegraphSeconds = 1.4f, StrikeSeconds = .3f, WidthH = .17f, CrossJitter = .2f
        };

        public static ScreenEventData Circles(string id = "T-CIRCLES") => new ScreenEventData
        {
            Id = id, Name = "Circles", Layout = ScreenEventLayout.Circles, Rare = false, Intensity = .4f, DamagePercent = 20,
            TelegraphSeconds = 1.2f, StrikeSeconds = .3f, CountMin = 3, CountMax = 5, RadiusMinH = .12f, RadiusMaxH = .2f,
            Placement = ScreenCirclePlacement.AroundPlayer, PlayerDistanceMinH = .12f, PlayerDistanceMaxH = .4f,
            PlayerClearanceH = .08f, GapH = .05f, StaggerSeconds = 0
        };

        public static ScreenEventData Ring(string id = "T-RING") => new ScreenEventData
        {
            Id = id, Name = "Ring", Layout = ScreenEventLayout.Ring, Rare = false, Intensity = .55f, DamagePercent = 20,
            TelegraphSeconds = 2f, StartRadiusH = .1f, ThicknessH = .06f, GapWidthH = .26f, SpeedH = .3f
        };

        public static ScreenEventData Half(string id = "T-HALF") => new ScreenEventData
        {
            Id = id, Name = "Half", Layout = ScreenEventLayout.HalfScreen, Rare = false, Intensity = .35f, DamagePercent = 22,
            TelegraphSeconds = 2.2f, StrikeSeconds = .4f, SplitJitter = .15f
        };

        public static ScreenEventData Corners(string id = "T-CORNERS") => new ScreenEventData
        {
            Id = id, Name = "Corners", Layout = ScreenEventLayout.Corners, Rare = false, Intensity = .5f, DamagePercent = 20,
            TelegraphSeconds = 1.2f, StrikeSeconds = .3f, CornerWidthFraction = .36f, CornerHeightFraction = .36f, StaggerSeconds = .8f
        };

        public static ScreenEventData Fan(string id = "T-FAN") => new ScreenEventData
        {
            Id = id, Name = "Fan", Layout = ScreenEventLayout.Fan, Rare = false, Intensity = .45f, DamagePercent = 14,
            TelegraphSeconds = 1.3f, AnglesDegrees = new[] { -22f, 0f, 22f }, WidthH = .12f, SpeedH = 1.8f, BodyLengthH = .45f
        };

        public static ScreenEventData Judgment(string id = "T-JUDGMENT") => new ScreenEventData
        {
            Id = id, Name = "Judgment", Layout = ScreenEventLayout.Judgment, Rare = true, Intensity = .95f, DamagePercent = 28,
            TelegraphSeconds = 2.4f, StrikeSeconds = .4f, SafeRadiusH = .2f, PlayerDistanceMinH = .1f, PlayerDistanceMaxH = .3f
        };

        public static ScreenEventData[] AllLayouts() => new[]
        {
            Spear(), Pillars(), Cross(), Circles(), Ring(), Half(), Corners(), Fan(), Judgment()
        };

        public static ScreenEventStageData Stage(float from, float valley, float peak, float pauseMin, float pauseMax, params (string id, float weight)[] pool)
            => new ScreenEventStageData
            {
                FromSeconds = from, ValleyIntensity = valley, PeakIntensity = peak, PauseMinSeconds = pauseMin, PauseMaxSeconds = pauseMax,
                Pool = pool.Select(item => new ScreenEventPoolEntryData { Event = item.id, Weight = item.weight }).ToArray()
            };

        /// <summary>Everything the schedule needs; the stages cover every event of <paramref name="events"/> by default.</summary>
        public static ScreenEventsData Profile(ScreenEventData[] events = null, ScreenEventStageData[] stages = null)
        {
            events ??= AllLayouts();
            stages ??= new[]
            {
                Stage(0, .1f, .5f, 10, 20, events.Select(e => (e.Id, 1f)).ToArray()),
                Stage(300, .2f, 1f, 6, 12, events.Select(e => (e.Id, 1f)).ToArray())
            };
            return new ScreenEventsData
            {
                ReferenceSeed = 7, PlayerHitRadius = .35f, FirstDelaySeconds = 5, RareMinIntervalSeconds = 100, SuspendWhileBossAlive = true,
                WavePeriodSeconds = 100, IntensityTolerance = .3f, RareMinIntensity = .8f, FairnessSpeed = 1.9f, FairnessAttempts = 12,
                FairnessCellSize = .4f, Stages = stages, Events = events
            };
        }

        public static ScreenEventsDefinition Definition(ScreenEventData[] events = null, ScreenEventStageData[] stages = null)
            => new ScreenEventsDefinition(Profile(events, stages));

        public static IReadOnlyList<Vector2> PlayerGrid(Rect view, int columns, int rows)
        {
            var points = new List<Vector2>();
            for (var y = 0; y < rows; y++)
            for (var x = 0; x < columns; x++)
                points.Add(new Vector2(view.xMin + (x + .5f) / columns * view.width, view.yMin + (y + .5f) / rows * view.height));
            return points;
        }
    }
}
