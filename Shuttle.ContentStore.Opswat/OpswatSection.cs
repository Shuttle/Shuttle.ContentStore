﻿using System.Configuration;
using Shuttle.Core.Configuration;

namespace Shuttle.ContentStore.Opswat
{
    public class OpswatSection : ConfigurationSection
    {
        [ConfigurationProperty("apiKey", IsRequired = true)]
        public string ApiKey => (string)this["apiKey"];

        [ConfigurationProperty("apiUrl", IsRequired = true)]
        public string ApiUrl => (string)this["apiUrl"];

        [ConfigurationProperty("sandboxFileExtensions", IsRequired = false)]
        public string SandboxFileExtensions => (string)this["sandboxFileExtensions"];

        [ConfigurationProperty("sampleSharingAllowed", IsRequired = false, DefaultValue = false)]
        public bool SampleSharingAllowed => (bool)this["sampleSharingAllowed"];

        public static IOpswatConfiguration Configuration()
        {
            var section =
                ConfigurationSectionProvider.Open<OpswatSection>("shuttle", "opswat");

            if (section == null)
            {
                throw new ConfigurationErrorsException("Could not find an 'opswat' configuration section.");
            }

            var result = new OpswatConfiguration(section.ApiUrl, section.ApiKey, section.SandboxFileExtensions);

            if (section.SampleSharingAllowed)
            {
                result.AllowSampleSharing();
            }

            return result;
        }
    }
}