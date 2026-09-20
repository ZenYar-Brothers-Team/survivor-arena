using System;
using System.Collections.Generic;
using Game.Content.Json;
using Game.Presentation.Json;
using UnityEngine;

namespace Game.Presentation
{
    public static class FixtureSpriteMotionProfileCatalog
    {
        private const string ResourcePath = "Content/Presentation/FixtureSpriteMotionProfiles";

        public static IReadOnlyList<SpriteMotionProfile> Create()
        {
            var data = JsonContentFile.Load<SpriteMotionProfileData[]>(ResourcePath);
            var profiles = new SpriteMotionProfile[data.Length];
            for (var i = 0; i < data.Length; i++)
                profiles[i] = ToProfile(data[i]);
            return profiles;
        }

        private static SpriteMotionProfile ToProfile(SpriteMotionProfileData data)
        {
            if (data == null)
                throw new InvalidOperationException("Fixture sprite motion profile data cannot contain null entries.");

            return new SpriteMotionProfile(
                data.Id,
                data.ReferenceSpeed,
                data.IdleBobAmplitude,
                data.IdleFrequency,
                data.IdleBreathStretch,
                data.IdleSwayDegrees,
                data.BobAmplitude,
                data.BobFrequency,
                data.LocomotionStretch,
                data.MaxTiltDegrees,
                data.HitDurationSeconds,
                data.HitSquash,
                data.HitTiltDegrees,
                data.HitFlashDurationSeconds,
                new Color(data.HitFlashRed, data.HitFlashGreen, data.HitFlashBlue, data.HitFlashAlpha),
                data.SpawnDurationSeconds,
                data.SpawnScaleFrom);
        }
    }
}
