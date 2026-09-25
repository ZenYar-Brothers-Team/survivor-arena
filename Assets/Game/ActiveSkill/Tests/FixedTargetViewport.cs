namespace Game.ActiveSkill.Tests
{
    public sealed class FixedTargetViewport : ITargetViewport
    {
        public TargetViewportRect Current { get; set; }
        public FixedTargetViewport(TargetViewportRect current) { Current = current; }
    }
}
