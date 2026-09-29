using UnityEngine;

namespace Game.Automation
{
    /// <summary>Visible beam or telegraph segment; width and endpoints are world units.</summary>
    public readonly struct BotBeam
    {
        public Vector2 Start { get; }
        public Vector2 End { get; }
        public float HalfWidth { get; }
        public float Weight { get; }

        public BotBeam(Vector2 start, Vector2 end, float halfWidth, float weight)
        {
            Start = start;
            End = end;
            HalfWidth = halfWidth;
            Weight = weight;
        }
    }
}
