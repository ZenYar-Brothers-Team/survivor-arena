using System;
using UnityEngine;

namespace Game.Presentation
{
    public interface IPresentationSource
    {
        bool IsRunning { get; }
        Vector2 Velocity { get; }
        event Action<PresentationSignal> Signaled;
    }
}
