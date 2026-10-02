using System;
using Game.Content;
using Game.Content.Json;
using Game.Presentation.Json;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Validated rim artwork and mesh-glyph tuning for the academy study. No gameplay values.</summary>
    public sealed class ZoneSealPresentationProfile
    {
        public Sprite RimSprite { get; }
        public float RimReferenceRadius { get; }
        public Sprite ExperienceGlyphSprite { get; }
        public Sprite KnockbackGlyphSprite { get; }
        public float RasterGlyphDiameterFraction { get; }
        public Sprite PortalGlyphSprite { get; }
        public float PortalCollapseSeconds { get; }
        public float PortalTransitSeconds { get; }
        public float PortalExpandSeconds { get; }
        public float RimTintBlend { get; }
        public float GlyphScale { get; }
        public float StrokeFraction { get; }
        public float MotionRadius { get; }
        public float RimAlpha { get; }
        public float GlyphAlpha { get; }
        public float MotionAlpha { get; }
        public float InactiveVisibilityMultiplier { get; }
        public float RotationDegreesPerSecond { get; }
        public float RelocatingRimDegreesPerSecond { get; }
        public float ApplicationFlashSeconds { get; }
        public float ApplicationFlashLightBlend { get; }
        public float FlickerFrequency { get; }
        public float FlickerAmount { get; }
        public float PortalRestVisibility { get; }
        public float GlyphLightBlend { get; }
        public float IdleColorBlend { get; }
        public float SlowMotionMultiplier { get; }
        public float HasteMotionMultiplier { get; }
        public Color InkColor { get; }
        public Color LightColor { get; }
        public Color RiftHitColor { get; }
        public float RiftHitSeconds { get; }
        public float RiftHitScale { get; }
        public int SortingOrder { get; }

        public static ZoneSealPresentationProfile Load() => new ZoneSealPresentationProfile(
            JsonContentFile.Load<ZoneSealPresentationData>("Content/Presentation/FixtureZoneSeals"));

        public ZoneSealPresentationProfile(ZoneSealPresentationData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            RimSprite = FixtureSpriteCatalog.CreateOne(new ContentId(data.RimVisualId), SpriteRole.Telegraph).Sprite;
            RimReferenceRadius = Require(data.RimReferenceRadius, "rimReferenceRadius", .5f, 1f);
            ExperienceGlyphSprite = FixtureSpriteCatalog.CreateOne(new ContentId(data.ExperienceGlyphVisualId), SpriteRole.Telegraph).Sprite;
            KnockbackGlyphSprite = FixtureSpriteCatalog.CreateOne(new ContentId(data.KnockbackGlyphVisualId), SpriteRole.Telegraph).Sprite;
            RasterGlyphDiameterFraction = Require(data.RasterGlyphDiameterFraction, "rasterGlyphDiameterFraction", .4f, 1f);
            PortalGlyphSprite = FixtureSpriteCatalog.CreateOne(new ContentId(data.PortalGlyphVisualId), SpriteRole.Telegraph).Sprite;
            PortalCollapseSeconds = Require(data.PortalCollapseSeconds, "portalCollapseSeconds", .05f, 1f);
            PortalTransitSeconds = Require(data.PortalTransitSeconds, "portalTransitSeconds", .1f, 3f);
            PortalExpandSeconds = Require(data.PortalExpandSeconds, "portalExpandSeconds", .05f, 1f);
            RimTintBlend = Require(data.RimTintBlend, "rimTintBlend", 0f, 1f);
            GlyphScale = Require(data.GlyphScale, "glyphScale", 1f, 2f);
            StrokeFraction = Require(data.StrokeFraction, "strokeFraction", .005f, .04f);
            MotionRadius = Require(data.MotionRadius, "motionRadius", .4f, .75f);
            RimAlpha = Require(data.RimAlpha, "rimAlpha", 0f, 1f);
            GlyphAlpha = Require(data.GlyphAlpha, "glyphAlpha", 0f, 1f);
            MotionAlpha = Require(data.MotionAlpha, "motionAlpha", 0f, 1f);
            InactiveVisibilityMultiplier = Require(data.InactiveVisibilityMultiplier, "inactiveVisibilityMultiplier", 0f, 1f);
            RotationDegreesPerSecond = Require(data.RotationDegreesPerSecond, "rotationDegreesPerSecond", 0f, 90f);
            RelocatingRimDegreesPerSecond = Require(data.RelocatingRimDegreesPerSecond, "relocatingRimDegreesPerSecond", .1f, 90f);
            ApplicationFlashSeconds = Require(data.ApplicationFlashSeconds, "applicationFlashSeconds", .05f, 1f);
            ApplicationFlashLightBlend = Require(data.ApplicationFlashLightBlend, "applicationFlashLightBlend", 0f, 1f);
            FlickerFrequency = Require(data.FlickerFrequency, "flickerFrequency", .1f, 5f);
            FlickerAmount = Require(data.FlickerAmount, "flickerAmount", 0f, .25f);
            PortalRestVisibility = Require(data.PortalRestVisibility, "portalRestVisibility", 0f, .25f);
            GlyphLightBlend = Require(data.GlyphLightBlend, "glyphLightBlend", 0f, 1f);
            IdleColorBlend = Require(data.IdleColorBlend, "idleColorBlend", 0f, 1f);
            SlowMotionMultiplier = Require(data.SlowMotionMultiplier, "slowMotionMultiplier", 0f, 1f);
            HasteMotionMultiplier = Require(data.HasteMotionMultiplier, "hasteMotionMultiplier", 1f, 3f);
            InkColor = Parse(data.InkColor, "inkColor");
            LightColor = Parse(data.LightColor, "lightColor");
            RiftHitColor = Parse(data.RiftHitColor, "riftHitColor");
            RiftHitSeconds = Require(data.RiftHitSeconds, "riftHitSeconds", .05f, .2f);
            RiftHitScale = Require(data.RiftHitScale, "riftHitScale", .5f, 1.5f);
            SortingOrder = data.SortingOrder ?? throw new ArgumentException("Zone seals require sortingOrder.");
        }

        private static float Require(float? value, string name, float min, float max)
        {
            var number = value ?? throw new ArgumentException($"Zone seals require {name}.");
            NumericValidation.ValidateRange(number, min, max, name);
            return number;
        }

        private static Color Parse(string value, string name) => ColorUtility.TryParseHtmlString(value, out var color)
            ? color : throw new ArgumentException($"Zone seals require an HTML {name}.");
    }
}
