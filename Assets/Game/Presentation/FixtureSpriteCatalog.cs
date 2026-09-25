using System;
using System.Collections.Generic;
using Game.Content;
using Game.Content.Json;
using Game.Movement;
using Game.Presentation.Json;
using UnityEngine;

namespace Game.Presentation
{
    public static class FixtureSpriteCatalog
    {
        private const string ResourcePath = "Content/Presentation/FixtureSprites";

        public static IReadOnlyList<SpriteDefinition> CreateFor(IEnumerable<ContentId> visualIds)
        {
            var definitions = new List<SpriteDefinition>();
            var seen = new HashSet<ContentId>();
            var configured = LoadConfiguredSprites();
            foreach (var id in visualIds)
            {
                if (!id.IsValid || !seen.Add(id))
                    continue;

                if (!configured.ContainsKey(id) && !id.ToString().StartsWith("FIXTURE-", StringComparison.Ordinal))
                    throw new InvalidOperationException($"Production visual '{id}' cannot use fixture fallback.");

                definitions.Add(configured.TryGetValue(id, out var sprite)
                    ? sprite
                    : new SpriteDefinition(id, PlaceholderSprite.Shared));
            }

            return definitions;
        }

        private static IReadOnlyDictionary<ContentId, SpriteDefinition> LoadConfiguredSprites()
        {
            var data = JsonContentFile.Load<SpriteDefinitionData[]>(ResourcePath);
            var definitions = new Dictionary<ContentId, SpriteDefinition>();
            for (var i = 0; i < data.Length; i++)
            {
                var entry = data[i] ?? throw new InvalidOperationException(
                    "Fixture sprite data cannot contain null entries.");
                if (string.IsNullOrWhiteSpace(entry.ResourcePath))
                    throw new InvalidOperationException($"Fixture sprite '{entry.Id}' requires a resource path.");
                if (!entry.Role.HasValue || entry.Role == SpriteRole.Unspecified)
                    throw new InvalidOperationException($"Fixture sprite '{entry.Id}' requires an explicit role.");

                ContentId id = entry.Id;
                var sprite = Resources.Load<Sprite>(entry.ResourcePath);
                if (sprite == null)
                    throw new InvalidOperationException(
                        $"Fixture sprite '{id}' is missing at Resources/{entry.ResourcePath}.");
                if (entry.ContactRadius.HasValue != entry.ContactCenterY.HasValue)
                    throw new InvalidOperationException($"Sprite '{id}' requires both contactRadius and contactCenterY.");
                var contact = entry.ContactRadius.HasValue
                    ? new SpriteContactProfile(entry.ContactRadius.Value, entry.ContactCenterY.Value) : null;
                var projectile = ToProjectile(entry, id);
                if (!definitions.TryAdd(id, new SpriteDefinition(id, sprite, entry.Role.Value, contact, projectile)))
                    throw new InvalidOperationException($"Duplicate fixture sprite id '{id}'.");
            }

            return definitions;
        }

        private static ProjectilePresentationProfile ToProjectile(SpriteDefinitionData entry, ContentId id)
        {
            if (entry.Role == SpriteRole.Projectile && entry.Projectile == null)
                throw new InvalidOperationException($"Projectile sprite '{id}' requires presentation settings.");
            if (entry.Role != SpriteRole.Projectile && entry.Projectile != null)
                throw new InvalidOperationException($"Non-projectile sprite '{id}' cannot define projectile presentation.");
            if (entry.Projectile == null) return null;
            var data = entry.Projectile;
            return new ProjectilePresentationProfile(data.VisualScale, data.SpinDegreesPerSecond,
                data.ImpactDurationSeconds, data.FlashSize,
                new Color(data.FlashRed, data.FlashGreen, data.FlashBlue, data.FlashAlpha),
                data.ParticleCount, data.ParticleSize, data.ParticleSpeed,
                new Color(data.ParticleRed, data.ParticleGreen, data.ParticleBlue, data.ParticleAlpha),
                ToExplosion(data.Explosion), ToThreatHalo(data.ThreatHalo));
        }

        private static ProjectileThreatHaloProfile ToThreatHalo(ProjectileThreatHaloData data) =>
            data == null ? null : new ProjectileThreatHaloProfile(data.Scale,
                new Color(data.Red, data.Green, data.Blue, data.Alpha), data.PulseSeconds, data.PulseAmplitude);

        private static ExplosionPresentationProfile ToExplosion(ExplosionPresentationProfileData data)
        {
            if (data == null) return null;
            return new ExplosionPresentationProfile(data.DurationSeconds, data.FlashSizeMultiplier,
                new Color(data.FlashRed, data.FlashGreen, data.FlashBlue, data.FlashAlpha),
                data.ParticleCount, data.ParticleSizeMultiplier, data.ParticleSpeedMultiplier,
                new Color(data.ParticleRed, data.ParticleGreen, data.ParticleBlue, data.ParticleAlpha));
        }
    }
}
