using System;

namespace Shuttle.ContentStore.Messages.v1
{
    public class DocumentProcessedEvent
    {
        public Guid Id { get; set; }
        public Guid ReferenceId { get; set; }
        public string SystemName { get; set; }
        public bool Suspicious { get; set; }
    }
}