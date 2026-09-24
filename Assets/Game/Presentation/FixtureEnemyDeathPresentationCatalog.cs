using Game.Content.Json;
using Game.Presentation.Json;
using UnityEngine;

namespace Game.Presentation
{
    public static class FixtureEnemyDeathPresentationCatalog
    {
        private const string ResourcePath = "Content/Presentation/FixtureEnemyDeathPresentation";

        public static EnemyDeathPresentationProfile Create()
        {
            var data = JsonContentFile.Load<EnemyDeathPresentationProfileData>(ResourcePath);
            return new EnemyDeathPresentationProfile(data.SquashDurationSeconds, data.FadeDurationSeconds,
                data.SquashWidthScale, data.SquashHeightScale, data.EndScale,
                new Color(data.EndRed, data.EndGreen, data.EndBlue, data.EndAlpha), data.DustCount,
                data.DustLifetimeSeconds, data.DustSpeed, data.DustSize,
                new Color(data.DustRed, data.DustGreen, data.DustBlue, data.DustAlpha));
        }
    }
}
