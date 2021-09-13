using System.Configuration;
using Shuttle.Core.Configuration;

namespace Shuttle.ContentStore.Application
{
    public class ContentStoreClientSection : ConfigurationSection
    {
        [ConfigurationProperty("url", IsRequired = true)]
        public string Url => (string)this["url"];

        [ConfigurationProperty("identityName", IsRequired = true)]
        public string IdentityName => (string)this["identityName"];

        [ConfigurationProperty("password", IsRequired = true)]
        public string Password => (string)this["password"];

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