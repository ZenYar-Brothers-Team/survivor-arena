using System;

namespace Game.Content
{
    public readonly struct ContentId : IEquatable<ContentId>
    {
        private readonly string _value;

        public ContentId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Content id cannot be null, empty, or whitespace.", nameof(value));

            _value = value;
        }

        public bool IsValid => !string.IsNullOrWhiteSpace(_value);

        public static implicit operator ContentId(string value) => new ContentId(value);

        public bool Equals(ContentId other) => string.Equals(_value, other._value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is ContentId other && Equals(other);
        public override int GetHashCode() => _value != null ? _value.GetHashCode() : 0;
        public override string ToString() => _value ?? string.Empty;

        public static bool operator ==(ContentId left, ContentId right) => left.Equals(right);
        public static bool operator !=(ContentId left, ContentId right) => !left.Equals(right);
    }
}
