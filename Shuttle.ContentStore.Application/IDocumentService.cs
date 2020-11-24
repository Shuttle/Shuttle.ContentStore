using System;

namespace Shuttle.ContentStore.Application
{
    public interface IDocumentService
    {
        Guid Register(Guid referenceId, string fileName, string contentType, byte[] content,
            string systemName, string username, DateTime effectiveFromDate);
    }
}