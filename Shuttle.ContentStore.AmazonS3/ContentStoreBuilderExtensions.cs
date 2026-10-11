using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.ContentStore.AmazonS3;

public static class ContentStoreBuilderExtensions
{
    extension(ContentStoreBuilder contentStoreBuilder)
    {
        /// <summary>
        /// Registers all accounts and stores from a configuration section containing <c>Accounts</c> and <c>Stores</c>
        /// children, e.g. <c>Shuttle:ContentStore:AmazonS3</c>.
        /// </summary>
        public ContentStoreBuilder AddAmazonS3(IConfiguration configuration)
        {
            Guard.AgainstNull(configuration);

            foreach (var section in configuration.GetSection("Accounts").GetChildren())
            {
                contentStoreBuilder.AddAmazonS3Account(section.Key, options => section.Bind(options));
            }

            foreach (var section in configuration.GetSection("Stores").GetChildren())
            {
                contentStoreBuilder.AddAmazonS3Store(section.Key, options => section.Bind(options));
            }

            return contentStoreBuilder;
        }

        public ContentStoreBuilder AddAmazonS3Account(string name, Action<AmazonS3AccountOptions> configureOptions)
        {
            Guard.AgainstEmpty(name);
            Guard.AgainstNull(configureOptions);

            contentStoreBuilder.AddAmazonS3Services();
            contentStoreBuilder.Services.AddOptions<AmazonS3AccountOptions>(name).Configure(configureOptions);

            return contentStoreBuilder;
        }

        public ContentStoreBuilder AddAmazonS3Store(string name, Action<AmazonS3ContentStoreOptions> configureOptions)
        {
            Guard.AgainstEmpty(name);
            Guard.AgainstNull(configureOptions);

            contentStoreBuilder.AddAmazonS3Services();

            contentStoreBuilder.Services
                .AddOptions<AmazonS3ContentStoreOptions>(name)
                .Configure(configureOptions)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            return contentStoreBuilder.AddContentStore(name, (serviceProvider, storeName) =>
            {
                var options = serviceProvider.GetRequiredService<IOptionsMonitor<AmazonS3ContentStoreOptions>>().Get(storeName);

                return new AmazonS3ContentStore(storeName, options, serviceProvider.GetRequiredService<IAmazonS3ClientProvider>().Get(options.Account));
            });
        }

        /// <summary>
        /// Registers an account and a store, both with the given name, for the common single-store case.
        /// </summary>
        public ContentStoreBuilder AddAmazonS3Store(string name, Action<AmazonS3AccountOptions> configureAccountOptions, Action<AmazonS3ContentStoreOptions> configureOptions)
        {
            Guard.AgainstNull(configureOptions);

            return contentStoreBuilder
                .AddAmazonS3Account(name, configureAccountOptions)
                .AddAmazonS3Store(name, options =>
                {
                    options.Account = name;
                    configureOptions(options);
                });
        }

        private void AddAmazonS3Services()
        {
            var services = contentStoreBuilder.Services;

            services.TryAddSingleton<IAmazonS3ClientProvider, AmazonS3ClientProvider>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<AmazonS3AccountOptions>, AmazonS3AccountOptionsValidator>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<AmazonS3ContentStoreOptions>, AmazonS3ContentStoreOptionsValidator>());
        }
    }
}
