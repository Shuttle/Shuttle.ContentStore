CREATE TABLE [dbo].[DocumentStatusEvent]
(
	[DocumentId] UNIQUEIDENTIFIER NOT NULL , 
    [SequenceNumber] INT NOT NULL, 
    [Status] VARCHAR(30) NOT NULL, 
    [DateRegistered] DATETIME2 NOT NULL, 
    PRIMARY KEY ([DocumentId], [SequenceNumber]), 
    CONSTRAINT [FK_DocumentStatusEvent_Document] FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Document]([Id])
)
