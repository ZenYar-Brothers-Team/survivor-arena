namespace Game.Presentation.Json
{
    public sealed class AltarPresentationData
    {
        public string PositiveVisualId { get; set; }
        public string NegativeVisualId { get; set; }
        public string ShrineVisualId { get; set; }
        public float? AltarHeight { get; set; }
        public float? ShrineHeight { get; set; }
        public float? IdleBrightness { get; set; }
        public float? ActiveBrightness { get; set; }
        public float? RingAlpha { get; set; }
        public float? RingThickness { get; set; }
        public float? RestingAlpha { get; set; }
        public int? SortingOrder { get; set; }
    }
}
