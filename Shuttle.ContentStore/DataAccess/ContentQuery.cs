using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.ContentStore.DataAccess.Query;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.ContentStore.DataAccess
{
    public class ContentQuery : IContentQuery
    {
        private readonly IQueryMapper _queryMapper;
        private readonly IContentQueryFactory _queryFactory;

        public ContentQuery(IQueryMapper queryMapper, IContentQueryFactory queryFactory)
        {
            Guard.AgainstNull(queryMapper, nameof(queryMapper));
            Guard.AgainstNull(queryFactory, nameof(queryFactory));

            _queryMapper = queryMapper;
            _queryFactory = queryFactory;
        }

        public IEnumerable<Query.Content> Search(Query.Content.Specification specification)
        {
            Guard.AgainstNull(specification, nameof(specification));

            var result = _queryMapper.MapObjects<Query.Content>(_queryFactory.Search(specification))
                .ToDictionary(content => content.Id);

            if (specification.StatusEventsIncluded && result.Any())
            {
                foreach (var mappedRow in _queryMapper.MapRows<Query.Content.StatusEvent>(_queryFactory.GetStatusEvents(result.Keys)))
                {
                    if (result.TryGetValue(Columns.ContentId.MapFrom(mappedRow.Row), out var content))
                    {
                        content.StatusEvents.Add(mappedRow.Result);
                    }
                }
            }

            if (specification.PropertiesIncluded && result.Any())
            {
                foreach (var mappedRow in _queryMapper.MapRows<Query.Content.Property>(query: _queryFactory.GetProperties(result.Keys)))
                {
                    if (result.TryGetValue(Columns.ContentId.MapFrom(mappedRow.Row), out var content))
                    {
                        content.Properties.Add(mappedRow.Result);
                    }
                }
            }

            return result.Values;
        }

        public RawContent FindRawContent(Guid id)
        {
            return _queryMapper.MapObject<RawContent>(_queryFactory.FindRawContent(id));
        }
    }
}