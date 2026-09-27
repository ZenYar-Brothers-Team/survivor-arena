using Game.Content;

namespace Game.Enemy
{
    /// <summary>State of one boss beam attack: telegraph, then active beams; hits at most once.</summary>
    internal sealed class BossBeamInstance
    {
        public BossBeamProfile Profile;
        public ContentId SourceId;
        public float BaseAngleDegrees;
        public bool Active;
        public float Elapsed;
        public bool HasHit;
    }
}
