using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.ContentStore.Azure;

public static class ContentStoreBuilderExtensions
{
    extension(ContentStoreBuilder contentStoreBuilder)
    {
        /// <summary>
        /// Registers all accounts and stores from a configuration section containing <c>Accounts</c> and <c>Stores</c>
        /// children, e.g. <c>Shuttle:ContentStore:Azure</c>.
        /// </summary>
        public ContentStoreBuilder AddAzure(IConfiguration configuration)
        {
            Guard.AgainstNull(configuration);

            foreach (var section in configuration.GetSection("Accounts").GetChildren())
            {
                contentStoreBuilder.AddAzureAccount(section.Key, options => section.Bind(options));
            }

            foreach (var section in configuration.GetSection("Stores").GetChildren())
            {
                contentStoreBuilder.AddAzureStore(section.Key, options => section.Bind(options));
            }

            return contentStoreBuilder;
        }

        public ContentStoreBuilder AddAzureAccount(string name, Action<AzureStorageAccountOptions> configureOptions)
        {
            Guard.AgainstEmpty(name);
            Guard.AgainstNull(configureOptions);

            contentStoreBuilder.AddAzureServices();
            contentStoreBuilder.Services.AddOptions<AzureStorageAccountOptions>(name).Configure(configureOptions);

            return contentStoreBuilder;
        }

        public ContentStoreBuilder AddAzureStore(string name, Action<AzureContentStoreOptions> configureOptions)
        {
            Guard.AgainstEmpty(name);
            Guard.AgainstNull(configureOptions);

            contentStoreBuilder.AddAzureServices();

            contentStoreBuilder.Services
                .AddOptions<AzureContentStoreOptions>(name)
                .Configure(configureOptions)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            return contentStoreBuilder.AddContentStore(name, (serviceProvider, storeName) =>
            {
                var options = serviceProvider.GetRequiredService<IOptionsMonitor<AzureContentStoreOptions>>().Get(storeName);
                var blobContainerClient = serviceProvider.GetRequiredService<IAzureBlobServiceClientProvider>()
                    .Get(options.Account)
                    .GetBlobContainerClient(options.ContainerName);

                return new AzureContentStore(storeName, options, blobContainerClient);
            });
        }

        /// <summary>
        /// Registers an account and a store, both with the given name, for the common single-store case.
        /// </summary>
        public ContentStoreBuilder AddAzureStore(string name, Action<AzureStorageAccountOptions> configureAccountOptions, Action<AzureContentStoreOptions> configureOptions)
        {
            Guard.AgainstNull(configureOptions);

            return contentStoreBuilder
                .AddAzureAccount(name, configureAccountOptions)
                .AddAzureStore(name, options =>
                {
                    options.Account = name;
                    configureOptions(options);
                });
        }

        private void AddAzureServices()
        {
            var services = contentStoreBuilder.Services;

            services.TryAddSingleton<IAzureBlobServiceClientProvider, AzureBlobServiceClientProvider>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<AzureStorageAccountOptions>, AzureStorageAccountOptionsValidator>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<AzureContentStoreOptions>, AzureContentStoreOptionsValidator>());
        }
    }
}
