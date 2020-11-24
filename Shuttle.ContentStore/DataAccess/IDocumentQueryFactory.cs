using System;
using System.Collections.Generic;
using Shuttle.Core.Data;

namespace Shuttle.ContentStore.DataAccess
{
    public interface IDocumentQueryFactory
    {
        IQuery Save(Document document);
        IQuery RemoveStatusEvents(Guid documentId);
        IQuery SaveStatusEvent(Guid documentId, int sequenceNumber, Document.StatusEvent statusEvent);
        IQuery Get(Guid id);
        IQuery GetStatusEvents(Guid id);
        IQuery RemoveProperties(Guid documentId);
        IQuery SaveProperty(Guid documentId, string name, string value);
        IQuery GetProperties(Guid id);
        IQuery Search(Query.Document.Specification specification);
        IQuery GetStatusEvents(IEnumerable<Guid> documentIds);
        IQuery GetProperties(IEnumerable<Guid> documentIds);
        IQuery FindContent(Guid id);
    }
}