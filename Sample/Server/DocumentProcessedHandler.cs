using System;
using Shuttle.ContentStore.Messages.v1;
using Shuttle.Esb;

namespace Server
{
    public class DocumentProcessedHandler : IMessageHandler<DocumentProcessedEvent>
    {
        public void ProcessMessage(IHandlerContext<DocumentProcessedEvent> context)
        {
            Console.WriteLine(
                $"[document processed] : {context.Message.Id} / suspicious = '{context.Message.Suspicious}' / system name = '{context.Message.SystemName}'");
        }
    }
}