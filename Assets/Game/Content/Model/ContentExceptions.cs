using System;
using System.Collections.Generic;
using System.Linq;

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

    public sealed class ContentNotFoundException : Exception
    {
        public ContentId Id { get; }

        public ContentNotFoundException(ContentId id)
            : base($"Content id '{id}' was not found.")
        {
            Id = id;
        }
    }

    public sealed class ContentValidationException : Exception
    {
        public IReadOnlyList<string> Errors { get; }

        public ContentValidationException(IReadOnlyList<string> errors)
            : base(BuildMessage(errors))
        {
            Errors = errors;
        }

        private static string BuildMessage(IReadOnlyList<string> errors) =>
            "Content validation failed:" + Environment.NewLine + string.Join(Environment.NewLine, errors.Select(e => "- " + e));
    }
}
