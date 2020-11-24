using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.ContentStore.DataAccess
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IDocumentQueryFactory _queryFactory;

        public DocumentRepository(IDatabaseGateway databaseGateway, IDocumentQueryFactory queryFactory)
        {
            Guard.AgainstNull(databaseGateway, nameof(databaseGateway));
            Guard.AgainstNull(queryFactory, nameof(queryFactory));

            _databaseGateway = databaseGateway;
            _queryFactory = queryFactory;
        }

        public void Save(Document document)
        {
            Guard.AgainstNull(document, nameof(document));

            _databaseGateway.ExecuteUsing(_queryFactory.Save(document));

            SaveStatusEvents(document);
            SaveProperties(document);
        }

        public void SaveProperties(Document document)
        {
            Guard.AgainstNull(document, nameof(document));

            _databaseGateway.ExecuteUsing(_queryFactory.RemoveProperties(document.Id));

            foreach (var pair in document.GetProperties())
            {
                _databaseGateway.ExecuteUsing(_queryFactory.SaveProperty(document.Id, pair.Key, pair.Value));
            }
        }

        private void SaveStatusEvents(Document document)
        {
            _databaseGateway.ExecuteUsing(_queryFactory.RemoveStatusEvents(document.Id));

            var sequenceNumber = 1;

            foreach (var statusEvent in document.GetStatusEvents())
            {
                _databaseGateway.ExecuteUsing(_queryFactory.SaveStatusEvent(document.Id, sequenceNumber++,
                    statusEvent));
            }
        }

        public Document Get(Guid id)
        {
            var row = _databaseGateway.GetSingleRowUsing(_queryFactory.Get(id));

            row.GuardAgainstRecordNotFound<Document>(id);

            var result = new Document(
                Columns.Id.MapFrom(row), 
                Columns.ReferenceId.MapFrom(row),
                Columns.FileName.MapFrom(row),
                Columns.ContentType.MapFrom(row),
                Columns.Content.MapFrom(row),
                Columns.SystemName.MapFrom(row),
                Columns.Username.MapFrom(row),
                Columns.EffectiveFromDate.MapFrom(row)
            );

            var sanitizedContent = Columns.SanitizedContent.MapFrom(row);

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