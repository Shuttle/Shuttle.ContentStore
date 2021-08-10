using System;
using System.Collections.Generic;
using Shuttle.Core.Data;

namespace Shuttle.ContentStore.DataAccess
{
    public interface IContentQueryFactory
    {
        IQuery Save(Content content);
        IQuery RemoveStatusEvents(Guid contentId);
        IQuery SaveStatusEvent(Guid contentId, int sequenceNumber, Content.StatusEvent statusEvent);
        IQuery Get(Guid id);
        IQuery GetStatusEvents(Guid id);
        IQuery RemoveProperties(Guid contentId);
        IQuery SaveProperty(Guid contentId, string name, string value);
        IQuery GetProperties(Guid id);
        IQuery Search(Query.Content.Specification specification);
        IQuery GetStatusEvents(IEnumerable<Guid> contentIds);
        IQuery GetProperties(IEnumerable<Guid> contentIds);
        IQuery FindRawContent(Guid id);
    }
}