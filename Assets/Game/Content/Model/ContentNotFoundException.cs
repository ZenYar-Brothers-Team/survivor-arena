using System;

namespace Game.Content
{
    public sealed class ContentNotFoundException : Exception
    {
        public ContentId Id { get; }

        public ContentNotFoundException(ContentId id)
            : base($"Content id '{id}' was not found.")
        {
            Id = id;
        }
    }
}
