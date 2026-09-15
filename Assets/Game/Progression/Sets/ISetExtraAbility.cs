using System;

namespace Game.Progression
{
    public interface ISetExtraAbility : IDisposable
    {
        void Tick(float deltaTime, bool isRunning);
    }
}
