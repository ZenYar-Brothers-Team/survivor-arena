namespace Game.Presentation.Json
{
    public sealed class AltarPresentationData
    {
        public string PositiveVisualId { get; set; }
        public string NegativeVisualId { get; set; }
        public string ShrineVisualId { get; set; }
        public float? AltarHeight { get; set; }
        public float? ShrineHeight { get; set; }
        public float? AltarContactRadius { get; set; }
        public float? AltarContactOffsetY { get; set; }
        public float? ShrineContactRadius { get; set; }
        public float? ShrineContactOffsetY { get; set; }
        public float? IdleBrightness { get; set; }
        public float? ActiveBrightness { get; set; }
        public float? RingAlpha { get; set; }
        public float? RingThickness { get; set; }
        public float? RestingAlpha { get; set; }
        public int? SortingOrder { get; set; }
        public float? StateRingRadius { get; set; }
        public float? ShrineStateRingRadius { get; set; }
        public float? StateRingAspect { get; set; }
        public float? StateRingThickness { get; set; }
        public float? GlowRadius { get; set; }
        public float? GlowHeightFraction { get; set; }
        public float? ReadyGlowAlpha { get; set; }
        public float? ActiveGlowAlpha { get; set; }
        public float? FlashSeconds { get; set; }
        public string GlowColor { get; set; }
    }
}
