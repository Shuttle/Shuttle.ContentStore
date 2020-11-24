using System;
using System.Collections.Generic;

namespace Shuttle.ContentStore.DataAccess
{
    public interface IDocumentQuery
    {
        IEnumerable<Query.Document> Search(Query.Document.Specification specification);
        Query.DocumentContent FindContent(Guid id);
    }
}