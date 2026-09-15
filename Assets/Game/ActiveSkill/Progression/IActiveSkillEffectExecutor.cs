namespace Game.ActiveSkill
{
    public interface IActiveSkillEffectExecutor
    {
        void Schedule(ActiveSkillActivation activation);
        void Tick(float deltaTime, bool isRunning);
    }
}
