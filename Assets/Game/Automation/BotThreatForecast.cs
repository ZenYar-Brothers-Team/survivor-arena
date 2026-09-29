using UnityEngine;

namespace Game.Automation
{
    /// <summary>AB-13: follow the simulated player; unsupported movement stays linear and is reported.</summary>
    public static class BotThreatForecast
    {
        public static Vector2 Advance(BotThreat threat, Vector2 position, Vector2 player, float seconds)
        {
            if (threat.Motion == BotThreatMotion.Linear) return position + threat.Velocity * seconds;
            var offset = player - position;
            var distance = offset.magnitude;
            if (distance <= 0.00001f) return position;
            var step = threat.MovementSpeed * seconds;
            if (threat.Motion == BotThreatMotion.KeepDistance)
            {
                if (distance < threat.PreferredDistance - threat.DistanceTolerance)
                    return position - offset / distance * step;
                if (distance <= threat.PreferredDistance + threat.DistanceTolerance) return position;
            }
            return position + offset / distance * Mathf.Min(step, distance);
        }
    }
}
