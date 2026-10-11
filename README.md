# Shuttle.ContentStore

Simple content store abstraction that supports any number of named stores, each backed by a provider such as Azure Blob Storage, Amazon S3, or the file system.

## Installation

```bash
dotnet add package Shuttle.ContentStore
```

Add the package for each provider in use:

| Package | Provider |
| --- | --- |
| [`Shuttle.ContentStore.Azure`](https://www.nuget.org/packages/Shuttle.ContentStore.Azure) | Azure Blob Storage |
| [`Shuttle.ContentStore.AmazonS3`](https://www.nuget.org/packages/Shuttle.ContentStore.AmazonS3) | Amazon S3 (and S3-compatible services such as MinIO) |
| [`Shuttle.ContentStore.FileSystem`](https://www.nuget.org/packages/Shuttle.ContentStore.FileSystem) | Local or network file system |

## Concepts

- **Store**: a logical, named content store (e.g. `documents`, `invoices`, `archive`).  Store names must be unique across all providers.  Consumers only ever deal with store names.
- **Account**: the connection details (endpoint and credentials) for a cloud provider.  Many stores may share one account, in which case they also share a single underlying client.

## Configuration

```c#
services.AddContentStore()
    .AddAzure(configuration.GetSection(AzureContentStoreOptions.SectionName))
    .AddAmazonS3(configuration.GetSection(AmazonS3ContentStoreOptions.SectionName))
    .AddFileSystem(configuration.GetSection(FileSystemContentStoreOptions.SectionName))
    .SetDefault("documents");
```

```json
{
  "Shuttle": {
    "ContentStore": {
      "Azure": {
        "Accounts": {
          "primary": { "StorageAccount": "acmeprod" },
          "archive": { "StorageAccount": "acmearchive", "AccountKey": "..." },
          "local": { "ConnectionString": "UseDevelopmentStorage=true" }
        },
        "Stores": {
          "documents": { "Account": "primary", "ContainerName": "documents" },
          "invoices": { "Account": "primary", "ContainerName": "finance", "Prefix": "invoices" },
          "archive": { "Account": "archive", "ContainerName": "cold-storage", "CreateContainerIfNotExists": false }
        }
      },
      "AmazonS3": {
        "Accounts": {
          "aws": { "Region": "eu-west-1" },
          "local": { "ServiceUrl": "http://localhost:4566", "ForcePathStyle": true, "AccessKey": "...", "SecretKey": "..." }
        },
        "Stores": {
          "exports": { "Account": "aws", "BucketName": "acme-exports", "Prefix": "daily" }
        }
      },
      "FileSystem": {
        "Stores": {
          "scratch": { "RootPath": "D:\\content\\scratch" }
        }
      }
    }
  }
}
```

Accounts and stores may also be registered in code:

```c#
services.AddContentStore()
    .AddAzureAccount("primary", options => options.StorageAccount = "acmeprod")
    .AddAzureStore("documents", options =>
    {
        options.Account = "primary";
        options.ContainerName = "documents";
    })
    .AddAzureStore("invoices", options =>
    {
        options.Account = "primary";
        options.ContainerName = "finance";
        options.Prefix = "invoices";
    })
    .AddFileSystemStore("scratch", options => options.RootPath = "scratch")
    .SetDefault("documents");
```

For a single store, the account and store can be registered together (both use the store name):

```c#
services.AddContentStore()
    .AddAzureStore("documents",
        account => account.ConnectionString = "UseDevelopmentStorage=true",
        store => store.ContainerName = "documents");
```

### Default store

An unkeyed `IContentStore` resolves the default store, which is the store specified by `SetDefault` (or `ContentStoreOptions.DefaultStore`).  When no default is specified and exactly one store is registered, that store is used.  With multiple stores and no default, resolving `IContentStore` throws.

### Azure

| Option | Description |
| --- | --- |
| `Accounts:{name}:StorageAccount` | Storage account name; uses `https://{StorageAccount}.blob.core.windows.net`. |
| `Accounts:{name}:AccountKey` | Shared key (requires `StorageAccount`).  When omitted, `TokenCredential` (code only) or a shared `DefaultAzureCredential` is used. |
| `Accounts:{name}:ServiceUri` | Overrides the blob endpoint, e.g. for sovereign clouds. |
| `Accounts:{name}:ConnectionString` | Used when neither `StorageAccount` nor `ServiceUri` is set. |
| `Stores:{name}:Account` | Required; the account to use. |
| `Stores:{name}:ContainerName` | Required. |
| `Stores:{name}:Prefix` | Optional blob name prefix. |
| `Stores:{name}:CreateContainerIfNotExists` | Defaults to `true`. |

### Amazon S3

| Option | Description |
| --- | --- |
| `Accounts:{name}:Region` | AWS region system name, e.g. `eu-west-1`.  Used as the authentication region when `ServiceUrl` is set. |
| `Accounts:{name}:ServiceUrl` | Custom endpoint for S3-compatible services. |
| `Accounts:{name}:ForcePathStyle` | Use path-style addressing (typically required for emulators such as LocalStack or MinIO). |
| `Accounts:{name}:AccessKey` / `SecretKey` / `SessionToken` | Explicit credentials.  Otherwise the named `Profile`, `Credentials` (code only), or the default AWS credential chain is used. |
| `Stores:{name}:Account` | Required; the account to use. |
| `Stores:{name}:BucketName` | Required. |
| `Stores:{name}:Prefix` | Optional object key prefix. |
| `Stores:{name}:CreateBucketIfNotExists` | Defaults to `false`. |

Non-seekable streams passed to `PutAsync` are buffered in memory since S3 requires a known content length.

### File system

| Option | Description |
| --- | --- |
| `Stores:{name}:RootPath` | Required; relative paths are resolved against `AppContext.BaseDirectory`. |

Keys may contain `/` to create sub-folders; keys that resolve outside of `RootPath` are rejected.

## Usage

```c#
public class InvoiceService([FromKeyedServices("invoices")] IContentStore invoices) { }

public class ArchiveJob(IContentStoreProvider contentStoreProvider)
{
    public Task ExecuteAsync(string storeName) => contentStoreProvider.Get(storeName).DeleteAsync("old.txt");
}

public class DocumentService(IContentStore contentStore)  // default store
{
    public async Task ExecuteAsync(Stream stream)
    {
        await contentStore.PutAsync("path/to/file.txt", stream);
        await using var content = await contentStore.OpenReadAsync("path/to/file.txt");
    }
}
```

`OpenReadAsync` throws a `FileNotFoundException` when the key does not exist, for all providers.

## Testing

The Azure and Amazon S3 fixtures run against [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) and [LocalStack](https://www.localstack.cloud/) (the last community release, `4.9`, since later images require a licence token) respectively, which may be started using:

```bash
docker compose up -d
```

The Amazon S3 fixture is ignored when LocalStack is not available on `localhost:4566`.
