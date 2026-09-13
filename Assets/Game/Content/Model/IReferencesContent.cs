using System;
using System.Collections.Generic;

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

    public interface IReferencesContent
    {
        IEnumerable<ContentReference> GetReferencedContent();
    }
}
