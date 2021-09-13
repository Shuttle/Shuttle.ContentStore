using System;

namespace Shuttle.ContentStore.Application
{
    public interface IContentStoreClientConfiguration
    {
        Uri Url { get; }
    }
}