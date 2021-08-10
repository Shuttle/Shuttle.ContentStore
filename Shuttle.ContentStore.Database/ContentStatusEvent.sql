CREATE TABLE [dbo].[ContentStatusEvent]
(
	[ContentId] UNIQUEIDENTIFIER NOT NULL , 
    [SequenceNumber] INT NOT NULL, 
    [Status] VARCHAR(30) NOT NULL, 
    [DateRegistered] DATETIME2 NOT NULL, 
    PRIMARY KEY ([ContentId], [SequenceNumber]), 
    CONSTRAINT [FK_ContentStatusEvent_Content] FOREIGN KEY ([ContentId]) REFERENCES [dbo].[Content]([Id])
)
