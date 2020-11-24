using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.ContentStore.DataAccess
{
    public class DocumentQueryFactory : IDocumentQueryFactory
    {
        public IQuery Save(Document document)
        {
            Guard.AgainstNull(document, nameof(document));

            return RawQuery.Create(@"
if exists
(
    select
        null
    from
        dbo.Document
    where
        Id = @Id
)
    update
        dbo.Document
    set
        EffectiveToDate = @EffectiveToDate,
        Status = @Status,
        StatusDateRegistered = @StatusDateRegistered,
        SanitizedContent = @SanitizedContent
    where
        Id = @Id
else
    begin
        update
            dbo.Document
        set
            EffectiveToDate = @EffectiveFromDate
        where
            ReferenceId = @ReferenceId
        and
            EffectiveToDate = @EffectiveToDate;

        insert into
            dbo.Document
            (
                Id,
                ReferenceId,
                EffectiveFromDate,
                EffectiveToDate,
                FileName,
                ContentType,
                Content,
                SystemName,
                Username,
                Status,
                StatusDateRegistered
            )
        values
            (
                @Id,
                @ReferenceId,
                @EffectiveFromDate,
                @EffectiveToDate,
                @FileName,
                @ContentType,
                @Content,
                @SystemName,
                @Username,
                @Status,
                @StatusDateRegistered
            );
    end
")
                .AddParameterValue(Columns.Id, document.Id)
                .AddParameterValue(Columns.ReferenceId, document.ReferenceId)
                .AddParameterValue(Columns.EffectiveFromDate, document.EffectiveFromDate)
                .AddParameterValue(Columns.EffectiveToDate, document.EffectiveToDate)
                .AddParameterValue(Columns.FileName, document.FileName)
                .AddParameterValue(Columns.ContentType, document.ContentType)
                .AddParameterValue(Columns.Content, document.Content)
                .AddParameterValue(Columns.SystemName, document.SystemName)
                .AddParameterValue(Columns.Username, document.Username)
                .AddParameterValue(Columns.Status, document.Status)
                .AddParameterValue(Columns.StatusDateRegistered, document.StatusDateRegistered)
                .AddParameterValue(Columns.SanitizedContent,
                    document.HasSanitizedContent ? document.SanitizedContent : null);
        }

        public IQuery RemoveStatusEvents(Guid documentId)
        {
            return RawQuery.Create(@"
delete
from
    dbo.DocumentStatusEvent
where
    DocumentId = @Id
")
                .AddParameterValue(Columns.Id, documentId);
        }

        public IQuery SaveStatusEvent(Guid documentId, int sequenceNumber, Document.StatusEvent statusEvent)
        {
            return RawQuery.Create(@"
insert into
    dbo.DocumentStatusEvent
    (
        DocumentId,
        SequenceNumber,
        Status,
        DateRegistered
    )
values
    (
        @DocumentId,
        @SequenceNumber,
        @Status,
        @DateRegistered
    )
")
                .AddParameterValue(Columns.DocumentId, documentId)
                .AddParameterValue(Columns.SequenceNumber, sequenceNumber)
                .AddParameterValue(Columns.Status, statusEvent.Status)
                .AddParameterValue(Columns.DateRegistered, statusEvent.DateRegistered);
        }

        public IQuery Get(Guid id)
        {
            return RawQuery.Create(@"
select
	Id,
    ReferenceId,
    EffectiveFromDate,
    EffectiveToDate,
	ContentType,
	FileName,
	SystemName,
	Username,
	Status,
	StatusDateRegistered,
	Content,
	SanitizedContent
from
	Document
where
	Id = @Id
")
                .AddParameterValue(Columns.Id, id);
        }

        public IQuery GetStatusEvents(Guid id)
        {
            return RawQuery.Create(@"
select
	DocumentId,
	SequenceNumber,
	Status,
	DateRegistered
from
	DocumentStatusEvent
where
	DocumentId = @DocumentId
order by
    SequenceNumber
")
                .AddParameterValue(Columns.DocumentId, id);
        }

        public IQuery RemoveProperties(Guid documentId)
        {
            return RawQuery.Create(@"
delete
from
    dbo.DocumentProperty
where
    DocumentId = @Id
")
                .AddParameterValue(Columns.Id, documentId);
        }

        public IQuery SaveProperty(Guid documentId, string name, string value)
        {
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            return RawQuery.Create(@"
insert into dbo.DocumentProperty
    (
        DocumentId,
        Name,
        Value
    )
values
    (
        @DocumentId,
        @Name,
        @Value
    )
")
                .AddParameterValue(Columns.DocumentId, documentId)
                .AddParameterValue(Columns.Name, name)
                .AddParameterValue(Columns.Value, value);
        }

        public IQuery GetProperties(Guid id)
        {
            return RawQuery.Create(@"
select
    DocumentId,
    Name,
    Value
from
    dbo.DocumentProperty
where
    DocumentId = @DocumentId
")
                .AddParameterValue(Columns.DocumentId, id);
        }

        public IQuery Search(Query.Document.Specification specification)
        {
            Guard.AgainstNull(specification, nameof(specification));

            var ids = string.Join(",", specification.GetIds().Select(item => $"'{item}'"));

            return RawQuery.Create($@"
select {(specification.MaximumRows > 0 ? $"top {specification.MaximumRows}" : string.Empty)}
	Id,
    ReferenceId,
    EffectiveFromDate,
    EffectiveToDate,
	ContentType,
	FileName,
	SystemName,
	Username,
	Status,
	StatusDateRegistered
from
	Document
where
    1 = 1
{(specification.HasIds ? $@"
and 
(
    Id in ({ids})
    or
    (
        ReferenceId in ({ids}) {(specification.ActiveOnly ? "and EffectiveToDate = @EffectiveToDate" : string.Empty)}
    )
)
" : string.Empty)}
order by
    EffectiveFromDate desc
")
                .AddParameterValue(Columns.EffectiveToDate, DateTime.MaxValue);
        }

        public IQuery GetStatusEvents(IEnumerable<Guid> documentIds)
        {
            Guard.AgainstNull(documentIds, nameof(documentIds));

            var ids = string.Join(",", documentIds.Select(id => $"'{id}'"));

            if (string.IsNullOrEmpty(ids))
            {
                throw new ArgumentException($"Argument '{nameof(documentIds)}' may not be empty.");
            }

            return RawQuery.Create($@"
select
	DocumentId,
	SequenceNumber,
	Status,
	DateRegistered
from
	DocumentStatusEvent
where
	DocumentId in ({ids})
order by
    DocumentId,
    SequenceNumber
");
        }

        public IQuery GetProperties(IEnumerable<Guid> documentIds)
        {
            Guard.AgainstNull(documentIds, nameof(documentIds));

            var ids = string.Join(",", documentIds.Select(id => $"'{id}'"));

            if (string.IsNullOrEmpty(ids))
            {
                throw new ArgumentException($"Argument '{nameof(documentIds)}' may not be empty.");
            }

            return RawQuery.Create($@"
select
	DocumentId,
	Name,
	Value
from
	DocumentProperty
where
	DocumentId in ({ids})
order by
    Name
");
        }

        public IQuery FindContent(Guid id)
        {
            return RawQuery.Create(@"
select
	[Status],
	case
		when [Status] = 'Cleared' then
			Content
		when [Status] = 'Suspicious' then
			SanitizedContent
		else
			null
	end Content,
    ContentType,
    FileName
from
	[dbo].[Document]
where
    Id = @Id
or
    (
        ReferenceId = @ReferenceId
    and
        EffectiveToDate = @EffectiveToDate
    )
")
                .AddParameterValue(Columns.Id, id)
                .AddParameterValue(Columns.ReferenceId, id)
                .AddParameterValue(Columns.EffectiveToDate, DateTime.MaxValue);
        }
    }
}