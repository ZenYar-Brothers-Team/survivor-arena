using System;
using Game.Content;

namespace Game.Combat
{
    internal readonly struct SlowSourceKey : IEquatable<SlowSourceKey>
    {
        private readonly Guid _owner;
        private readonly ContentId? _content;
        private readonly string _channel;
        public SlowSourceKey(CombatDamageRequest request)
        {
            _owner = request.Source.Owner.LifeId;
            _content = request.Source.ContentId;
            _channel = request.Controls.Channel;
        }
        public bool Equals(SlowSourceKey other) => _owner == other._owner && _content == other._content && _channel == other._channel;
        public override bool Equals(object obj) => obj is SlowSourceKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_owner, _content, _channel);
    }
}
