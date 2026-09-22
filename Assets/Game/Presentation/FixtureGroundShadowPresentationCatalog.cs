using Game.Content.Json;
using Game.Presentation.Json;
using UnityEngine;

namespace Game.Presentation
{
    public static class FixtureGroundShadowPresentationCatalog
    {
        private const string ResourcePath = "Content/Presentation/FixtureGroundShadowPresentation";

        public static GroundShadowPresentationProfile Create()
        {
            var data = JsonContentFile.Load<GroundShadowPresentationProfileData>(ResourcePath);
            return new GroundShadowPresentationProfile(data.FallbackWidth, data.ContactWidthScale,
                data.Height, data.OffsetY, data.FallbackCenterY,
                new Color(data.Red, data.Green, data.Blue, data.Alpha));
        }
    }
}
