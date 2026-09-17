using System;
using Game.Content;
using UnityEngine;

namespace Game.Presentation
{
    public sealed class SpriteMotionProfile : IContentDefinition
    {
        public ContentId Id { get; }
        public float ReferenceSpeed { get; }
        public float IdleBobAmplitude { get; }
        public float IdleFrequency { get; }
        public float IdleBreathStretch { get; }
        public float IdleSwayDegrees { get; }
        public float BobAmplitude { get; }
        public float BobFrequency { get; }
        public float LocomotionStretch { get; }
        public float MaxTiltDegrees { get; }
        public float HitDurationSeconds { get; }
        public float HitSquash { get; }
        public float HitTiltDegrees { get; }
        public float HitFlashDurationSeconds { get; }
        public Color HitFlashColor { get; }
        public float SpawnDurationSeconds { get; }
        public float SpawnScaleFrom { get; }

        public SpriteMotionProfile(
            ContentId id,
            float referenceSpeed,
            float idleBobAmplitude,
            float idleFrequency,
            float idleBreathStretch,
            float idleSwayDegrees,
            float bobAmplitude,
            float bobFrequency,
            float locomotionStretch,
            float maxTiltDegrees,
            float hitDurationSeconds,
            float hitSquash,
            float hitTiltDegrees,
            float hitFlashDurationSeconds,
            Color hitFlashColor,
            float spawnDurationSeconds,
            float spawnScaleFrom)
        {
            if (!id.IsValid)
                throw new ArgumentException("Sprite motion profile requires a valid content id.", nameof(id));

            NumericValidation.ValidatePositive(referenceSpeed, nameof(referenceSpeed));
            NumericValidation.ValidateRange(idleBobAmplitude, 0f, 0.1f, nameof(idleBobAmplitude));
            NumericValidation.ValidateRange(idleFrequency, 0.1f, 5f, nameof(idleFrequency));
            NumericValidation.ValidateRange(idleBreathStretch, 0f, 0.08f, nameof(idleBreathStretch));
            NumericValidation.ValidateRange(idleSwayDegrees, 0f, 3f, nameof(idleSwayDegrees));
            NumericValidation.ValidateRange(bobAmplitude, 0f, 0.25f, nameof(bobAmplitude));
            NumericValidation.ValidateRange(bobFrequency, 0f, 20f, nameof(bobFrequency));
            NumericValidation.ValidateRange(locomotionStretch, 0f, 0.12f, nameof(locomotionStretch));
            NumericValidation.ValidateRange(maxTiltDegrees, 0f, 8f, nameof(maxTiltDegrees));
            NumericValidation.ValidatePositive(hitDurationSeconds, nameof(hitDurationSeconds));
            NumericValidation.ValidateRange(hitSquash, 0f, 0.12f, nameof(hitSquash));
            NumericValidation.ValidateRange(hitTiltDegrees, 0f, 8f, nameof(hitTiltDegrees));
            NumericValidation.ValidatePositive(hitFlashDurationSeconds, nameof(hitFlashDurationSeconds));
            NumericValidation.ValidateRange(hitFlashColor.r, 0f, 1f, nameof(hitFlashColor));
            NumericValidation.ValidateRange(hitFlashColor.g, 0f, 1f, nameof(hitFlashColor));
            NumericValidation.ValidateRange(hitFlashColor.b, 0f, 1f, nameof(hitFlashColor));
            NumericValidation.ValidateRange(hitFlashColor.a, 0f, 1f, nameof(hitFlashColor));
            NumericValidation.ValidatePositive(spawnDurationSeconds, nameof(spawnDurationSeconds));
            NumericValidation.ValidateRange(spawnScaleFrom, 0.5f, 1f, nameof(spawnScaleFrom));

            Id = id;
            ReferenceSpeed = referenceSpeed;
            IdleBobAmplitude = idleBobAmplitude;
            IdleFrequency = idleFrequency;
            IdleBreathStretch = idleBreathStretch;
            IdleSwayDegrees = idleSwayDegrees;
            BobAmplitude = bobAmplitude;
            BobFrequency = bobFrequency;
            LocomotionStretch = locomotionStretch;
            MaxTiltDegrees = maxTiltDegrees;
            HitDurationSeconds = hitDurationSeconds;
            HitSquash = hitSquash;
            HitTiltDegrees = hitTiltDegrees;
            HitFlashDurationSeconds = hitFlashDurationSeconds;
            HitFlashColor = hitFlashColor;
            SpawnDurationSeconds = spawnDurationSeconds;
            SpawnScaleFrom = spawnScaleFrom;
        }
    }
}
