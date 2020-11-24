using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.ContentStore.DataAccess.Query;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.ContentStore.DataAccess
{
    public class DocumentQuery : IDocumentQuery
    {
        private readonly IQueryMapper _queryMapper;
        private readonly IDocumentQueryFactory _queryFactory;

        public DocumentQuery(IQueryMapper queryMapper, IDocumentQueryFactory queryFactory)
        {
            Guard.AgainstNull(queryMapper, nameof(queryMapper));
            Guard.AgainstNull(queryFactory, nameof(queryFactory));

            _queryMapper = queryMapper;
            _queryFactory = queryFactory;
        }

        public IEnumerable<Query.Document> Search(Query.Document.Specification specification)
        {
            Guard.AgainstNull(specification, nameof(specification));

            var result = _queryMapper.MapObjects<Query.Document>(_queryFactory.Search(specification))
                .ToDictionary(document => document.Id);

            if (specification.StatusEventsIncluded && result.Any())
            {
                foreach (var mappedRow in _queryMapper.MapRows<Query.Document.StatusEvent>(_queryFactory.GetStatusEvents(result.Keys)))
                {
                    if (result.TryGetValue(Columns.DocumentId.MapFrom(mappedRow.Row), out var document))
                    {
                        document.StatusEvents.Add(mappedRow.Result);
                    }
                }
            }

            if (specification.PropertiesIncluded && result.Any())
            {
                foreach (var mappedRow in _queryMapper.MapRows<Query.Document.Property>(query: _queryFactory.GetProperties(result.Keys)))
                {
                    if (result.TryGetValue(Columns.DocumentId.MapFrom(mappedRow.Row), out var document))
                    {
                        document.Properties.Add(mappedRow.Result);
                    }
                }
            }

            return result.Values;
        }

        public DocumentContent FindContent(Guid id)
        {
            return _queryMapper.MapObject<DocumentContent>(_queryFactory.FindContent(id));
        }
    }
}