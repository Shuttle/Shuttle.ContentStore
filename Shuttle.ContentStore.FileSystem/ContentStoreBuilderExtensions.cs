using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.ContentStore.FileSystem;

public static class ContentStoreBuilderExtensions
{
    extension(ContentStoreBuilder contentStoreBuilder)
    {
        /// <summary>
        /// Registers all stores from a configuration section containing <c>Stores</c> children, e.g. <c>Shuttle:ContentStore:FileSystem</c>.
        /// </summary>
        public ContentStoreBuilder AddFileSystem(IConfiguration configuration)
        {
            Guard.AgainstNull(configuration);

            foreach (var section in configuration.GetSection("Stores").GetChildren())
            {
                contentStoreBuilder.AddFileSystemStore(section.Key, options => section.Bind(options));
            }

            return contentStoreBuilder;
        }

        public ContentStoreBuilder AddFileSystemStore(string name, Action<FileSystemContentStoreOptions> configureOptions)
        {
            Guard.AgainstEmpty(name);
            Guard.AgainstNull(configureOptions);

            contentStoreBuilder.Services
                .AddOptions<FileSystemContentStoreOptions>(name)
                .Configure(configureOptions)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            return contentStoreBuilder.AddContentStore(name, (serviceProvider, storeName) =>
                new FileSystemContentStore(storeName, serviceProvider.GetRequiredService<IOptionsMonitor<FileSystemContentStoreOptions>>().Get(storeName)));
        }
    }
}
