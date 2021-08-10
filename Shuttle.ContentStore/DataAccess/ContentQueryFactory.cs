using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.ContentStore.DataAccess
{
    public class ContentQueryFactory : IContentQueryFactory
    {
        public IQuery Save(Content content)
        {
            Guard.AgainstNull(content, nameof(content));

            return RawQuery.Create(@"
if exists
(
    select
        null
    from
        dbo.Content
    where
        Id = @Id
)
    update
        dbo.Content
    set
        EffectiveToDate = @EffectiveToDate,
        Status = @Status,
        StatusDateRegistered = @StatusDateRegistered,
        SanitizedBytes = @SanitizedBytes
    where
        Id = @Id
else
    begin
        update
            dbo.Content
        set
            EffectiveToDate = @EffectiveFromDate
        where
            ReferenceId = @ReferenceId
        and
            EffectiveToDate = @EffectiveToDate;

        insert into
            dbo.Content
            (
                Id,
                ReferenceId,
                EffectiveFromDate,
                EffectiveToDate,
                FileName,
                ContentType,
                Bytes,
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
                @Bytes,
                @SystemName,
                @Username,
                @Status,
                @StatusDateRegistered
            );
    end
")
                .AddParameterValue(Columns.Id, content.Id)
                .AddParameterValue(Columns.ReferenceId, content.ReferenceId)
                .AddParameterValue(Columns.EffectiveFromDate, content.EffectiveFromDate)
                .AddParameterValue(Columns.EffectiveToDate, content.EffectiveToDate)
                .AddParameterValue(Columns.FileName, content.FileName)
                .AddParameterValue(Columns.ContentType, content.ContentType)
                .AddParameterValue(Columns.Bytes, content.Bytes)
                .AddParameterValue(Columns.SystemName, content.SystemName)
                .AddParameterValue(Columns.Username, content.Username)
                .AddParameterValue(Columns.Status, content.Status)
                .AddParameterValue(Columns.StatusDateRegistered, content.StatusDateRegistered)
                .AddParameterValue(Columns.SanitizedBytes,
                    content.HasSanitizedBytes ? content.SanitizedBytes : null);
        }

        public IQuery RemoveStatusEvents(Guid contentId)
        {
            return RawQuery.Create(@"
delete
from
    dbo.ContentStatusEvent
where
    ContentId = @Id
")
                .AddParameterValue(Columns.Id, contentId);
        }

        public IQuery SaveStatusEvent(Guid contentId, int sequenceNumber, Content.StatusEvent statusEvent)
        {
            return RawQuery.Create(@"
insert into
    dbo.ContentStatusEvent
    (
        ContentId,
        SequenceNumber,
        Status,
        DateRegistered
    )
values
    (
        @ContentId,
        @SequenceNumber,
        @Status,
        @DateRegistered
    )
")
                .AddParameterValue(Columns.ContentId, contentId)
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
	Bytes,
	SanitizedBytes
from
	Content
where
	Id = @Id
")
                .AddParameterValue(Columns.Id, id);
        }

        public IQuery GetStatusEvents(Guid id)
        {
            return RawQuery.Create(@"
select
	ContentId,
	SequenceNumber,
	Status,
	DateRegistered
from
	ContentStatusEvent
where
	ContentId = @ContentId
order by
    SequenceNumber
")
                .AddParameterValue(Columns.ContentId, id);
        }

        public IQuery RemoveProperties(Guid contentId)
        {
            return RawQuery.Create(@"
delete
from
    dbo.ContentProperty
where
    ContentId = @Id
")
                .AddParameterValue(Columns.Id, contentId);
        }

        public IQuery SaveProperty(Guid contentId, string name, string value)
        {
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            return RawQuery.Create(@"
insert into dbo.ContentProperty
    (
        ContentId,
        Name,
        Value
    )
values
    (
        @ContentId,
        @Name,
        @Value
    )
")
                .AddParameterValue(Columns.ContentId, contentId)
                .AddParameterValue(Columns.Name, name)
                .AddParameterValue(Columns.Value, value);
        }

        public IQuery GetProperties(Guid id)
        {
            return RawQuery.Create(@"
select
    ContentId,
    Name,
    Value
from
    dbo.ContentProperty
where
    ContentId = @ContentId
")
                .AddParameterValue(Columns.ContentId, id);
        }

        public IQuery Search(Query.Content.Specification specification)
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
	Content
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

        public IQuery GetStatusEvents(IEnumerable<Guid> contentIds)
        {
            Guard.AgainstNull(contentIds, nameof(contentIds));

            var ids = string.Join(",", contentIds.Select(id => $"'{id}'"));

            if (string.IsNullOrEmpty(ids))
            {
                throw new ArgumentException($"Argument '{nameof(contentIds)}' may not be empty.");
            }

            return RawQuery.Create($@"
select
	ContentId,
	SequenceNumber,
	Status,
	DateRegistered
from
	ContentStatusEvent
where
	ContentId in ({ids})
order by
    ContentId,
    SequenceNumber
");
        }

        public IQuery GetProperties(IEnumerable<Guid> contentIds)
        {
            Guard.AgainstNull(contentIds, nameof(contentIds));

            var ids = string.Join(",", contentIds.Select(id => $"'{id}'"));

            if (string.IsNullOrEmpty(ids))
            {
                throw new ArgumentException($"Argument '{nameof(contentIds)}' may not be empty.");
            }

            return RawQuery.Create($@"
select
	ContentId,
	Name,
	Value
from
	ContentProperty
where
	ContentId in ({ids})
order by
    Name
");
        }

        public IQuery FindRawContent(Guid id)
        {
            return RawQuery.Create(@"
select
	[Status],
	case
		when [Status] = 'Passed' then
			Bytes
		when [Status] = 'Suspicious' then
			SanitizedBytes
		else
			null
	end Bytes,
    ContentType,
    FileName
from
	[dbo].[Content]
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