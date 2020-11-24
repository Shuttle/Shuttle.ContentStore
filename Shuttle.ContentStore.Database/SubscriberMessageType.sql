CREATE TABLE [dbo].[SubscriberMessageType]
(
	[MessageType] VARCHAR(250) NOT NULL , 
    [InboxWorkQueueUri] VARCHAR(130) NOT NULL, 
    PRIMARY KEY ([MessageType], [InboxWorkQueueUri])
)
