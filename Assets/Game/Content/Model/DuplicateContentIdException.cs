using System;

namespace Game.Content
{
    public sealed class DuplicateContentIdException : Exception
    {
        public ContentId Id { get; }

        public DuplicateContentIdException(ContentId id)
            : base($"Content id '{id}' is already registered.")
        {
            Id = id;
        }
    }
}
