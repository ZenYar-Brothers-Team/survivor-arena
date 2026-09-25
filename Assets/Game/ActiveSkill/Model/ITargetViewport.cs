namespace Game.ActiveSkill
{
    /// <summary>Current visible screen for player target selection (DECISION-0058).</summary>
    public interface ITargetViewport
    {
        TargetViewportRect Current { get; }
    }
}
