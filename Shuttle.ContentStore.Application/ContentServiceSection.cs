using System.Configuration;
using Shuttle.Core.Configuration;

namespace Shuttle.ContentStore.Application
{
    public class ContentServiceSection : ConfigurationSection
    {
        [ConfigurationProperty("apiUrl", IsRequired = true)]
        public string ApiUrl => (string)this["apiUrl"];

        public static IContentServiceConfiguration Configuration()
        {
            var section =
                ConfigurationSectionProvider.Open<ContentServiceSection>("shuttle", "contentService");

            if (section == null)
            {
                throw new ConfigurationErrorsException("Could not find an 'contentService' configuration section.");
            }

            return new ContentServiceConfiguration(section.ApiUrl);
        }
    }
}