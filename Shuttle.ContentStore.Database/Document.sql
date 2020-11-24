CREATE TABLE [dbo].[Document]
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
    [Content] VARBINARY(MAX) NOT NULL, 
    [SanitizedContent] VARBINARY(MAX) NULL
)

GO

CREATE CLUSTERED INDEX [IX_Document_ReferenceId] ON [dbo].[Document] ([ReferenceId], [EffectiveFromDate], [EffectiveToDate])
