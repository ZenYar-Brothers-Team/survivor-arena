namespace Game.Presentation.Json
{
    /// <summary>Seal tuning: registered rim sprite, tint 0..1, glyph scale 1..2, widths/radii relative to zone radius.</summary>
    public sealed class ZoneSealPresentationData
    {
        public string RimVisualId { get; set; }
        public string PortalVisualId { get; set; }
        public float? PortalHeight { get; set; }
        public int? PortalSortingOrder { get; set; }
        public float? PortalCollapseSeconds { get; set; }
        public float? PortalTransitSeconds { get; set; }
        public float? PortalExpandSeconds { get; set; }
        public float? RimTintBlend { get; set; }
        public float? GlyphScale { get; set; }
        public float? StrokeFraction { get; set; }
        public float? MotionRadius { get; set; }
        public float? RimAlpha { get; set; }
        public float? GlyphAlpha { get; set; }
        public float? MotionAlpha { get; set; }
        public float? RotationDegreesPerSecond { get; set; }
        public float? RelocatingRimDegreesPerSecond { get; set; }
        public float? ApplicationFlashSeconds { get; set; }
        public float? ApplicationFlashLightBlend { get; set; }
        public float? FlickerFrequency { get; set; }
        public float? FlickerAmount { get; set; }
        public float? PortalRestVisibility { get; set; }
        public float? GlyphLightBlend { get; set; }
        public float? IdleColorBlend { get; set; }
        public float? SlowMotionMultiplier { get; set; }
        public float? HasteMotionMultiplier { get; set; }
        public string InkColor { get; set; }
        public string LightColor { get; set; }
        public string RiftHitColor { get; set; }
        public float? RiftHitSeconds { get; set; }
        public float? RiftHitScale { get; set; }
        public int? SortingOrder { get; set; }
    }
}
