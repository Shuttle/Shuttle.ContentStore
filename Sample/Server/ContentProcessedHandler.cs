using System;
using Shuttle.ContentStore.Messages.v1;
using Shuttle.Esb;

namespace Server
{
    public class ContentProcessedHandler : IMessageHandler<ContentProcessedEvent>
    {
        public void ProcessMessage(IHandlerContext<ContentProcessedEvent> context)
        {
            Console.WriteLine(
                $"[content processed] : {context.Message.Id} / suspicious = '{context.Message.Suspicious}' / system name = '{context.Message.SystemName}'");
        }
    }
}