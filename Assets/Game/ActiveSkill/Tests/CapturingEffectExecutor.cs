using System.Collections.Generic;

namespace Game.ActiveSkill.Tests
{
    public sealed class CapturingEffectExecutor : IActiveSkillEffectExecutor
    {
        public List<ActiveSkillActivation> Activations { get; } = new List<ActiveSkillActivation>();
        public void Schedule(ActiveSkillActivation activation) => Activations.Add(activation);
        public void Tick(float deltaTime, bool isRunning) { }
    }
}
