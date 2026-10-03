using System;
using System.Collections.Generic;
using Game.Traps;
using Game.Traps.Json;
using UnityEngine;

namespace Game.Bootstrap.Tests
{
    /// <summary>Small hand-built trap data for simulation tests; production numbers live in the generated field content.</summary>
    internal static class TrapTestLayouts
    {
        public const float ScreenWidth = 10f;

        public static TrapProjectileData Projectile(string id, float radius = .2f, float speed = 5f, float damage = 5f,
            float? returnAfter = null, float? maxDistance = null, string visual = "spikeball") => new TrapProjectileData
        {
            Id = id, Visual = visual, Radius = radius, Speed = speed, Damage = damage, SpinDegreesPerSecond = 0f,
            ReturnAfterSeconds = returnAfter, MaxDistance = maxDistance
        };

        public static TrapShotGroupData Shots(string projectile, int count = 1, float angleStart = 0f, float angleStep = 0f,
            float lateralStart = 0f, float lateralStep = 0f, float delayStep = 0f) => new TrapShotGroupData
        {
            Projectile = projectile, Count = count, AngleStart = angleStart, AngleStep = angleStep,
            LateralStart = lateralStart, LateralStep = lateralStep, DelayStep = delayStep
        };

        public static TrapTypeData Type(string id, string heading, TrapShotGroupData[] shots, float telegraph = .5f,
            float cooldown = 2f, float body = .5f, int count = 1, float? headingStep = null, int? headingCycle = null,
            float? lineLength = null, float? spin = null, string model = null, float? aimResume = null) => new TrapTypeData
        {
            Id = id, Name = id, Tier = 1, Count = count, Model = model, AimResumeDelaySeconds = aimResume, BodyRadius = body, Heading = heading,
            RotationsDegrees = heading == "fixed" ? new[] { 0f } : null, HeadingStepDegrees = headingStep,
            HeadingCycleVolleys = headingCycle, SpinDegreesPerSecond = spin, TelegraphSeconds = telegraph, TelegraphLineLength = lineLength,
            CooldownSeconds = cooldown, Shots = shots
        };

        public static TrapBarrelData Barrels(int count = 0, float share = 0f) => new TrapBarrelData
        {
            Id = "TRAP-BARREL", Count = count, ExplosiveShare = share, BodyRadius = .5f, TriggerRadius = 2f,
            FuseSeconds = 1f, BlastRadius = 2f, Damage = 18f
        };

        public static TrapModelData Model(string key = "cross") => new TrapModelData
        { Key = key, HeadNode = "RotatingHead", TiltDegrees = 54f, Scale = 1f, DepthOffset = 3f };

        public static TrapStartData Start(string type, float x = 3f, float y = 2f, string model = null) => new TrapStartData
        { Type = type, OffsetX = x, OffsetY = y, Model = model };

        public static TrapLayoutData Data(TrapProjectileData[] projectiles, TrapTypeData[] types, int maxActive = 100,
            TrapBarrelData barrels = null, TrapModelData[] models = null, TrapStartData[] starts = null,
            System.Collections.Generic.Dictionary<string, string> sprites = null, TrapStartBarrelData[] startBarrels = null,
            TrapDensityData density = null) => new TrapLayoutData
        {
            ReferenceSeed = 1, RadiusScreenWidths = 1f, PlayerHitRadius = .35f, MaxActiveProjectiles = maxActive,
            EdgeMargin = 2f, StartClearRadius = 4f, MinGap = 1f, ObstacleClearance = 1f, ForwardClearance = 3f,
            PlacementAttempts = 60, Rage = new TrapRageData { MaxMultiplier = 3f, SecondsToMax = 60f },
            Projectiles = projectiles, Types = types, Barrels = barrels ?? Barrels(), Models = models, StartTraps = starts, Sprites = sprites, StartBarrels = startBarrels, Density = density
        };

        public static TrapLayoutDefinition Layout(TrapProjectileData[] projectiles, TrapTypeData[] types, int maxActive = 100,
            TrapBarrelData barrels = null, TrapModelData[] models = null, TrapStartData[] starts = null,
            System.Collections.Generic.Dictionary<string, string> sprites = null, TrapStartBarrelData[] startBarrels = null,
            TrapDensityData density = null) =>
            new TrapLayoutDefinition(Data(projectiles, types, maxActive, barrels, models, starts, sprites, startBarrels, density));

        public static TrapDensityData Density(int one = 2, int two = 7, int three = 1) => new TrapDensityData
        {
            CellWidth = 17.8f, CellHeight = 10f, CountWeights = new[]
            {
                new TrapCountWeightData { Count = 1, Weight = one }, new TrapCountWeightData { Count = 2, Weight = two },
                new TrapCountWeightData { Count = 3, Weight = three }
            }
        };

        public static TrapStartBarrelData StartBarrel(float x = 3f, float y = 0f, bool explosive = false) => new TrapStartBarrelData
        { OffsetX = x, OffsetY = y, Explosive = explosive };

        public static TrapPlacementSet One(TrapLayoutDefinition layout, int typeIndex, Vector2 center,
            float rotation = 0f, float cooldown = .1f, params TrapBarrelPlacement[] barrels) =>
            new TrapPlacementSet(new List<TrapPlacement> { new TrapPlacement(layout.Types[typeIndex], center, rotation, cooldown) },
                new List<TrapBarrelPlacement>(barrels), 0, 0);

        public static TrapRuntime Runtime(TrapLayoutDefinition layout, TrapPlacementSet set, FakeTrapPlayerTarget player,
            IReadOnlyList<IReadOnlyList<Vector2>> outlines = null) => new TrapRuntime(layout, set, player, outlines, 200f);

        /// <summary>Runs the simulation for the given seconds in fixed steps.</summary>
        public static void Advance(TrapRuntime runtime, float seconds, float step = .02f)
        {
            for (var elapsed = 0f; elapsed < seconds - 1e-4f; elapsed += step) runtime.Tick(step, ScreenWidth);
        }

        public static IReadOnlyList<IReadOnlyList<Vector2>> Box(float xMin, float yMin, float xMax, float yMax) =>
            new List<IReadOnlyList<Vector2>>
            {
                new[] { new Vector2(xMin, yMin), new Vector2(xMax, yMin), new Vector2(xMax, yMax), new Vector2(xMin, yMax) }
            };
    }
}
