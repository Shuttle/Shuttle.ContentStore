# Shuttle.ContentStore

Simple content store abstraction.

## Installation

```bash
dotnet add package Shuttle.ContentStore
```

## Configuration

Register a content store implementation using the `AddContentStore()` builder:

```c#
services.AddContentStore()
    .UseAzure(options =>
    {
        options.ConnectionString = "UseDevelopmentStorage=true";
        options.ContainerName = "content-store";
    });
```

The Azure implementation requires the [`Shuttle.ContentStore.Azure`](https://www.nuget.org/packages/Shuttle.ContentStore.Azure) package, and can be tested locally against the [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite) emulator.

Once registered, resolve `IContentStore` (or a specific keyed store, e.g. `[FromKeyedServices("azure")]`) to read and write content:

```c#
await contentStore.PutAsync("path/to/file.txt", stream);
await using var content = await contentStore.OpenReadAsync("path/to/file.txt");
```