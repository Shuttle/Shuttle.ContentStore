using System.Configuration;
using Shuttle.Core.Configuration;

namespace Shuttle.ContentStore.Application
{
    public class ContentStoreClientSection : ConfigurationSection
    {
        [ConfigurationProperty("url", IsRequired = true)]
        public string Url => (string)this["url"];

        public static IContentStoreClientConfiguration Configuration()
        {
            var section =
                ConfigurationSectionProvider.Open<ContentStoreClientSection>("shuttle", "contentStore");

            if (section == null)
            {
                throw new ConfigurationErrorsException("Could not find an 'contentStore' configuration section.");
            }

            return new ContentStoreClientConfiguration(section.Url);
        }
    }
}