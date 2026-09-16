namespace Game.Presentation.Json
{
    public sealed class SpriteMotionProfileData
    {
        public string Id { get; set; }
        public float ReferenceSpeed { get; set; }
        public float IdleBobAmplitude { get; set; }
        public float IdleFrequency { get; set; }
        public float IdleBreathStretch { get; set; }
        public float IdleSwayDegrees { get; set; }
        public float BobAmplitude { get; set; }
        public float BobFrequency { get; set; }
        public float LocomotionStretch { get; set; }
        public float MaxTiltDegrees { get; set; }
        public float HitDurationSeconds { get; set; }
        public float HitSquash { get; set; }
        public float HitTiltDegrees { get; set; }
        public float HitFlashDurationSeconds { get; set; }
        public float HitFlashRed { get; set; }
        public float HitFlashGreen { get; set; }
        public float HitFlashBlue { get; set; }
        public float HitFlashAlpha { get; set; }
        public float SpawnDurationSeconds { get; set; }
        public float SpawnScaleFrom { get; set; }
    }
}
