using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.ContentStore.DataAccess
{
    public class ContentRepository : IContentRepository
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IContentQueryFactory _queryFactory;

        public ContentRepository(IDatabaseGateway databaseGateway, IContentQueryFactory queryFactory)
        {
            Guard.AgainstNull(databaseGateway, nameof(databaseGateway));
            Guard.AgainstNull(queryFactory, nameof(queryFactory));

            _databaseGateway = databaseGateway;
            _queryFactory = queryFactory;
        }

        public void Save(Content content)
        {
            Guard.AgainstNull(content, nameof(content));

            _databaseGateway.ExecuteUsing(_queryFactory.Save(content));

            SaveStatusEvents(content);
            SaveProperties(content);
        }

        public void SaveProperties(Content content)
        {
            Guard.AgainstNull(content, nameof(content));

            _databaseGateway.ExecuteUsing(_queryFactory.RemoveProperties(content.Id));

            foreach (var pair in content.GetProperties())
            {
                _databaseGateway.ExecuteUsing(_queryFactory.SaveProperty(content.Id, pair.Key, pair.Value));
            }
        }

        private void SaveStatusEvents(Content content)
        {
            _databaseGateway.ExecuteUsing(_queryFactory.RemoveStatusEvents(content.Id));

            var sequenceNumber = 1;

            foreach (var statusEvent in content.GetStatusEvents())
            {
                _databaseGateway.ExecuteUsing(_queryFactory.SaveStatusEvent(content.Id, sequenceNumber++,
                    statusEvent));
            }
        }

        public Content Get(Guid id)
        {
            var row = _databaseGateway.GetSingleRowUsing(_queryFactory.Get(id));

            row.GuardAgainstRecordNotFound<Content>(id);

            var result = new Content(
                Columns.Id.MapFrom(row), 
                Columns.ReferenceId.MapFrom(row),
                Columns.FileName.MapFrom(row),
                Columns.ContentType.MapFrom(row),
                Columns.Bytes.MapFrom(row),
                Columns.SystemName.MapFrom(row),
                Columns.Username.MapFrom(row),
                Columns.EffectiveFromDate.MapFrom(row)
            );

            var sanitizedContent = Columns.SanitizedBytes.MapFrom(row);

            if (sanitizedContent != null && sanitizedContent.Length > 0)
            {
                result.WithSanitizedContent(sanitizedContent);
            }

            foreach (var statusEventRow in _databaseGateway.GetRowsUsing(_queryFactory.GetStatusEvents(id)))
            {
                result.OnStatusEvent(
                    (ServiceStatus)Enum.Parse(typeof(ServiceStatus), Columns.Status.MapFrom(statusEventRow)),
                    Columns.DateRegistered.MapFrom(statusEventRow));
            }

            foreach (var propertyRow in _databaseGateway.GetRowsUsing(_queryFactory.GetProperties(id)))
            {
                result.SetProperty(Columns.Name.MapFrom(propertyRow), Columns.Value.MapFrom(propertyRow));
            }

            return result;
        }
    }
}