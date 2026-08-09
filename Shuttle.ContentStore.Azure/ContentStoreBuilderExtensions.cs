using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Shuttle.ContentStore.Azure;

public static class ContentStoreBuilderExtensions
{
    extension(ContentStoreBuilder contentStoreBuilder)
    {
        public ContentStoreBuilder UseAzure(Action<AzureContentStoreOptions> configureOptions)
        {
            var services = contentStoreBuilder.Services;

            services
                .AddOptions<AzureContentStoreOptions>()
                .Configure(configureOptions)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<IValidateOptions<AzureContentStoreOptions>, AzureContentStoreOptionsValidator>();

            contentStoreBuilder.AddContentStore<AzureContentStore>(AzureContentStore.StoreName);

            return contentStoreBuilder;
        }
    }
}