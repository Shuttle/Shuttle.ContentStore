using System;
using System.IO;
using Shuttle.ContentStore.Opswat;
using NUnit.Framework;
using Shuttle.Core.Configuration;

namespace Shuttle.ContentStore.Tests.Integration.Opswat
{
    [TestFixture]
    public class OpswatSectionFixture
    {
        [Test]
        public void Should_be_able_to_load_the_configuration()
        {
            var section = ConfigurationSectionProvider.OpenFile<OpswatSection>("shuttle", "opswat",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".config\\Opswat.config"));

            Assert.That(section, Is.Not.Null);
            Assert.That(section.ApiKey, Is.EqualTo("api-key"));
            Assert.That(section.ApiUrl, Is.EqualTo("api-url"));
        }

	}
}