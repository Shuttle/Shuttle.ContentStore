using System;
using System.Collections.Generic;

namespace Shuttle.ContentStore.DataAccess
{
    public interface IContentQuery
    {
        IEnumerable<Query.Content> Search(Query.Content.Specification specification);
        Query.RawContent FindRawContent(Guid id);
    }
}