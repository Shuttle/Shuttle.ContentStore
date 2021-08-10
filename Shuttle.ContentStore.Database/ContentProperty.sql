CREATE TABLE [dbo].[ContentProperty]
(
	[ContentId] UNIQUEIDENTIFIER NOT NULL , 
    [Name] VARCHAR(130) NOT NULL, 
    [Value] VARCHAR(260) NULL, 
    PRIMARY KEY ([ContentId], [Name]), 
    CONSTRAINT [FK_ContentProperty_Content] FOREIGN KEY ([ContentId]) REFERENCES [dbo].[Content]([Id])
)
