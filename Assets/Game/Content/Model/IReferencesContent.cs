using System.Collections.Generic;

namespace Game.Content
{
    public interface IReferencesContent
    {
        IEnumerable<ContentReference> GetReferencedContent();
    }
}
