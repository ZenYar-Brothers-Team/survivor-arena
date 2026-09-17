using System;

namespace Game.Content
{
    public readonly struct ContentReference
    {
        public ContentId Id { get; }
        public Type ExpectedType { get; }

        public ContentReference(ContentId id, Type expectedType)
        {
            Id = id;
            ExpectedType = expectedType;
        }
    }
}
