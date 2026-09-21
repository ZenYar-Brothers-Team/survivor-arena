using System;
using Game.Content;
namespace Game.Settings
{
    public sealed class VideoMode : IEquatable<VideoMode>
    {
        public int Width { get; }
        public int Height { get; }
        public bool Borderless { get; }
        public VideoMode(int width, int height, bool borderless)
        {
            NumericValidation.ValidateCount(width, nameof(width)); NumericValidation.ValidateCount(height, nameof(height));
            Width = width; Height = height; Borderless = borderless;
        }
        public bool Equals(VideoMode other) => other != null && Width == other.Width && Height == other.Height && Borderless == other.Borderless;
        public override bool Equals(object obj) => Equals(obj as VideoMode);
        public override int GetHashCode() => (Width * 397 ^ Height) * 397 ^ Borderless.GetHashCode();
        public override string ToString() => Width + " × " + Height;
    }
}
