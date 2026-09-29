using UnityEngine;

namespace Game.Automation
{
    internal struct BotTrajectoryRoute
    {
        public Vector2 First;
        public Vector2 Second;
        public Vector2 Target;
        public Vector2 Exit;
        public int Stage;
        public bool HasXpTarget;

        public Vector2 Waypoint => Stage == 0 ? First : Stage == 1 ? Second : Stage == 2 ? Target : Exit;

        public BotTrajectoryRoute(Vector2 first, Vector2 second, Vector2 target, int stage, bool hasXpTarget,
            Vector2? exit = null)
        {
            First = first;
            Second = second;
            Target = target;
            Exit = exit ?? target;
            Stage = stage;
            HasXpTarget = hasXpTarget;
        }
    }
}
