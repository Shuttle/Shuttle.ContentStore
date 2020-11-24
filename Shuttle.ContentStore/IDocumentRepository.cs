using System;

namespace Shuttle.ContentStore
{
    public interface IDocumentRepository
    {
        void Save(Document document);
        void SaveProperties(Document document);
        Document Get(Guid id);
    }
}