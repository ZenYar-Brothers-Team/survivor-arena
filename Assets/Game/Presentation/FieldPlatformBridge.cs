namespace Game.Presentation
{
    /// <summary>A straight bridge between two platforms (indices into <see cref="FieldPlatformLayout.Platforms"/>).</summary>
    public readonly struct FieldPlatformBridge
    {
        public int From { get; }
        public int To { get; }
        /// <summary>Distance between the two platform edges, i.e. the length of the open span.</summary>
        public float Gap { get; }

        public FieldPlatformBridge(int from, int to, float gap)
        {
            From = from;
            To = to;
            Gap = gap;
        }
    }
}
