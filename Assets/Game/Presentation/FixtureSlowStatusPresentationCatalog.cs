using System;
using Game.Content.Json;
using Game.Presentation.Json;
using UnityEngine;

namespace Game.Presentation
{
    public static class FixtureSlowStatusPresentationCatalog
    {
        private const string ResourcePath = "Content/Presentation/FixtureSlowStatusPresentation";

        public static SlowStatusPresentationProfile Create() =>
            Map(JsonContentFile.Load<SlowStatusPresentationProfileData>(ResourcePath));

        public static SlowStatusPresentationProfile Map(SlowStatusPresentationProfileData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            return new SlowStatusPresentationProfile(
                ToColor(data.TintColor, "tintColor"), ToColor(data.IceColor, "iceColor"),
                ToColor(data.OutlineColor, "outlineColor"), Require(data.OutlineWidth, "outlineWidth"),
                Require(data.BarWidth, "barWidth"), Require(data.BarHeight, "barHeight"),
                Require(data.BarOffsetY, "barOffsetY"), ToColor(data.BarFillColor, "barFillColor"),
                ToColor(data.BarBackColor, "barBackColor"), Require(data.PreviewSlowFraction, "previewSlowFraction"),
                Require(data.PreviewSlowSeconds, "previewSlowSeconds"));
        }

        private static float Require(float? value, string name) =>
            value ?? throw new InvalidOperationException($"Slow status presentation requires {name}.");

        private static Color ToColor(float[] rgba, string name)
        {
            if (rgba == null || rgba.Length != 4)
                throw new InvalidOperationException($"Slow status presentation {name} requires exactly four RGBA values.");
            return new Color(rgba[0], rgba[1], rgba[2], rgba[3]);
        }
    }
}
