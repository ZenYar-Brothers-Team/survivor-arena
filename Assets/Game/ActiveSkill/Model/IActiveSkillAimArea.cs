using UnityEngine;

namespace Game.ActiveSkill
{
    /// <summary>Fallback aim point for point attacks when no valid enemy is on screen (DECISION-0058).</summary>
    public interface IActiveSkillAimArea
    {
        /// <summary>True when selection is limited to the visible screen and fallbacks apply.</summary>
        bool IsScreenLimited { get; }
        bool TryPickPoint(Vector2 origin, float radius, System.Random random, out Vector2 point);
    }
}
