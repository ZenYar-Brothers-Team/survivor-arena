using System;
using UnityEngine;

namespace Game.Presentation.Tests
{
    public sealed class FakePresentationSource : IPresentationSource
    {
        public bool IsRunning { get; set; } = true;
        public Vector2 Velocity { get; set; }
        public event Action<PresentationSignal> Signaled;
        public void Emit(PresentationSignal signal) => Signaled?.Invoke(signal);
    }
}
