CREATE TABLE [dbo].[DocumentProperty]
(
	[DocumentId] UNIQUEIDENTIFIER NOT NULL , 
    [Name] VARCHAR(130) NOT NULL, 
    [Value] VARCHAR(260) NULL, 
    PRIMARY KEY ([DocumentId], [Name]), 
    CONSTRAINT [FK_DocumentProperty_Document] FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Document]([Id])
)
