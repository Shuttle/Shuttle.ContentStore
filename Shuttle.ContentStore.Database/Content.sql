CREATE TABLE [dbo].[Content]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED, 
	[ReferenceId] UNIQUEIDENTIFIER NOT NULL, 
    [EffectiveFromDate] DATETIME2 NOT NULL,
    [EffectiveToDate] DATETIME2 NOT NULL,
    [ContentType] VARCHAR(128) NOT NULL, 
    [FileName] VARCHAR(260) NOT NULL, 
    [SystemName] VARCHAR(128) NOT NULL, 
    [Username] VARCHAR(128) NOT NULL, 
    [Status] VARCHAR(30) NOT NULL, 
    [StatusDateRegistered] DATETIME2 NOT NULL, 
    [Bytes] VARBINARY(MAX) NOT NULL, 
    [SanitizedBytes] VARBINARY(MAX) NULL
)

GO

CREATE CLUSTERED INDEX [IX_Content_ReferenceId] ON [dbo].[Content] ([ReferenceId], [EffectiveFromDate], [EffectiveToDate])
