using System;

namespace Shuttle.ContentStore
{
    public interface IContentRepository
    {
        void Save(Content content);
        void SaveProperties(Content content);
        Content Get(Guid id);
    }
}