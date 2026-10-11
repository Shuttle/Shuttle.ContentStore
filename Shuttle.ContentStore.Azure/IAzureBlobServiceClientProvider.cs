using Azure.Storage.Blobs;

namespace Shuttle.ContentStore.Azure;

public interface IAzureBlobServiceClientProvider
{
    BlobServiceClient Get(string account);
}
