using System;
using System.Collections.Generic;
using Game.Content;
namespace Game.UI
{
    public sealed class NotificationQueue
    {
        private readonly Queue<string> _pending=new Queue<string>();
        private readonly float _duration;
        private float _remaining;
        public string Current {get;private set;}="";
        public event Action Changed;
        public NotificationQueue(float duration) { NumericValidation.ValidatePositive(duration,nameof(duration));_duration=duration; }
        public void Push(string message)
        {
            if(string.IsNullOrWhiteSpace(message))return;
            if(Current.Length==0) { Current=message;_remaining=_duration;Changed?.Invoke(); }
            else { if(_pending.Count==8)_pending.Dequeue();_pending.Enqueue(message); }
        }
        public void Tick(float deltaTime,bool paused)
        {
            NumericValidation.ValidateNonNegative(deltaTime,nameof(deltaTime));
            if(paused||Current.Length==0)return;
            _remaining-=deltaTime;if(_remaining>0)return;
            Current=_pending.Count>0?_pending.Dequeue():"";_remaining=_duration;Changed?.Invoke();
        }
        public void Clear() { _pending.Clear();Current="";_remaining=0;Changed?.Invoke(); }
    }
}
