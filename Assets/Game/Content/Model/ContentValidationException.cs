using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Content
{
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
